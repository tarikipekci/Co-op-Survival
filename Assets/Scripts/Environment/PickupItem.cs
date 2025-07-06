using Interface;
using Unity.Netcode;
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupItem : NetworkBehaviour
    {
        [SerializeField] private float lifetime = 10f;

        private void Start()
        {
            if (IsServer)
            {
                Invoke(nameof(SelfDestruct), lifetime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;

            if (other.CompareTag("Player"))
            {
                Debug.Log("player entered");
                if (TryGetComponent<IPickupEffect>(out var effect))
                {
                    effect.Apply(other.gameObject);
                    Debug.Log("Picked up effect");
                }

                if (NetworkObject.IsSpawned)
                    NetworkObject.Despawn();
                else
                    Destroy(gameObject);
            }
        }

        private void SelfDestruct()
        {
            if (NetworkObject.IsSpawned)
                NetworkObject.Despawn();
            else
                Destroy(gameObject);
        }
    }
}
