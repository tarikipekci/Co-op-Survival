using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Manager
{
    public class PlayerDataManager : NetworkBehaviour
    {
        public static PlayerDataManager Instance;
        private Dictionary<ulong, PlayerData> playerDataDict = new Dictionary<ulong, PlayerData>();

        [SerializeField] private GameObject playerDataPrefab; 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                var playerData = GetOrCreatePlayerData(clientId);
                SyncPlayerDataClientRpc(clientId, playerData.NetworkObject);
            }
        }

        public PlayerData GetOrCreatePlayerData(ulong clientId)
        {
            if (playerDataDict.TryGetValue(clientId, out var existingData))
                return existingData;

            var newData = CreatePlayerData(clientId);
            playerDataDict.Add(clientId, newData);
            return newData;
        }

        private PlayerData CreatePlayerData(ulong clientId)
        {
            if (playerDataPrefab == null)
            {
                Debug.LogError("PlayerData prefab is not assigned in PlayerDataManager!");
                return null;
            }

            var go = Instantiate(playerDataPrefab);
            go.name = $"PlayerData_{clientId}";
            var pd = go.GetComponent<PlayerData>();
            var netObj = go.GetComponent<NetworkObject>();

            if (IsServer && !netObj.IsSpawned)
            {
                netObj.Spawn(true);
            }

            DontDestroyOnLoad(go);
            return pd;
        }

        [ClientRpc]
        private void SyncPlayerDataClientRpc(ulong clientId, NetworkObjectReference netObjRef)
        {
            if (netObjRef.TryGet(out NetworkObject netObj))
            {
                var playerData = netObj.GetComponent<PlayerData>();
                if (playerData != null)
                {
                    playerDataDict[clientId] = playerData;
                    Debug.Log($"Client {clientId} synced PlayerData.");
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
    }
}