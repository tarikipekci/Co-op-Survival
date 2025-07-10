using Enemy;
using Unity.Netcode;
using UnityEngine;

namespace Weapon
{
    public class Projectile : NetworkBehaviour
    {
        public float speed;
        public float lifetime;
        public int damage;
        private Rigidbody2D rb;

        private NetworkVariable<Vector2> direction = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Server);

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

        public void Init(Vector2 dir)
        {
            direction.Value = dir; 
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;

            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamageServerRpc(damage);
            }

            if (NetworkObject.IsSpawned)
                NetworkObject.Despawn();
        }
    }
}
