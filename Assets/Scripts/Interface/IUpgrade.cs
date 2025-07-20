using Player;
using UnityEngine;

namespace Interface
{
    public interface IUpgrade
    {
        string Name { get; }
        string Description { get; }
        Sprite Icon { get; }

        public bool IsAvailable(PlayerData playerData);

        public void Apply(PlayerData playerData);
    }
}
