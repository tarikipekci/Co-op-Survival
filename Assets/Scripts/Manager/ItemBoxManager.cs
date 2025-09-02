using System.Collections;
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

        private readonly Dictionary<Transform, NetworkObject> activeBoxes = new();
        private readonly HashSet<Transform> spawningPoints = new();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                StartCoroutine(SpawnAllBoxesWithDelay());
        }

        private IEnumerator SpawnAllBoxesWithDelay()
        {
            yield return new WaitForSeconds(2f);
            SpawnAllBoxes();
        }

        private void SpawnAllBoxes()
        {
            foreach (var point in spawnPoints)
            {
                if (point == null || itemBoxPrefab == null) continue;
                if (!activeBoxes.ContainsKey(point) && !spawningPoints.Contains(point))
                    StartCoroutine(SpawnBoxAtDeferred(point));
            }
        }

        private IEnumerator SpawnBoxAtDeferred(Transform point)
        {
            if (point == null || itemBoxPrefab == null) yield break;
            if (spawningPoints.Contains(point)) yield break;

            spawningPoints.Add(point);

            NetworkObject netObj = NetworkPoolManager.Instance.Spawn(itemBoxPrefab, point.position, Quaternion.identity);
            if (netObj == null)
            {
                spawningPoints.Remove(point);
                yield break;
            }

            yield return null;

            if (netObj == null)
            {
                spawningPoints.Remove(point);
                yield break;
            }

            activeBoxes[point] = netObj;

            var watcher = netObj.GetComponent<ItemBoxDespawnWatcher>();
            watcher?.Initialize(this, point);

            spawningPoints.Remove(point);
        }

        public void OnBoxDestroyed(Transform point)
        {
            if (!IsServer) return;

            if (activeBoxes.TryGetValue(point, out var netObj))
            {
                if (netObj != null && netObj.IsSpawned)
                    NetworkPoolManager.Instance.Despawn(netObj);

                activeBoxes.Remove(point);
            }

            if (!spawningPoints.Contains(point))
                StartCoroutine(RespawnAfterDelay(point));
        }

        private IEnumerator RespawnAfterDelay(Transform point)
        {
            yield return new WaitForSeconds(respawnTime);
            StartCoroutine(SpawnBoxAtDeferred(point));
        }
    }
}
