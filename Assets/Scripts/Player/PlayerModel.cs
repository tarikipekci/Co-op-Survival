using UnityEngine;

namespace Player
{
    public class PlayerModel
    {
        public float MoveSpeed { get; private set; }
        public int MaxHealth { get; private set; }

        public int Damage { get; private set; }

        public float AttackRate { get; private set; }

        private readonly PlayerData _playerData;
        private PlayerHealth _playerHealth;

        public PlayerModel(PlayerData playerData)
        {
            _playerData = playerData;
            MoveSpeed = playerData.MoveSpeed.Value;
            MaxHealth = playerData.MaxHealth.Value;
            Damage = playerData.Damage.Value;
            AttackRate = playerData.AttackRate.Value;

            playerData.MoveSpeed.OnValueChanged += OnMoveSpeedChanged;
            playerData.MaxHealth.OnValueChanged += OnMaxHealthChanged;
            playerData.Damage.OnValueChanged += OnDamageChanged;
            playerData.AttackRate.OnValueChanged += OnAttackRateChanged;
        }

        private void OnAttackRateChanged(float previousValue, float newValue)
        {
            AttackRate = newValue;
            Debug.Log($"PlayerModel: AttackRate updated to {newValue}");
        }

        private void OnDamageChanged(int previousValue, int newValue)
        {
            Damage = newValue;
            Debug.Log($"PlayerModel: Damage updated to {newValue}");
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