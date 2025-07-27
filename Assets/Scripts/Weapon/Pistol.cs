using UnityEngine;
using Unity.Netcode;

namespace Weapon
{
    public class Pistol : WeaponBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        public override void Attack()
        {
            if (!IsCooldownOver()) return;
            base.Attack();
            lastAttackTime = Time.time;
            
            Vector2 direction = transform.right;
            int totalDamage = Mathf.RoundToInt(weaponData.Damage * playerData.Damage.Value);
            ShootServerRpc(firePoint.position, direction, totalDamage);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ShootServerRpc(Vector2 spawnPosition, Vector2 direction, int damage)
        {
            GameObject proj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            var netObj = proj.GetComponent<NetworkObject>();
            var projectile = proj.GetComponent<Projectile>();

            if (netObj != null && projectile != null)
            {
                netObj.Spawn(true);
                projectile.Init(direction, ProjectileOwner.Player, damage);
            }
            else
            {
                Destroy(proj);
            }
        }
    }
}
