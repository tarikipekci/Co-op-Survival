using Unity.Netcode;
using UnityEngine;
using Manager;

namespace Enemy
{
    public class EnemyHealth : NetworkBehaviour
    {
        private int maxHealth;
        private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

        private EnemyView enemyView;
        [SerializeField] private int xpValue;

        public override void OnNetworkSpawn()
        {
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
            if (WaveManager.Instance.currentBossHealth == this)
            {
                WaveManager.Instance.OnBossTakeDamage?.Invoke(currentHealth.Value, maxHealth);
                WaveManager.Instance.UpdateBossHealthClientRpc(currentHealth.Value, maxHealth);
            }

            currentHealth.Value -= amount;

            Debug.Log($"Enemy took {amount} damage, remaining {currentHealth.Value}");

            if (currentHealth.Value <= 0)
            {
                XPManager.Instance.SpawnXPPickupDelayed(transform.position, xpValue);
                DieClientRpc(); 
            }
        }

        [ClientRpc]
        private void DieClientRpc()
        {
            Debug.Log("Enemy died");
            enemyView?.PlayDeathAnimation();

            if (WaveManager.Instance.currentBossHealth == this)
            {
                WaveManager.Instance.SetBossBarActiveClientRpc(false); 
                WaveManager.Instance.currentBossHealth = null;
            }

            if (IsServer)
            {
                if (NetworkObject.IsSpawned)
                    NetworkObject.Despawn();
                else
                    Destroy(gameObject);
            }
        }

        public void SetMaxHealth(int value)
        {
            maxHealth = value;
            currentHealth.Value = maxHealth;
        }
    }
}
