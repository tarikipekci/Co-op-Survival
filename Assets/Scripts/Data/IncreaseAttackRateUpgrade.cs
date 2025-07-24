using Player;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Upgrade/Increase Attack Rate")]
    public class IncreaseAttackRateUpgrade : Upgrade
    {
        [SerializeField] private float minAttackRate = 0.1f;
        [SerializeField] private float decreaseAmount = 0.1f;

        public override bool IsAvailable(PlayerData playerData)
        {
            if (playerData.AttackRate.Value - decreaseAmount >= minAttackRate)
                return true;

            return false;
        }

        public override void Apply(PlayerData playerData)
        {
            playerData.AttackRate.Value -= decreaseAmount;
        }
    }
}