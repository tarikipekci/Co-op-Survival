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

        [SerializeField] private int xpPerLevel = 100;
        [SerializeField] private int requiredXPMultiplier = 100;

        [SerializeField] private GameObject xpPickupPrefab; 

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddXPServerRpc(int amount)
        {
            Experience.Value += amount;
            Debug.Log("Current experience is :" + Experience.Value);
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            while (Experience.Value >= xpPerLevel)
            {
                Experience.Value -= xpPerLevel;
                Level.Value++;
                xpPerLevel = Level.Value * requiredXPMultiplier;
            }
        }

        public void SpawnXPPickupDelayed(Vector3 position, int amount, float delay = 1.0f)
        {
            StartCoroutine(SpawnCoroutine(position, amount, delay));
        }

        private IEnumerator SpawnCoroutine(Vector3 position, int amount, float delay)
        {
            yield return new WaitForSeconds(delay);

            GameObject xpObj = Instantiate(xpPickupPrefab, position, Quaternion.identity);
            var netObj = xpObj.GetComponent<NetworkObject>();
            netObj.Spawn();

            var xp = xpObj.GetComponent<XPPickup>();
            xp.xpValue = amount;
            xp.Activate(position);

            Debug.Log("XP spawned after delay.");
        }
    }
}
