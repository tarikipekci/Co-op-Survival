using UnityEngine;

namespace CameraBehavior
{
    public class CameraBehaviour : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 0, -10);
        public float damping = 0.25f;
        private Vector3 _velocity = Vector3.zero;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, damping);
        }

        public void Follow(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
