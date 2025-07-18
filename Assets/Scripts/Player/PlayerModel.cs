using UnityEngine;

namespace Player
{
    public class PlayerModel
    {
        public readonly float MoveSpeed;
        public readonly int MaxHealth;
        public Vector2 MoveInput = Vector2.zero;

        public PlayerModel(PlayerData playerData)
        {
            MoveSpeed = playerData.MoveSpeed.Value;
            MaxHealth = playerData.MaxHealth.Value;
        }
    }
}