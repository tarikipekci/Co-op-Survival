using System.Collections;
using System.Collections.Generic;
using Data;
using Enemy;
using Interface;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Manager
{
    public class WaveManager : NetworkBehaviour
    {
        [SerializeField] private List<WaveData> waves;
        [SerializeField] private Transform[] enemySpawnPoints;
        
        private float timer;
        private bool[] waveStarted;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            waveStarted = new bool[waves.Count];
            timer = 0f;
        }

        private void Update()
        {
            if (!IsServer) return;

            timer += Time.deltaTime;

            for (int i = 0; i < waves.Count; i++)
            {
                if (!waveStarted[i] && timer >= waves[i].startTime)
                {
                    waveStarted[i] = true;
                    StartCoroutine(RunWave(waves[i]));
                    Debug.Log($"[WaveManager] Wave {i + 1} started at {Mathf.FloorToInt(timer)}s");
                }
            }
        }

        private IEnumerator RunWave(WaveData wave)
        {
            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            var enemies = wave.GetScaledEnemies(playerCount);
            var boss = wave.GetScaledBoss(playerCount);

            foreach (var enemy in enemies)
            {
                for (int i = 0; i < enemy.count; i++)
                {
                    SpawnEnemy(enemy.enemyPrefab);
                    yield return new WaitForSeconds(0.3f);
                }
            }

            if (boss != null)
            {
                yield return new WaitForSeconds(3f);
                for (int i = 0; i < boss.count; i++)
                {
                    SpawnEnemy(boss.enemyPrefab);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        private void SpawnEnemy(GameObject prefab)
        {
            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
            GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            var netObj = enemy.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn();

            var data = enemy.GetComponent<EnemyData>();
            if (data == null || data.stats == null)
            {
                Debug.LogError("Enemy prefab does not contain EnemyData or stats is not assigned.");
                return;
            }

            var stats = data.stats;
            var model = new EnemyModel(stats.health, stats.speed, stats.damage, stats.attackRate);

            IEnemyBehavior behavior;

            if (stats is RangedEnemyStats rangedStats)
            {
                behavior = new ChaseAndShoot(rangedStats.projectilePrefab);
            }
            else if (stats is MeleeEnemyStats)
            {
                behavior = new ChasePlayer();
            }
            else
            {
                Debug.LogWarning("Unknown enemy type. Defaulting to ChasePlayer.");
                behavior = new ChasePlayer();
            }

            var controller = enemy.GetComponent<EnemyController>();
            if (controller != null && behavior != null)
            {
                controller.Initialize(model, behavior);
            }

            var health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.SetMaxHealth(model.Health);
            }

            var agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
            }
        }
    }
}
