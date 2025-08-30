using Interface;
using UnityEngine;

namespace Enemy
{
    public class ChasePlayer : IEnemyBehavior
    {
        public override void Execute(EnemyController controller)
        {
            if (controller == null) return;

            controller.UpdateTarget();
            if (controller.GetCachedTarget() == null) return;

            Transform targetTransform = controller.GetCachedTarget().transform;
            if (targetTransform != null)
            {
                controller.Move();
                controller.TryAttack(targetTransform, controller.GetCachedTarget());
            }
        }
    }
}
