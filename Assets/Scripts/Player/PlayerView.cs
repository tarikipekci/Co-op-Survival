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
        private PlayerFlashlightController _playerFlashlightController;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            weaponManager = GetComponentInChildren<WeaponManager>();
            _playerFlashlightController = GetComponent<PlayerFlashlightController>();

            if (spriteRenderer == null) Debug.LogError("SpriteRenderer not found!");
            if (rb == null) Debug.LogError("Rigidbody2D not found!");
            if (weaponManager == null) Debug.LogError("WeaponManager not found!");
            if (_playerFlashlightController == null) Debug.LogError("PlayerFlashlightController not found!");
            if (animator == null) Debug.LogError("Animator not found!");
        }

        public void Move(Vector2 direction, float speed, Vector2 lookDir)
        {
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }

            if (spriteRenderer != null && Mathf.Abs(lookDir.x) > 0.01f)
            {
                spriteRenderer.flipX = lookDir.x < 0;
            }

            if (animator != null)
            {
                if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
                {
                    animator.SetFloat(MoveX, Mathf.Abs(lookDir.x));
                    animator.SetFloat(MoveY, lookDir.y);
                    animator.SetFloat(Speed, direction.sqrMagnitude);
                }
                else
                {
                    animator.SetFloat(MoveX, 0);
                    animator.SetFloat(MoveY, 0);
                    animator.SetFloat(Speed, 0);
                }
            }

            if (weaponManager != null)
            {
                var weapon = weaponManager.GetCurrentWeapon();
                if (weapon != null)
                {
                    weapon.SetLookDirection(lookDir, weaponManager.NetworkObject.NetworkObjectId);
                }
            }

            if (_playerFlashlightController != null)
            {
                _playerFlashlightController.UpdateLookDirection(lookDir);
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