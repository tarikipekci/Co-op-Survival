using System.Collections.Generic;
using Player;
using UI;
using UnityEngine;

namespace Manager
{
    public class UpgradeUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject upgradeCardPrefab;
        [SerializeField] private Transform cardContainer;

        private List<GameObject> activeCards = new();

        public void ShowUpgradeOptions(PlayerData playerData)
        {
            foreach (var card in activeCards)
                Destroy(card);
            activeCards.Clear();

            var upgrades = UpgradeManager.Instance.GetAvailableUpgrades(playerData);

            foreach (var upgrade in upgrades)
            {
                var cardGO = Instantiate(upgradeCardPrefab, cardContainer);
                var card = cardGO.GetComponent<UpgradeCardUI>();
                card.Initialize(upgrade, () =>
                {
                    UpgradePhaseManager.Instance.RequestUpgradeServerRpc(upgrade.Id);
                    HideUpgradeUIWithoutResume();
                });
                activeCards.Add(cardGO);
            }

            gameObject.SetActive(true);
        }

        public void HideUpgradeUIWithoutResume()
        {
            gameObject.SetActive(false);
        }
    }
}