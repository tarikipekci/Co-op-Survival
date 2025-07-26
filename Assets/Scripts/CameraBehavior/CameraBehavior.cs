using UnityEngine;

namespace CameraBehavior
{
    public class CameraBehaviour : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 0, -10);
        public float damping = 0.25f;
        private Vector3 _velocity = Vector3.zero;

        public float minX = -9f;
        public float maxX = 9f;
        public float minY = -14f;
        public float maxY = 14f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, damping);
        }

        public void Follow(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
