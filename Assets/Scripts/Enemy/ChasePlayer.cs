using UnityEngine;

namespace Enemy
{
    public class ChasePlayer : IEnemyBehavior
    {
        public void Execute(EnemyController controller)
        {
            Transform closestPlayer = controller.FindClosestPlayer();
            if (closestPlayer != null)
            {
                Vector2 dir = (closestPlayer.position - controller.transform.position).normalized;
                controller.Move(dir);
                controller.TryAttack(closestPlayer);
            }
        }
    }
}
