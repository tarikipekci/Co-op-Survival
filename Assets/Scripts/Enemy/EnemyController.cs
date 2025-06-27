using UnityEngine;
using Unity.Netcode;

namespace Enemy
{
    [RequireComponent(typeof(NetworkObject))]
    public class EnemyController : NetworkBehaviour
    {
        private EnemyModel model;
        private EnemyView view;
        private IEnemyBehavior behavior;

        private float lastAttackTime;

        public void Initialize(EnemyModel modelData, IEnemyBehavior behaviorLogic)
        {
            model = modelData;
            behavior = behaviorLogic;
            view = GetComponent<EnemyView>();
        }

        private void Update()
        {
            if (!IsServer) return; 

            behavior?.Execute(this);
        }

        public void Move(Vector2 direction)
        {
            transform.position += (Vector3)(direction * (model.MoveSpeed * Time.deltaTime));
        }

        public void TryAttack(Transform target)
        {
            if (Time.time - lastAttackTime < model.AttackCooldown) return;

            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= 1.5f)
            {
                Debug.Log("Enemy attacks target!");
                lastAttackTime = Time.time;
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
                view.Die();
        }
    }
}
