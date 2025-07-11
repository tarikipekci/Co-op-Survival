using Unity.Netcode;
using UnityEngine;

namespace Weapon
{
    public class Shotgun : WeaponBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private int pelletCount = 6;
        [SerializeField] private float spreadAngle = 15f;

        public override void Attack()
        {
            if (!IsCooldownOver()) return;
            base.Attack();
            lastAttackTime = Time.time;

            for (int i = 0; i < pelletCount; i++)
            {
                float angleOffset = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
                Vector2 baseDirection = transform.right;
                Vector2 spreadDirection = Quaternion.Euler(0, 0, angleOffset) * baseDirection;

                ShootServerRpc(firePoint.position, spreadDirection);
            }
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
