using System.Collections.Generic;
using Player;
using UI;
using Unity.Netcode;
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
            var playerController =
                NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

            foreach (var upgrade in upgrades)
            {
                var cardGO = Instantiate(upgradeCardPrefab, cardContainer);
                var card = cardGO.GetComponent<UpgradeCardUI>();
                card.Initialize(upgrade, () =>
                {
                    playerController.RequestUpgradeServerRpc(upgrade.Id);
                    HideUpgradeUIWithoutResume();
                });
                activeCards.Add(cardGO);
            }

            gameObject.SetActive(true);
        }

        private void HideUpgradeUI()
        {
            gameObject.SetActive(false);
        }

        public void HideUpgradeUIWithoutResume()
        {
            gameObject.SetActive(false);
        }
    }
}