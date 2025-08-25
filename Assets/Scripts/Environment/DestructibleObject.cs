using Interface;
using Unity.Netcode;
using UnityEngine;

namespace Environment
{
    public class DestructibleObject : NetworkBehaviour, ICanTakeDamage
    {
        [SerializeField] private int maxHealth = 3;
        private int currentHealth;

        [SerializeField] private GameObject destroyEffect;

        private IDropProvider dropProvider;
        private bool isDead; 

        private void Awake()
        {
            currentHealth = maxHealth;
            dropProvider = GetComponent<IDropProvider>();
        }

        public void TakeDamage(int amount)
        {
            if (IsServer)
            {
                if (isDead) return; 
                currentHealth -= amount;

                if (currentHealth <= 0)
                {
                    Die();
                }
            }
            else
            {
                TakeDamageServerRpc(amount);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(int amount)
        {
            TakeDamage(amount);
        }

        private void Die()
        {
            if (isDead) return; 
            isDead = true;

            if (destroyEffect != null)
            {
                var fx = Instantiate(destroyEffect, transform.position, Quaternion.identity);
                Destroy(fx, 1f);
            }

            dropProvider?.Drop(transform.position);

            if (NetworkObject.IsSpawned)
                NetworkObject.Despawn();
            else
                Destroy(gameObject);
        }
    }
}
