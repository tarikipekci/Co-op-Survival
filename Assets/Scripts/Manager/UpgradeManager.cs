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
            var available = allUpgrades
                .Where(upgrade => upgrade.IsAvailable(playerData))
                .ToList();

            int count = Mathf.Min(3, available.Count);
            return available
                .OrderBy(_ => Random.value)
                .Take(count)
                .ToList();
        }

        public Upgrade GetUpgradeById(string id)
        {
            return allUpgrades.FirstOrDefault(u => u.Id == id);
        }
    }
}