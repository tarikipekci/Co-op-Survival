using Enemy;
using Interface;
using UnityEngine;

namespace Weapon
{
    public class Knife : WeaponBehaviour
    {
        public override void Attack()
        {
            base.Attack();
            Debug.Log("Knife Slash!");

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,1.0f);
            foreach (var hit in hits)
            {
                EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamageServerRpc(weaponData.Damage);
                }
                else
                {
                    hit.TryGetComponent<ICanTakeDamage>(out var damageable);
                    damageable?.TakeDamage(weaponData.Damage);
                }
            }
        }
    }
}