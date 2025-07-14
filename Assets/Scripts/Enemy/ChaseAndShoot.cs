using Interface;
using Player;
using UnityEngine;

namespace Enemy
{
    public class ChaseAndShoot : IEnemyBehavior
    {
        private const float shootCooldown = 1.5f;
        private float lastShootTime;
        private GameObject projectilePrefab;

        public ChaseAndShoot(GameObject projectilePrefab)
        {
            this.projectilePrefab = projectilePrefab;
        }

        public override void Execute(EnemyController controller)
        {
            if (controller == null) return;

            PlayerController playerController = controller.FindClosestPlayerController();
            if (playerController == null) return;

            Transform target = playerController.transform;
            Vector2 dir = (target.position - controller.transform.position).normalized;

            controller.Move(dir);

            float distance = Vector2.Distance(controller.transform.position, target.position);

            if (distance <= 5f)
            {
                if (Time.time - lastShootTime >= shootCooldown)
                {
                    lastShootTime = Time.time;
                    controller.ShootProjectile(target.position, projectilePrefab, playerController);
                }
            }
            else
            {
                controller.TryAttack(target, playerController);
            }
        }
    }
}