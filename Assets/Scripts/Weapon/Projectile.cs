using Enemy;
using Interface;
using Player;
using Unity.Netcode;
using UnityEngine;

public enum ProjectileOwner
{
    Player,
    Enemy
}

namespace Weapon
{
    public class Projectile : NetworkBehaviour
    {
        public float speed;
        public float lifetime;
        public int damage;
        private Rigidbody2D rb;
        public ProjectileOwner owner;

        private NetworkVariable<Vector2> direction =
            new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            rb = GetComponent<Rigidbody2D>();

            if (IsServer)
            {
                Destroy(gameObject, lifetime);
            }
        }

        private void Update()
        {
            if (rb == null) return;

            rb.linearVelocity = direction.Value.normalized * speed;

            float angle = Mathf.Atan2(direction.Value.y, direction.Value.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void Init(Vector2 dir, ProjectileOwner ownerType)
        {
            direction.Value = dir;
            owner = ownerType;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;

            if (owner == ProjectileOwner.Player)

                if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Projectile"))
                    return;
            if (owner == ProjectileOwner.Enemy)

                if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Projectile"))
                    return;

            if (owner == ProjectileOwner.Player)
            {
                var enemyHealth = other.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamageServerRpc(damage);
                }
                else
                {
                    other.TryGetComponent<ICanTakeDamage>(out var damageable);
                    damageable?.TakeDamage(damage);
                }
            }
            else if (owner == ProjectileOwner.Enemy)
            {
                var playerHealth = other.GetComponentInParent<PlayerHealth>();
                var playerController = other.GetComponentInParent<PlayerController>();
                if (playerHealth != null)
                {
                    playerController.GetView().PlayHitEffectClientRpc();
                    playerHealth.TakeDamageServerRpc(damage);
                }
            }

            if (NetworkObject.IsSpawned)
                NetworkObject.Despawn();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!IsServer) return;
            if (owner == ProjectileOwner.Player)

                if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Projectile"))
                    return;
            if (owner == ProjectileOwner.Enemy)

                if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Projectile"))
                    return;

            if (NetworkObject.IsSpawned)
                NetworkObject.Despawn();
        }
    }
}