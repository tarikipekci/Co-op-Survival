using Player;
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
            model.target = FindClosestPlayerController().transform;
            agent.SetDestination(model.target.position);

            //Server updates the value
            NetworkDirection.Value = direction;
        }

        private void OnDirectionChanged(Vector2 previous, Vector2 current)
        {
            view.PlayWalkAnimation(current);
        }

        public void TryAttack(Transform target, PlayerController playerController)
        {
            if (!IsServer) return; 

            if (Time.time - lastAttackTime < model.AttackCooldown) return;

            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= 1f)
            {
                lastAttackTime = Time.time;

                PlayAttackAnimationClientRpc();
                
                playerController.GetComponent<PlayerHealth>().TakeDamageServerRpc(1);

                //playerController.TakeDamage(model.AttackDamage);

                playerController.GetView().PlayHitEffectClientRpc();
            }
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