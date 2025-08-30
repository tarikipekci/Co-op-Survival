using Interface;
using Manager;
using Player;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using Weapon;

namespace Enemy
{
    [RequireComponent(typeof(NetworkObject))]
    public class EnemyController : NetworkBehaviour
    {
        private EnemyModel model;
        private EnemyView view;
        private IEnemyBehavior behavior;
        private NavMeshAgent agent;
        private float lastAttackTime;
        private Vector3 lastSentDirection;
        private PlayerController cachedTarget;
        private float targetUpdateTime;
        private const float targetUpdateInterval = 0.3f;
        private const float directionThreshold = 0.25f;
        private Vector3 lastTargetPos;

        public NetworkVariable<Vector2> NetworkDirection = new NetworkVariable<Vector2>(Vector2.zero);

        public override void OnNetworkSpawn()
        {
            view = GetComponent<EnemyView>();
            if (IsClient)
            {
                NetworkDirection.OnValueChanged += OnDirectionChanged;
            }
        }

        public void Initialize(EnemyModel modelData, IEnemyBehavior behaviorLogic)
        {
            model = modelData;
            behavior = behaviorLogic;
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = model.Speed;
        }

        private void Update()
        {
            if (!IsServer) return;
            behavior?.Execute(this);
        }

        public void Move()
        {
            UpdateTarget();
            if (cachedTarget == null) return;

            Vector3 currentTargetPos = cachedTarget.transform.position;
            Vector3 direction = (currentTargetPos - transform.position).normalized;

            if ((lastSentDirection - direction).sqrMagnitude > directionThreshold * directionThreshold)
            {
                lastSentDirection = direction;
                NetworkDirection.Value = new Vector2(direction.x, direction.y);
            }

            if (agent.remainingDistance < 0.1f || (lastTargetPos - currentTargetPos).sqrMagnitude > 0.01f)
            {
                agent.SetDestination(currentTargetPos);
                lastTargetPos = currentTargetPos;
            }
        }

        private void OnDirectionChanged(Vector2 previous, Vector2 current)
        {
            view.PlayWalkAnimation(current);
        }

        public void TryAttack(Transform target, PlayerController playerController)
        {
            if (!IsServer) return;
            float distSqr = (target.position - transform.position).sqrMagnitude;
            if (Time.time - lastAttackTime < model.AttackRate || distSqr > 0.36f) return;

            lastAttackTime = Time.time;
            PlayAttackAnimationClientRpc();

            playerController.GetComponent<PlayerHealth>().TakeDamageServerRpc(model.Damage);
            playerController.GetView().PlayHitEffectClientRpc();
        }

        public void ShootProjectile(Vector3 targetPosition, GameObject projectilePrefab)
        {
            if (!IsServer) return;

            Vector3 spawnPos = transform.position + (targetPosition - transform.position).normalized * 0.5f;

            var netObj = NetworkPoolManager.Instance.Spawn(projectilePrefab, spawnPos, Quaternion.identity);
            if (netObj != null)
            {
                var projectile = netObj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    Vector3 dir = (targetPosition - spawnPos).normalized;
                    projectile.Init(dir, ProjectileOwner.Enemy, model.Damage);
                }
                else
                {
                    NetworkPoolManager.Instance.Despawn(netObj);
                }
            }
        }

        [ClientRpc]
        private void PlayAttackAnimationClientRpc()
        {
            view.PlayAttackAnimation();
        }

        public void UpdateTarget()
        {
            if (Time.time - targetUpdateTime < targetUpdateInterval) return;
            targetUpdateTime = Time.time;

            float minDist = float.MaxValue;
            PlayerController closestController = null;

            var clients = NetworkManager.Singleton.ConnectedClientsList;
            foreach (var t in clients)
            {
                var obj = t.PlayerObject;
                if (obj == null) continue;

                float distSqr = (obj.transform.position - transform.position).sqrMagnitude;
                if (distSqr < minDist)
                {
                    minDist = distSqr;
                    closestController = obj.GetComponent<PlayerController>();
                }
            }

            cachedTarget = closestController;
        }

        public override void OnDestroy()
        {
            if (IsClient)
                NetworkDirection.OnValueChanged -= OnDirectionChanged;
        }

        public PlayerController GetCachedTarget()
        {
            return cachedTarget;
        }
    }
}
