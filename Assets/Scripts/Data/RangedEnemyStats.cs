using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemies/Ranged Enemy")]
    public class RangedEnemyStats : EnemyStatsSO
    {
        //Ranged enemy exclusive
        public GameObject projectilePrefab;
    }
}
