using Data;
using Interface;
using Manager;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Weapon
{
    public class WeaponBehaviour : NetworkBehaviour, IWeapon
    {
        [SerializeField] protected WeaponData weaponData;
        private Animator animator;
        protected PlayerData playerData;
        private SpriteRenderer spriteRenderer;

        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private Vector2 lastLookDir;
        protected float lastAttackTime;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            var myClientId = NetworkManager.Singleton.LocalClientId;
            playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(myClientId);
        }

        public void SetLookDirection(Vector2 lookDir)
        {
            if (lookDir.sqrMagnitude < 0.01f) return;
            if (this == null || gameObject == null || transform == null) return;

            if (lookDir != lastLookDir)
            {
                lastLookDir = lookDir;
                UpdateDirection(lookDir);
            }
        }

        private void UpdateDirection(Vector2 lookDir)
        {
            if (this == null || gameObject == null || transform == null || spriteRenderer == null) return;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            spriteRenderer.flipY = lookDir.x < 0;

            var manager = GetComponentInParent<WeaponManager>();
            if (manager == null || manager.weaponHolderLeft == null || manager.weaponHolderRight == null) return;

            transform.position = (lookDir.x < 0)
                ? manager.weaponHolderLeft.position
                : manager.weaponHolderRight.position;
        }

        public virtual void Attack()
        {
            if (!IsCooldownOver()) return;

            lastAttackTime = Time.time;
            animator?.SetTrigger(AttackTrigger);
        }

        protected bool IsCooldownOver()
        {
            if (playerData == null) return true;
            return Time.time >= lastAttackTime + weaponData.coolDown * playerData.AttackRate.Value;
        }

        public Sprite GetIcon() => weaponData.icon;
        public string GetName() => weaponData.weaponName;
    }
}