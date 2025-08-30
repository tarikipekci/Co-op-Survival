using UnityEngine;
using Unity.Netcode;
using Manager;

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
            if (NetworkPoolManager.Instance == null) return;

            var projObj = NetworkPoolManager.Instance.Spawn(projectilePrefab, spawnPosition, Quaternion.identity);
            var projectile = projObj.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.Init(direction, ProjectileOwner.Player, damage);
            }
            else
            {
                NetworkPoolManager.Instance.Despawn(projObj.GetComponent<NetworkObject>());
            }
        }
    }
}
