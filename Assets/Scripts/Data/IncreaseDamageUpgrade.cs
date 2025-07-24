using Player;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Upgrade/Increase Damage")]
    public class IncreaseDamageUpgrade : Upgrade
    {
        [SerializeField] private int maxDamageCapacity = 3;
        [SerializeField] private int increaseAmount = 1;

        public override bool IsAvailable(PlayerData playerData)
        {
            if (playerData.Damage.Value + increaseAmount <= maxDamageCapacity)
                return true;

            return false;
        }

        public override void Apply(PlayerData playerData)
        {
            playerData.Damage.Value += increaseAmount;
        }
    }
}