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
            Manager.NetworkPoolManager.Instance.Spawn(prefabToDrop, position, Quaternion.identity);
        }
    }
}
