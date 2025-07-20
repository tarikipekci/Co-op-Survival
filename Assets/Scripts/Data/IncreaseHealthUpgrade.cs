using Player;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Upgrade/Increase Max Health")]
    public class IncreaseHealthUpgrade : Upgrade
    {
        [SerializeField] private int maxHealthCapacity = 6;
        [SerializeField] private int increaseAmount = 1;

        public override bool IsAvailable(PlayerData playerData)
        {
            if (playerData.MaxHealth.Value + increaseAmount <= maxHealthCapacity)
                return true;

            return false;
        }

        public override void Apply(PlayerData playerData)
        {
            playerData.MaxHealth.Value += increaseAmount;
        }
    }
}