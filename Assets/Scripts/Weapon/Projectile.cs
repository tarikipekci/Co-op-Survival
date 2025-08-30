using Manager;
using Unity.Netcode;
using UnityEngine;

public enum ProjectileOwner { Player, Enemy }

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
                StartCoroutine(LifetimeCoroutine());
        }

        private System.Collections.IEnumerator LifetimeCoroutine()
        {
            yield return new WaitForSeconds(lifetime);
            Despawn();
        }

        private void Update()
        {
            if (rb == null) return;
            rb.linearVelocity = direction.Value.normalized * speed;

            float angle = Mathf.Atan2(direction.Value.y, direction.Value.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void Init(Vector2 dir, ProjectileOwner ownerType, int damageValue)
        {
            direction.Value = dir;
            owner = ownerType;
            damage = damageValue;

            ResetAnimation();
        }

        private void ResetAnimation()
        {
            var anim = GetComponent<Animator>();
            if (anim != null)
                anim.Play(0, -1, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;
            if (ShouldIgnoreCollision(other)) return;

            if (owner == ProjectileOwner.Player)
            {
                var enemyHealth = other.GetComponentInParent<Enemy.EnemyHealth>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamageServerRpc(damage);
                else
                {
                    other.TryGetComponent<Interface.ICanTakeDamage>(out var damageable);
                    damageable?.TakeDamage(damage);
                }
            }
            else if (owner == ProjectileOwner.Enemy)
            {
                var playerHealth = other.GetComponentInParent<Player.PlayerHealth>();
                var playerController = other.GetComponentInParent<Player.PlayerController>();
                if (playerHealth != null)
                {
                    playerController.GetView().PlayHitEffectClientRpc();
                    playerHealth.TakeDamageServerRpc(damage);
                }
            }

            Despawn();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!IsServer) return;
            if (ShouldIgnoreCollision(other.collider)) return;

            Despawn();
        }

        private bool ShouldIgnoreCollision(Collider2D col)
        {
            if (owner == ProjectileOwner.Player && (col.CompareTag("Player") || col.CompareTag("Projectile")))
                return true;
            if (owner == ProjectileOwner.Enemy && (col.CompareTag("Enemy") || col.CompareTag("Projectile")))
                return true;
            return false;
        }

        private void Despawn()
        {
            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkPoolManager.Instance.Despawn(NetworkObject);
        }
    }
}
