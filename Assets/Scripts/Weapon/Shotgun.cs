using Unity.Netcode;
using UnityEngine;
using Manager;

namespace Weapon
{
    public class Shotgun : WeaponBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private int pelletCount = 3;
        [SerializeField] private float spreadAngle = 30f;

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
                int totalDamage = Mathf.RoundToInt(weaponData.Damage * playerData.Damage.Value);

                ShootServerRpc(firePoint.position, spreadDirection, totalDamage);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ShootServerRpc(Vector2 spawnPosition, Vector2 direction, int damage)
        {
            if (NetworkPoolManager.Instance == null) return;

            var netObj = NetworkPoolManager.Instance.Spawn(projectilePrefab, spawnPosition, Quaternion.identity);
            if (netObj != null)
            {
                var projectile = netObj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Init(direction, ProjectileOwner.Player, damage);
                }
                else
                {
                    NetworkPoolManager.Instance.Despawn(netObj);
                }
            }
        }
    }
}
