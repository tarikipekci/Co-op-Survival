using System.Collections.Generic;
using Environment;
using Unity.Netcode;
using UnityEngine;

namespace Manager
{
    public class ItemBoxManager : NetworkBehaviour
    {
        [SerializeField] private GameObject itemBoxPrefab;
        [SerializeField] private Transform[] spawnPoints;

        [SerializeField] private float respawnTime = 15f;

        private Dictionary<Transform, GameObject> activeBoxes = new Dictionary<Transform, GameObject>();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                SpawnAllBoxes();
            }
        }

        private void SpawnAllBoxes()
        {
            foreach (var point in spawnPoints)
            {
                if (point == null || itemBoxPrefab == null)
                    continue;

                if (!activeBoxes.ContainsKey(point) || activeBoxes[point] == null)
                {
                    GameObject box = Instantiate(itemBoxPrefab, point.position, Quaternion.identity);
                    NetworkObject netObj = box.GetComponent<NetworkObject>();
                    if (netObj != null)
                    {
                        netObj.Spawn();
                        activeBoxes[point] = box;

                        var despawner = box.AddComponent<ItemBoxDespawnWatcher>();
                        despawner.Initialize(this, point);
                    }
                }
            }
        }

        public void OnBoxDestroyed(Transform point)
        {
            if (!IsServer) return;

            activeBoxes[point] = null;
            StartCoroutine(RespawnAfterDelay(point));
        }

        private System.Collections.IEnumerator RespawnAfterDelay(Transform point)
        {
            yield return new WaitForSeconds(respawnTime);
            if (point != null)
            {
                GameObject box = Instantiate(itemBoxPrefab, point.position, Quaternion.identity);
                NetworkObject netObj = box.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                    activeBoxes[point] = box;

                    var despawner = box.AddComponent<ItemBoxDespawnWatcher>();
                    despawner.Initialize(this, point);
                }
            }
        }
    }
}
