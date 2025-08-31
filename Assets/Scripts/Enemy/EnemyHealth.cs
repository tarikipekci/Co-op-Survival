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
                    enemyView?.PlayHitEffect();
            };
        }

        public void InitializeHealth(int health)
        {
            maxHealth = health;

            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                currentHealth.Value = maxHealth;
            }
            else
            {
                StartCoroutine(SetHealthAfterSpawn());
            }
        }

        private System.Collections.IEnumerator SetHealthAfterSpawn()
        {
            yield return new WaitUntil(() => NetworkObject != null && NetworkObject.IsSpawned);
            currentHealth.Value = maxHealth;
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(int amount)
        {
            if (!IsServer) return;

            currentHealth.Value -= amount;

            if (WaveManager.Instance.currentBossHealth == this)
            {
                WaveManager.Instance.OnBossTakeDamage?.Invoke(currentHealth.Value, maxHealth);
                WaveManager.Instance.UpdateBossHealthClientRpc(currentHealth.Value, maxHealth);
            }

            if (currentHealth.Value <= 0)
            {
                XPManager.Instance.SpawnXPPickupDelayed(transform.position, xpValue);
                Die();
            }
        }

        private void Die()
        {
            if (!IsServer) return;
            PlayDeathClientRpc();

            if (WaveManager.Instance.currentBossHealth == this)
                WaveManager.Instance.currentBossHealth = null;

            NetworkPoolManager.Instance.Despawn(NetworkObject);
        }

        [ClientRpc]
        private void PlayDeathClientRpc()
        {
            enemyView?.PlayDeathAnimation();
        }
    }
}
