using Interface;
using Player;
using UnityEngine;

namespace Enemy
{
    public class ChasePlayer : IEnemyBehavior
    {
        public override void Execute(EnemyController controller)
        {
            if (controller == null) return;

            PlayerController playerController = controller.FindClosestPlayerController();

            if (playerController != null)
            {
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
}