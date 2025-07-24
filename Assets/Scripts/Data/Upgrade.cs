using Interface;
using Player;
using UnityEngine;

namespace Data
{
    public abstract class Upgrade : ScriptableObject, IUpgrade
    {
        [SerializeField] private string upgradeName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [SerializeField] private string upgradeId;
        public string Id => upgradeId;

        public string Name => upgradeName;
        public string Description => description;
        public Sprite Icon => icon;

        public virtual bool IsAvailable(PlayerData playerData)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Apply(PlayerData playerData)
        {
            throw new System.NotImplementedException();
        }
    }
}