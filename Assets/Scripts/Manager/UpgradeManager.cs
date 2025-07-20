using System.Collections.Generic;
using System.Linq;
using Data;
using Player;
using UnityEngine;

namespace Manager
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private List<Upgrade> allUpgrades;
        public static UpgradeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
        }

        public List<Upgrade> GetAvailableUpgrades(PlayerData playerData)
        {
            return allUpgrades
                .Where(upgrade => upgrade.IsAvailable(playerData))
                .Take(3)
                .ToList();
        }
    }
}