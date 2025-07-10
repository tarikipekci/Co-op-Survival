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
            base.Attack();
            
            Vector2 direction = transform.right;
            ShootServerRpc(firePoint.position, direction);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ShootServerRpc(Vector2 spawnPosition, Vector2 direction)
        {
            GameObject proj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            var netObj = proj.GetComponent<NetworkObject>();
            var projectile = proj.GetComponent<Projectile>();

            if (netObj != null && projectile != null)
            {
                projectile.damage = weaponData.Damage;
                netObj.Spawn(true);
                projectile.Init(direction);
            }
            else
            {
                Destroy(proj);
            }
        }
    }
}
