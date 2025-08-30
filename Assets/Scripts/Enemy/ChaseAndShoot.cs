using Interface;
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

            controller.UpdateTarget();
            if (controller.GetCachedTarget() == null) return;

            Transform target = controller.GetCachedTarget().transform;

            controller.Move();

            float distance = Vector2.Distance(controller.transform.position, target.position);

            if (distance <= 5f)
            {
                if (Time.time - lastShootTime >= shootCooldown)
                {
                    lastShootTime = Time.time;
                    controller.ShootProjectile(target.position, projectilePrefab);
                }
            }
            else
            {
                controller.TryAttack(target, controller.GetCachedTarget());
            }
        }
    }
}
