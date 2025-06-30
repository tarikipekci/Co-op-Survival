using Player;
using UnityEngine;

namespace Enemy
{
    public class ChasePlayer : IEnemyBehavior
    {
        public void Execute(EnemyController controller)
        {
            PlayerController playerController = controller.FindClosestPlayerController();
            Transform closestPlayer = playerController.transform;
            if (closestPlayer != null)
            {
                Vector2 dir = (closestPlayer.position - controller.transform.position).normalized;
                controller.Move(dir);
                controller.TryAttack(closestPlayer, playerController);
            }
        }
    }
}
