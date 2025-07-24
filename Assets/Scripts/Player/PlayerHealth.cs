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

        private int maxHealth;
        private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

        public delegate void OnDeathHandler(ulong clientId);

        public static event OnDeathHandler OnPlayerDied;

        public int CurrentHealth => currentHealth.Value;

        public void InitializeHealth(int initialHealth)
        {
            currentHealth.OnValueChanged += OnHealthValueChanged;

            if (IsServer)
            {
                maxHealth = initialHealth;
                currentHealth.Value = maxHealth;
            }
            else
            {
                var playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
                if (playerData != null)
                {
                    maxHealth = playerData.MaxHealth.Value;

                    if (maxHealth == 0)
                    {
                        playerData.MaxHealth.OnValueChanged += OnMaxHealthSynced;
                    }
                }
            }

            if (IsOwner)
                UIManager.Instance?.RegisterPlayerHealth(this);
        }

        private void OnMaxHealthSynced(int oldVal, int newVal)
        {
            var playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
            if (playerData == null) return;

            maxHealth = newVal;

            if (IsServer)
            {
                currentHealth.Value = maxHealth;
            }

            playerData.MaxHealth.OnValueChanged -= OnMaxHealthSynced;
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

            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
        }

        public int GetMaxHealth()
        {
            return maxHealth;
        }

        public void IncreaseCurrentHealth(int amount)
        {
            if (!IsServer) return;

            currentHealth.Value = Mathf.Clamp(currentHealth.Value + amount, 0, maxHealth);
        }

        public void FullHealth()
        {
            if (!IsServer) return;

            currentHealth.Value = maxHealth;
        }

        public void UpdateMaxHealth(int newValue)
        {
            maxHealth = newValue;

            if (!IsServer) return;

            if (currentHealth.Value != maxHealth)
                currentHealth.Value = maxHealth;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
                UIManager.Instance?.UnregisterPlayerHealth(this);
        }
    }
}
