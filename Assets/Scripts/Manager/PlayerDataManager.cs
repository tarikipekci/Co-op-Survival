using System.Collections;
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
            if (!IsServer) return;

            StartCoroutine(DelayedCreatePlayerData(clientId));
        }

        private IEnumerator DelayedCreatePlayerData(ulong clientId)
        {
            yield return new WaitForSeconds(0.5f);
            var playerData = GetOrCreatePlayerData(clientId);
            if (playerData != null)
                SyncPlayerDataClientRpc(clientId, playerData.NetworkObject);
        }

        public PlayerData GetOrCreatePlayerData(ulong clientId)
        {
            if (playerDataDict.TryGetValue(clientId, out var existingData))
                return existingData;

            if (!IsServer)
            {
                RequestPlayerDataServerRpc(clientId);
                return null;
            }

            var newData = CreatePlayerData(clientId);
            if (newData != null)
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

            if (IsServer)
            {
                if (!netObj.IsSpawned)
                {
                    netObj.Spawn(true);
                }
                else
                {
                    Debug.LogWarning($"Tried to spawn PlayerData for Client {clientId} but it's already spawned.");
                }
            }

            DontDestroyOnLoad(go);
            return pd;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPlayerDataServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            var playerData = GetOrCreatePlayerData(clientId);
            if (playerData != null)
            {
                SyncPlayerDataClientRpc(clientId, playerData.NetworkObject, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
                });
            }
        }

        [ClientRpc]
        private void SyncPlayerDataClientRpc(ulong clientId, NetworkObjectReference netObjRef,
            ClientRpcParams rpcParams = default)
        {
            if (netObjRef.TryGet(out NetworkObject netObj))
            {
                var pd = netObj.GetComponent<PlayerData>();
                if (pd != null)
                {
                    playerDataDict[clientId] = pd;
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