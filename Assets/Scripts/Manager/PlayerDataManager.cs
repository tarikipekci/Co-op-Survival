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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
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
            var go = new GameObject($"PlayerData_{clientId}");
            var pd = go.AddComponent<PlayerData>();

            var netObj = go.AddComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);
            DontDestroyOnLoad(go);
            return pd;
        }
    }
}
