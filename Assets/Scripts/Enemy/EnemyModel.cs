using UnityEngine;

namespace Enemy
{
    public class EnemyModel
    {
        public float MaxHealth;
        public float CurrentHealth;
        public float MoveSpeed;
        public float Damage;
        public float AttackCooldown;
        public Transform target;

        public EnemyModel(float maxHealth, float speed, float damage, float cooldown)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            MoveSpeed = speed;
            Damage = damage;
            AttackCooldown = cooldown;
        }
    }
}
