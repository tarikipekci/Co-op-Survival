using Data;
using Interface;
using Unity.Netcode;
using UnityEngine;

namespace Weapon
{
    public class WeaponBehaviour : NetworkBehaviour, IWeapon
    {
        [SerializeField] protected WeaponData weaponData;
        private Animator animator;

        private static readonly int AttackTrigger = Animator.StringToHash("Attack");

        private SpriteRenderer spriteRenderer;
        
        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
        }

        public virtual void Attack()
        {
            if (IsServer)
            {
                PlayAttackAnimationClientRpc();
            }
            else
            {
                PlayAttackAnimationServerRpc();
            }
        }

        public void UpdateDirection(Vector2 lookDir)
        {
            if (lookDir.sqrMagnitude < 0.01f) return;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            spriteRenderer.flipY = lookDir.x < 0;
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayAttackAnimationServerRpc()
        {
            PlayAttackAnimationClientRpc();
        }

        [ClientRpc]
        private void PlayAttackAnimationClientRpc()
        {
            animator?.SetTrigger(AttackTrigger);
        }

        public Sprite GetIcon() => weaponData.icon;
        public string GetName() => weaponData.weaponName;
    }
}