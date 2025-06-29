using System.Collections;
using Enemy;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Manager
{
    public class WaveManager : NetworkBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float waveDelay = 5f;
        [SerializeField] private int enemiesPerWave = 3;

        private int currentWave;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                StartCoroutine(WaveLoop());
            }
        }

        private IEnumerator WaveLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(waveDelay);
                currentWave++;
                SpawnWave(currentWave);
            }
        }

        private void SpawnWave(int waveNumber)
        {
            Debug.Log($"Wave {waveNumber} starting!");

            for (int i = 0; i < enemiesPerWave; i++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

                GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                enemyInstance.GetComponent<NetworkObject>().Spawn();

                var model = new EnemyModel(50, 1.5f, 10, 1.2f); // hp, speed, damage, cooldown
                var behavior = new ChasePlayer();
                enemyInstance.GetComponent<EnemyController>().Initialize(model, behavior);
                enemyInstance.GetComponent<NavMeshAgent>().enabled = true;
            }
        }
    }
}
