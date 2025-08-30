namespace Enemy
{
    public class EnemyModel
    {
        public int Health { get; }
        public float Speed { get; }
        public int Damage { get; }
        
        public float AttackRate { get; }

        public EnemyModel(int health, float speed, int damage, float attackRate)
        {
            Health = health;
            Speed = speed;
            Damage = damage;
            AttackRate = attackRate;
        }
    }
}