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