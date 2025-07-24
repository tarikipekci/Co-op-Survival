using Enemy;
using Interface;
using UnityEngine;

namespace Weapon
{
    public class Knife : WeaponBehaviour
    {
        public override void Attack()
        {
            if (!IsCooldownOver()) return;
            base.Attack();
            lastAttackTime = Time.time;
            Debug.Log("Knife Slash!");

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position,1.0f);
            foreach (var hit in hits)
            {
                EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamageServerRpc(weaponData.Damage * playerData.Damage.Value);
                    Debug.Log(playerData.Damage.Value);
                }
                else
                {
                    hit.TryGetComponent<ICanTakeDamage>(out var damageable);
                    damageable?.TakeDamage(weaponData.Damage * playerData.Damage.Value);
                }
            }
        }
    }
}