using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerData : NetworkBehaviour
    {
        public NetworkVariable<int> MaxHealth = new NetworkVariable<int>();
        public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            MaxHealth.OnValueChanged += OnMaxHealthChanged;
            MoveSpeed.OnValueChanged += OnMoveSpeedChanged;
        }

        private void OnMaxHealthChanged(int oldValue, int newValue)
        {
            Debug.Log($"MaxHealth changed from {oldValue} to {newValue} for client {OwnerClientId}");
        }

        private void OnMoveSpeedChanged(float oldValue, float newValue)
        {
            Debug.Log($"MoveSpeed changed from {oldValue} to {newValue} for client {OwnerClientId}");
        }
    }
}