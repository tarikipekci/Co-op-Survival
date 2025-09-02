using Interface;
using Unity.Netcode;
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class PickupItem : NetworkBehaviour
    {
        [SerializeField] private float lifetime = 10f;
        private bool applyResult;

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

            if (other.CompareTag("Player") && !applyResult)
            {
                Debug.Log("player entered");
                if (TryGetComponent<IPickupEffect>(out var effect))
                {
                    applyResult = effect.Apply(other.gameObject);
                    Debug.Log("Picked up effect");
                }

                if (applyResult && NetworkObject != null && NetworkObject.IsSpawned)
                {
                    Manager.NetworkPoolManager.Instance.Despawn(NetworkObject);
                }
            }
        }

        private void SelfDestruct()
        {
            if (!IsServer) return;
            if (NetworkObject != null && NetworkObject.IsSpawned)
                Manager.NetworkPoolManager.Instance.Despawn(NetworkObject);
        }

        private void OnEnable()
        {
            if (IsServer)
            {
                Invoke(nameof(SelfDestruct), lifetime);
            }
        }

        private void OnDisable()
        {
            if (IsServer)
            {
                CancelInvoke(nameof(SelfDestruct));
            }
        }
    }
}
