using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

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
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;

            behavior?.Execute(this);
        }

        public void Move(Vector2 direction)
        {
            model.target = FindClosestPlayer();
            agent.SetDestination(model.target.position);

            //Server updates the value
            NetworkDirection.Value = direction;
        }

        private void OnDirectionChanged(Vector2 previous, Vector2 current)
        {
            view.PlayWalkAnimation(current);
        }

        public void TryAttack(Transform target)
        {
            if (Time.time - lastAttackTime < model.AttackCooldown) return;

            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= 1.5f)
            {
                Debug.Log("Enemy attacks target!");
                lastAttackTime = Time.time;
                view.PlayAttackAnimation();
            }
        }

        public Transform FindClosestPlayer()
        {
            float minDist = float.MaxValue;
            Transform closest = null;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var obj = client.PlayerObject;
                if (obj == null) continue;

                float dist = Vector2.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = obj.transform;
                }
            }

            return closest;
        }

        public void TakeDamage(float amount)
        {
            model.CurrentHealth -= amount;
            view.PlayHitEffect();

            if (model.CurrentHealth <= 0)
                view.PlayDeathAnimation();
        }

        public override void OnDestroy()
        {
            if (IsClient)
                NetworkDirection.OnValueChanged -= OnDirectionChanged;
        }
    }
}