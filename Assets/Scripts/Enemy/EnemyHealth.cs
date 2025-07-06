using Unity.Netcode;
using UnityEngine;

namespace Enemy
{
    public class EnemyHealth : NetworkBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

        private EnemyView enemyView;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                currentHealth.Value = maxHealth;
            }

            enemyView = GetComponent<EnemyView>();

            currentHealth.OnValueChanged += (oldVal, newVal) =>
            {
                if (newVal < oldVal)
                {
                    enemyView?.PlayHitEffect();
                }
            };
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(int amount)
        {
            if (!IsServer) return;

            currentHealth.Value -= amount;

            Debug.Log($"Enemy took {amount} damage, remaining {currentHealth.Value}");

            if (currentHealth.Value <= 0)
            {
                DieClientRpc();
            }
        }

        [ClientRpc]
        private void DieClientRpc()
        {
            Debug.Log("Enemy died");
            enemyView?.PlayDeathAnimation();

            if (IsServer)
            {
                if (NetworkObject.IsSpawned)
                {
                    NetworkObject.Despawn();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}