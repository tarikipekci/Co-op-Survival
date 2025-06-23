using UnityEngine;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        public void Move(Vector2 direction, float speed)
        {
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));
        }
    }
}
