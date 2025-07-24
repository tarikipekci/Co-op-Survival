using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerData : NetworkBehaviour
    {
        public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(3);
        public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(2);
        public NetworkVariable<int> Damage = new NetworkVariable<int>(1);
        public NetworkVariable<float> AttackRate = new NetworkVariable<float>(1);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            MaxHealth.OnValueChanged += OnMaxHealthChanged;
            MoveSpeed.OnValueChanged += OnMoveSpeedChanged;
            Damage.OnValueChanged += OnDamageChanged;
            AttackRate.OnValueChanged += OnAttackRateChanged;
        }

        private void OnMaxHealthChanged(int oldValue, int newValue)
        {
            Debug.Log($"MaxHealth changed from {oldValue} to {newValue} for client {OwnerClientId}");
            NetworkObject playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(OwnerClientId);
            var healthComp = playerObj.GetComponent<PlayerHealth>();
            healthComp.UpdateMaxHealth(MaxHealth.Value);
        }

        private void OnMoveSpeedChanged(float oldValue, float newValue)
        {
            Debug.Log($"MoveSpeed changed from {oldValue} to {newValue} for client {OwnerClientId}");
        }

        private void OnDamageChanged(int previousValue, int newValue)
        {
            Debug.Log($"Damage changed from {previousValue} to {newValue} for client {OwnerClientId}");
        }

        private void OnAttackRateChanged(float previousValue, float newValue)
        {
            Debug.Log($"AttackRate changed from {previousValue} to {newValue} for client {OwnerClientId}");
        }
    }
}