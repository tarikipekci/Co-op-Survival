using Manager;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Hit = Animator.StringToHash("Hit");

        [SerializeField] private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private WeaponManager weaponManager;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            weaponManager = GetComponentInChildren<WeaponManager>();
        }

        public void Move(Vector2 direction, float speed, Vector2 lookDir)
        {
            rb.linearVelocity = direction * speed;

            if (Mathf.Abs(lookDir.x) > 0.01f)
                spriteRenderer.flipX = lookDir.x < 0;

            if (animator != null)
            {
                if (rb.linearVelocity.sqrMagnitude > 0.1f)
                {
                    animator.SetFloat(MoveX, Mathf.Abs(lookDir.x));
                    animator.SetFloat(MoveY, lookDir.y);
                    animator.SetFloat(Speed, direction.sqrMagnitude);

                    if (lookDir.x < 0)
                    {
                        weaponManager.GetCurrentWeapon().UpdateDirection(lookDir, weaponManager.weaponHolderRight);
                    }
                    else
                    {
                         weaponManager.GetCurrentWeapon().UpdateDirection(lookDir, weaponManager.weaponHolderLeft);
                    }
                }
                else
                {
                    animator.SetFloat(MoveX, 0);
                    animator.SetFloat(MoveY, 0);
                    animator.SetFloat(Speed, 0);
                    weaponManager.GetCurrentWeapon().UpdateDirection(Vector2.zero, weaponManager.weaponHolderRight);
                }
            }
        }

        private void PlayHitEffect()
        {
            if (animator != null)
            {
                animator.SetTrigger(Hit);
            }
        }

        [ClientRpc]
        public void PlayHitEffectClientRpc()
        {
            PlayHitEffect();
        }
    }
}