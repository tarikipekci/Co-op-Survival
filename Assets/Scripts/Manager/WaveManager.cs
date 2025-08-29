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
        public static WaveManager Instance { get; private set; }

        [SerializeField] private List<WaveData> waves;
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private DayNightManager dayNightManager;
        [HideInInspector] public EnemyHealth currentBossHealth;

        private float timer;
        private bool[] waveStarted;

        public System.Action<int, int> OnBossTakeDamage;

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

                    HandleNightTransition(waves[i].isNight);
                }
            }
        }

        private void HandleNightTransition(bool isNight)
        {
            if (isNight)
                dayNightManager.SetNight();
            else
                dayNightManager.SetDay();
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
                    SpawnEnemy(enemy.enemyPrefab, false);
                    yield return new WaitForSeconds(0.3f);
                }
            }

            if (boss != null)
            {
                yield return new WaitForSeconds(3f);
                for (int i = 0; i < boss.count; i++)
                {
                    SpawnEnemy(boss.enemyPrefab, true);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        private void SpawnEnemy(GameObject prefab, bool isBoss)
        {
            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
            GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            var netObj = enemy.GetComponent<NetworkObject>();
            if (netObj != null) netObj.Spawn();

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
                behavior = new ChaseAndShoot(rangedStats.projectilePrefab);
            else
                behavior = new ChasePlayer();

            var controller = enemy.GetComponent<EnemyController>();
            controller?.Initialize(model, behavior);

            var health = enemy.GetComponent<EnemyHealth>();
            health?.SetMaxHealth(model.Health);

            if (isBoss)
            {
                currentBossHealth = health;
                SetBossBarActiveClientRpc(true);
            }

            var agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = true;
        }

        [ClientRpc]
        public void SetBossBarActiveClientRpc(bool isActive)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.SetBossBarActive(isActive);
        }

        [ClientRpc]
        public void UpdateBossHealthClientRpc(int currentHealth, int maxHealth)
        {
            OnBossTakeDamage?.Invoke(currentHealth, maxHealth);
        }
    }
}