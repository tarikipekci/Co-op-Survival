using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class WaveData
    {
        public float startTime; 
        public List<EnemyWaveInfo> baseEnemies;
        public bool isNight;
        public EnemyWaveInfo boss;

        public List<EnemyWaveInfo> GetScaledEnemies(int playerCount)
        {
            float multiplier = GetMultiplier(playerCount);
            List<EnemyWaveInfo> scaled = new List<EnemyWaveInfo>();

            foreach (var e in baseEnemies)
            {
                int scaledCount = Mathf.CeilToInt(e.count * multiplier);
                scaled.Add(new EnemyWaveInfo
                {
                    enemyPrefab = e.enemyPrefab,
                    count = scaledCount
                });
            }

            return scaled;
        }

        public EnemyWaveInfo GetScaledBoss(int playerCount)
        {
            if (boss == null || boss.enemyPrefab == null || boss.count <= 0)
                return null;

            float multiplier = GetMultiplier(playerCount);
            int scaledCount = Mathf.CeilToInt(boss.count * multiplier * 0.5f); 

            return new EnemyWaveInfo
            {
                enemyPrefab = boss.enemyPrefab,
                count = scaledCount
            };
        }

        private float GetMultiplier(int players)
        {
            if (players <= 1) return 1f;
            return 1f + ((players - 1) * 0.5f); // 1→1x, 2→1.5x, 3→2x, 4→2.5x
        }
    }
}
