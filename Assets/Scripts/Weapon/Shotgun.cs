using UnityEngine;

namespace Weapon
{
    public class Shotgun : WeaponBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        public override void Attack()
        {
            if (!IsCooldownOver()) return;
            base.Attack();
            lastAttackTime = Time.time;
            Debug.Log("Shotgun attack!");
        }
    }
}
