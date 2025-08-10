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
        private bool isDead;

        public void InitializeHealth(int initialHealth)
        {
            currentHealth.OnValueChanged += OnHealthValueChanged;
            var playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
            
            if (IsServer)
            {
                maxHealth = initialHealth;
                currentHealth.Value = maxHealth;
            }
            else
            {
                if (playerData != null)
                {
                    maxHealth = playerData.MaxHealth.Value;
                }
            }

            if (IsOwner)
            {
                UIManager.Instance?.RegisterPlayerHealth(this);
                playerData.MaxHealth.OnValueChanged += OnMaxHealthSynced;
            }
        }

        private void OnMaxHealthSynced(int oldVal, int newVal)
        {
            var playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
            if (playerData == null) return;

            if (!isDead)
            {
                if (CurrentHealth == maxHealth)
                {
                    maxHealth = newVal;
                    UpdateCurrentHealthServerRpc(maxHealth);
                    OnHealthChanged?.Invoke(maxHealth);
                }
                else
                {
                    var increaseAmount = newVal - maxHealth;
                    UpdateCurrentHealthServerRpc(CurrentHealth + increaseAmount);
                    maxHealth = newVal;
                    OnHealthChanged?.Invoke(currentHealth.Value);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateCurrentHealthServerRpc(int newCurrentHealth)
        {
            if (!IsServer)
                return;

            if (NetworkObject == null || !NetworkObject.IsSpawned)
            {
                Debug.LogWarning("PlayerHealth NetworkObject is null or not spawned, can't update health.");
                return;
            }

            currentHealth.Value = newCurrentHealth;
        }

        private void OnHealthValueChanged(int oldValue, int newValue)
        {
            OnHealthChanged?.Invoke(newValue);

            if (IsServer && newValue <= 0 && !isDead)
            {
                isDead = true;
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

            isDead = false;
            currentHealth.Value = maxHealth;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
                UIManager.Instance?.UnregisterPlayerHealth(this);

            var playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
            playerData.MaxHealth.OnValueChanged -= OnMaxHealthSynced;
        }
    }
}