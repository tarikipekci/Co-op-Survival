using System.Collections;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Manager
{
    public class RespawnManager : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private void OnEnable()
        {
            PlayerHealth.OnPlayerDied += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            PlayerHealth.OnPlayerDied -= HandlePlayerDeath;
        }

        private void HandlePlayerDeath(ulong clientId)
        {
            StartCoroutine(RespawnAfterDelay(clientId, 5f)); 
        }

        private IEnumerator RespawnAfterDelay(ulong clientId, float delay)
        {
            yield return new WaitForSeconds(delay);
            RespawnPlayer(clientId);
        }

        private void RespawnPlayer(ulong clientId)
        {
            if (!IsServer) return;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            var playerHealth = playerInstance.GetComponent<PlayerHealth>();
            playerHealth.FullHealth();
        }
    }
}
