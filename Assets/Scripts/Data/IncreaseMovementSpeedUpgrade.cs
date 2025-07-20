using Player;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Upgrade/Increase Move Speed")]
    public class IncreaseMovementSpeedUpgrade : Upgrade
    {
        [SerializeField] private int maxMovementSpeed = 6;
        [SerializeField] private int increaseAmount = 1;

        public override bool IsAvailable(PlayerData playerData)
        {
            if (playerData.MoveSpeed.Value + increaseAmount <= maxMovementSpeed)
                return true;

            return false;
        }

        public override void Apply(PlayerData playerData)
        {
            playerData.MoveSpeed.Value += increaseAmount;
        }
    }
}