using System.Collections;
using Environment;
using Unity.Netcode;
using UnityEngine;

namespace Manager
{
    public class XPManager : NetworkBehaviour
    {
        public static XPManager Instance { get; private set; }

        public NetworkVariable<int> Experience = new NetworkVariable<int>();
        public NetworkVariable<int> Level = new NetworkVariable<int>(1);

        public NetworkVariable<int> xpPerLevel = new NetworkVariable<int>(100);
        [SerializeField] private int requiredXPMultiplier = 100;

        [SerializeField] private GameObject xpPickupPrefab;
        public event System.Action<int> OnLevelUp;
        public event System.Action<int, int> OnXPChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                OnLevelUp += HandleLevelUp;
            }

            if (IsClient)
            {
                Experience.OnValueChanged += OnXPChangedClient;
            }
        }

        private void OnXPChangedClient(int oldValue, int newValue)
        {
            OnXPChanged?.Invoke(newValue, xpPerLevel.Value);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddXPServerRpc(int amount)
        {
            Experience.Value += amount;
            Debug.Log("Current experience is :" + Experience.Value);
            OnXPChanged?.Invoke(Experience.Value, xpPerLevel.Value);
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            while (Experience.Value >= xpPerLevel.Value)
            {
                Experience.Value -= xpPerLevel.Value;
                Level.Value++;
                xpPerLevel.Value = Level.Value * requiredXPMultiplier;
                OnLevelUp?.Invoke(Level.Value);
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            if (IsServer)
                ShowUpgradeUIClientRpc();
            if (IsHost)
            {
                UpgradePhaseManager.Instance.StartUpgradePhase();
            }
        }

        [ClientRpc]
        private void ShowUpgradeUIClientRpc()
        {
            Time.timeScale = 0f;

            var playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(NetworkManager.Singleton.LocalClientId);
            if (playerData == null)
            {
                Debug.LogError("PlayerData not found for upgrade UI.");
                return;
            }

            var upgradeUI = UIManager.Instance.GetUpgradeUIManager();
            if (upgradeUI != null)
                upgradeUI.ShowUpgradeOptions(playerData);
        }

        public void SpawnXPPickupDelayed(Vector3 position, int amount, float delay = 1.0f)
        {
            StartCoroutine(SpawnCoroutine(position, amount, delay));
        }

        private IEnumerator SpawnCoroutine(Vector3 position, int amount, float delay)
        {
            yield return new WaitForSeconds(delay);

            NetworkObject netObj = NetworkPoolManager.Instance.Spawn(xpPickupPrefab, position, Quaternion.identity);
            if (netObj != null)
            {
                var xpObj = netObj.gameObject;
                var xp = xpObj.GetComponent<XPPickup>();

                xp.xpValue = amount;
                xp.Activate(position);

                Debug.Log("XP spawned after delay.");
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsClient)
            {
                Experience.OnValueChanged -= OnXPChangedClient;
            }

            if (IsServer)
            {
                OnLevelUp -= HandleLevelUp;
            }
        }
    }
}
