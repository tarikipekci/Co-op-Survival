using UnityEngine;

namespace Player
{
    public class PlayerModel
    {
        public float MoveSpeed { get; private set; }
        public int MaxHealth { get; private set; }
        public Vector2 MoveInput = Vector2.zero;

        private readonly PlayerData _playerData;

        public PlayerModel(PlayerData playerData)
        {
            _playerData = playerData;
            MoveSpeed = playerData.MoveSpeed.Value;
            MaxHealth = playerData.MaxHealth.Value;

            playerData.MoveSpeed.OnValueChanged += OnMoveSpeedChanged;
            playerData.MaxHealth.OnValueChanged += OnMaxHealthChanged;
        }

        private void OnMoveSpeedChanged(float oldValue, float newValue)
        {
            MoveSpeed = newValue;
            Debug.Log($"PlayerModel: MoveSpeed updated to {newValue}");
        }

        private void OnMaxHealthChanged(int oldValue, int newValue)
        {
            MaxHealth = newValue;
            Debug.Log($"PlayerModel: MaxHealth updated to {newValue}");
        }
    }
}