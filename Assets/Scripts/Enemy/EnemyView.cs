using UnityEngine;

namespace Enemy
{
    public class EnemyView : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Die = Animator.StringToHash("Die");

        public SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject bloodEffect;

        [SerializeField] private Animator animator;

        public void PlayHitEffect()
        {
            animator.SetTrigger(Hit);
        }

        public void PlayWalkAnimation(Vector2 direction)
        {
            animator.SetFloat(MoveX, Mathf.Abs(direction.x));
            animator.SetFloat(MoveY, direction.y);
            animator.SetFloat(Speed, direction.sqrMagnitude);
            //Debug.Log(direction);
            if (direction.x != 0)
                spriteRenderer.flipX = direction.x > 0;
        }

        public void PlayAttackAnimation()
        {
            //animator.SetTrigger(Attack);
        }
        
        public void PlayDeathAnimation()
        {
            animator.SetTrigger(Die);
            var blood = Instantiate(bloodEffect, transform.position, Quaternion.identity);
            Destroy(blood, 1f);
        }
    }
}