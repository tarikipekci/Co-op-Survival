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
        }

        public void StartUpgradePhase()
        {
            if (!IsServer) return;

            totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
            playersWhoSelectedUpgrade.Clear();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestUpgradeServerRpc(string upgradeId, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            var playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(clientId);

            if (playerData == null) return;

            var upgrade = UpgradeManager.Instance.GetUpgradeById(upgradeId);
            if (upgrade == null) return;
            if (!upgrade.IsAvailable(playerData)) return;

            upgrade.Apply(playerData);


            RegisterPlayerSelection(clientId);
        }

        [ClientRpc]
        private void ApplyUpgradeClientRpc(string upgradeId, ClientRpcParams rpcParams = default)
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(clientId);
            if (playerData == null) return;

            var upgrade = UpgradeManager.Instance.GetUpgradeById(upgradeId);
            if (upgrade == null) return;

            upgrade.Apply(playerData);
        }

        private void RegisterPlayerSelection(ulong clientId)
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