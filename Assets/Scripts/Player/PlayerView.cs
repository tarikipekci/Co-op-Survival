using Manager;
using UnityEngine;
using Unity.Netcode;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private WeaponManager weaponManager;
        private PlayerFlashlightController flashlightController;

        private Vector2 targetLookDir;
        private Vector2 smoothLookDir;

        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Hit = Animator.StringToHash("Hit");

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            weaponManager = GetComponentInChildren<WeaponManager>();
            flashlightController = GetComponent<PlayerFlashlightController>();
        }

        private void Start()
        {
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null && !controller.IsOwner)
            {
                controller.LookDir.OnValueChanged += (oldVal, newVal) =>
                {
                    if (oldVal != newVal)
                        targetLookDir = newVal;
                };
            }
        }

        public void UpdateLookDirection(Vector2 lookDir, Vector2 moveInput)
        {
            smoothLookDir = lookDir;

            if (spriteRenderer != null && Mathf.Abs(smoothLookDir.x) > 0.01f)
                spriteRenderer.flipX = smoothLookDir.x < 0;

            if (animator != null)
            {
                float speed = moveInput.sqrMagnitude;
                if (speed > 0.01f)
                {
                    animator.SetFloat(MoveX, Mathf.Abs(smoothLookDir.x));
                    animator.SetFloat(MoveY, smoothLookDir.y);
                    animator.SetFloat(Speed, speed);
                }
                else
                {
                    animator.SetFloat(MoveX, 0);
                    animator.SetFloat(MoveY, 0);
                    animator.SetFloat(Speed, 0);
                }
            }

            weaponManager?.GetCurrentWeapon()?.SetLookDirection(smoothLookDir);
            flashlightController?.UpdateLookDirection(smoothLookDir);
        }

        private void Update()
        {
            PlayerController controller = GetComponent<PlayerController>();
            if (controller == null || controller.IsOwner) return;

            smoothLookDir = Vector2.Lerp(smoothLookDir, targetLookDir, Time.deltaTime * 30f);
            UpdateLookDirection(smoothLookDir, controller.MoveInput.Value);
        }

        private void PlayHitEffect()
        {
            if (animator != null) animator.SetTrigger(Hit);
        }

        [ClientRpc]
        public void PlayHitEffectClientRpc()
        {
            PlayHitEffect();
        }

        public SpriteRenderer GetSpriteRenderer()
        {
            return spriteRenderer;
        }
    }
}
