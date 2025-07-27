using Interface;
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

        public NetworkVariable<Vector2> NetworkDirection = new NetworkVariable<Vector2>(
            Vector2.zero);

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

        private void FixedUpdate()
        {
            if (!IsServer) return;

            behavior?.Execute(this);
        }

        public void Move(Vector2 direction)
        {
            var closestPlayerController = FindClosestPlayerController();

            model.target = closestPlayerController.transform;
            agent.SetDestination(model.target.position);
            NetworkDirection.Value = direction;
        }

        private void OnDirectionChanged(Vector2 previous, Vector2 current)
        {
            view.PlayWalkAnimation(current);
        }

        public void TryAttack(Transform target, PlayerController playerController)
        {
            if (!IsServer) return; 

            if (Time.time - lastAttackTime < model.AttackRate) return;

            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= 0.6f)
            {
                lastAttackTime = Time.time;

                PlayAttackAnimationClientRpc();
                
                playerController.GetComponent<PlayerHealth>().TakeDamageServerRpc(model.Damage);

                playerController.GetView().PlayHitEffectClientRpc();
            }
        }

        public void ShootProjectile(Vector3 targetPosition, GameObject projectilePrefab,
            PlayerController playerController)
        {
            if (!IsServer) return;

            Vector3 spawnPos = transform.position + (targetPosition - transform.position).normalized * 0.5f;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            projectile.GetComponent<NetworkObject>().Spawn();

            var dir = (targetPosition - spawnPos).normalized;
            projectile.GetComponent<Projectile>().Init(dir, ProjectileOwner.Enemy, model.Damage);
        }

        [ClientRpc]
        private void PlayAttackAnimationClientRpc()
        {
            view.PlayAttackAnimation();
        }
        
        public PlayerController FindClosestPlayerController()
        {
            float minDist = float.MaxValue;
            PlayerController closestController = null;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var obj = client.PlayerObject;
                if (obj == null) continue;

                float dist = Vector2.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestController = obj.GetComponent<PlayerController>();
                }
            }

            return closestController;
        }
        
        public override void OnDestroy()
        {
            if (IsClient)
                NetworkDirection.OnValueChanged -= OnDirectionChanged;
        }
    }
}