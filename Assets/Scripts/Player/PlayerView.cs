using UnityEngine;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        [SerializeField] private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void Move(Vector2 direction, float speed)
        {
            rb.linearVelocity = direction * speed;

            // Flip + Animator
            if (direction.x != 0)
                spriteRenderer.flipX = direction.x < 0;

            if (animator != null)
            {
                animator.SetFloat(MoveX, Mathf.Abs(rb.linearVelocityX));
                animator.SetFloat(MoveY, rb.linearVelocityY);
                Debug.Log("X" + rb.linearVelocityX + "Y" + rb.linearVelocityY);
            }
        }
    }
}