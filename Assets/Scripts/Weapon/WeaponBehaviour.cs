using Data;
using Interface;
using Manager;
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
        private Vector2 lastLookDir;

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

        public void SetLookDirection(Vector2 lookDir, ulong weaponManagerId)
        {
            if (!IsOwner) return;

            if (lookDir.sqrMagnitude < 0.01f) return;

            if (lookDir != lastLookDir)
            {
                lastLookDir = lookDir;
                UpdateDirectionServerRpc(lookDir, weaponManagerId);
            }
        }

        private void UpdateDirection(Vector2 lookDir, WeaponManager weaponManager)
        {
            if (lookDir.sqrMagnitude < 0.01f) return;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            spriteRenderer.flipY = lookDir.x < 0;

            if (lookDir.x < 0.0f)
            {
                transform.position = weaponManager.weaponHolderLeft.transform.position;
            }
            else
            {
                transform.position = weaponManager.weaponHolderRight.transform.position;
            }
        }

        [ServerRpc]
        private void UpdateDirectionServerRpc(Vector2 lookDir, ulong managerId)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(managerId, out var obj)) return;

            var weaponManager = obj.GetComponent<WeaponManager>();
            if (weaponManager == null) return;

            UpdateDirection(lookDir, weaponManager);
            UpdateDirectionClientRpc(lookDir, managerId);
        }

        [ClientRpc]
        private void UpdateDirectionClientRpc(Vector2 lookDir, ulong managerId)
        {
            if (IsServer) return;

            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(managerId, out var obj)) return;

            var weaponManager = obj.GetComponent<WeaponManager>();
            if (weaponManager == null) return;

            UpdateDirection(lookDir, weaponManager);
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