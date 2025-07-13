using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemies/Enemy Stats")]
    public class EnemyStatsSO : ScriptableObject
    {
        public int health;
        public float speed;
        public int damage;
        public float attackRate;
    }
}
