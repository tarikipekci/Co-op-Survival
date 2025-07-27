using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Manager
{
    public class UpgradePhaseManager : NetworkBehaviour
    {
        public static UpgradePhaseManager Instance { get; private set; }

        private HashSet<ulong> playersWhoSelectedUpgrade = new();
        private int totalPlayers;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        public void StartUpgradePhase()
        {
            if (!IsServer) return;

            totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
            playersWhoSelectedUpgrade.Clear();
        }

        public void RegisterPlayerSelection(ulong clientId)
        {
            if (!IsServer) return;

            playersWhoSelectedUpgrade.Add(clientId);
            Debug.Log($"Player {clientId} selected upgrade ({playersWhoSelectedUpgrade.Count}/{totalPlayers})");

            if (playersWhoSelectedUpgrade.Count >= totalPlayers)
            {
                ResumeGameForAllClientsClientRpc();
            }
        }

        [ClientRpc]
        private void ResumeGameForAllClientsClientRpc()
        {
            Time.timeScale = 1f;

            var upgradeUI = UIManager.Instance.GetUpgradeUIManager();
            if (upgradeUI != null)
                upgradeUI.HideUpgradeUIWithoutResume();
        }
    }
}
