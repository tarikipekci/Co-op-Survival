using System;
using System.Collections;
using Manager;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerHealth : NetworkBehaviour
    {
        public event Action<int> OnHealthChanged;

        [SerializeField] private int maxHealth = 3;
        private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

        public delegate void OnDeathHandler(ulong clientId);

        public static event OnDeathHandler OnPlayerDied;

        public int CurrentHealth => currentHealth.Value;

        public override void OnNetworkSpawn()
        {
            currentHealth.OnValueChanged += OnHealthValueChanged;

            if (IsServer)
                currentHealth.Value = maxHealth;

            if (IsOwner)
                UIManager.Instance?.RegisterPlayerHealth(this);
        }

        private void OnHealthValueChanged(int oldValue, int newValue)
        {
            OnHealthChanged?.Invoke(newValue);

            if (IsServer && newValue <= 0)
            {
                currentHealth.Value = 0;
                StartCoroutine(DelayedDestroy());
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(int damage)
        {
            if (IsServer)
            {
                currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0);
            }
        }

        private IEnumerator DelayedDestroy()
        {
            yield return new WaitForSeconds(0.1f);

            var flashlightController = GetComponent<PlayerFlashlightController>();
            flashlightController?.RemoveFlashlight();

            OnPlayerDied?.Invoke(OwnerClientId);
            NetworkObject.Despawn();
        }

        public int GetMaxHealth()
        {
            return maxHealth;
        }

        public void IncreaseCurrentHealth(int amount)
        {
            currentHealth.Value = Mathf.Clamp(currentHealth.Value + amount, 0, maxHealth);
        }

        public void FullHealth()
        {
            currentHealth.Value = maxHealth;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
                UIManager.Instance?.UnregisterPlayerHealth(this);
        }
    }
}