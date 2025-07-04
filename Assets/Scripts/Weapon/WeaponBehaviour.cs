using Data;
using Interface;
using UnityEngine;

namespace Weapon
{
    public abstract class WeaponBehaviour : MonoBehaviour, IWeapon
    {
        [SerializeField] protected WeaponData weaponData;
        protected Animator animator;
        protected static readonly int AttackTrigger = Animator.StringToHash("Attack");

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
        }

        public virtual void Attack()
        {
            Debug.Log("Base attack");
        }

        public void UpdateDirection(Vector2 lookDir, Transform WeaponPoint)
        {
            if (lookDir.sqrMagnitude < 0.01f)
                return;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (lookDir.x < 0)
            {
                spriteRenderer.flipY = true;
            }
            else
            {
                spriteRenderer.flipY = false;
            }

            transform.position = WeaponPoint.position;
        }

        public Sprite GetIcon() => weaponData.icon;
        public string GetName() => weaponData.weaponName;
    }
}