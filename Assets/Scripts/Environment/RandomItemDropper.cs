using Interface;
using Unity.Netcode;
using UnityEngine;

namespace Environment
{
    public class RandomItemDropper : MonoBehaviour, IDropProvider
    {
        [SerializeField] private GameObject[] possibleItems;

        public void Drop(Vector3 position)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (possibleItems == null || possibleItems.Length == 0) return;

            GameObject prefabToDrop = possibleItems[Random.Range(0, possibleItems.Length)];
            GameObject instance = Instantiate(prefabToDrop, position, Quaternion.identity);

            if (instance.TryGetComponent<NetworkObject>(out var netObj))
            {
                netObj.Spawn();
            }
        }
    }
}
