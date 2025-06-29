using Unity.Netcode;
using UnityEngine;

namespace Enemy
{
    public class EnemyView : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Die1 = Animator.StringToHash("Die");

        public Rigidbody2D rb;
        public SpriteRenderer spriteRenderer;

        [SerializeField] private Animator animator;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void PlayHitEffect()
        {
        }

        public void Die()
        {
            Destroy(gameObject);
        }
        
        public void PlayWalkAnimation(Vector2 direction)
        {
            animator.SetFloat(MoveX, Mathf.Abs(direction.x));
            animator.SetFloat(MoveY, direction.y);
            animator.SetFloat(Speed, direction.sqrMagnitude);
            Debug.Log(direction);
            if (direction.x != 0)
                spriteRenderer.flipX = direction.x > 0;
        }

        public void PlayAttackAnimation()
        {
            //animator.SetTrigger(Attack);
        }

        public void PlayDeathAnimation()
        {
            //animator.SetTrigger(Die1);
        }
    }
}