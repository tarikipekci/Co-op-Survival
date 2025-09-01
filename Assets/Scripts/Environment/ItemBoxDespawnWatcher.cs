using UnityEngine;
using Unity.Netcode;

namespace Environment
{
    public class ItemBoxDespawnWatcher : MonoBehaviour
    {
        private Manager.ItemBoxManager manager;
        private Transform spawnPoint;

        public void Initialize(Manager.ItemBoxManager itemBoxManager, Transform point)
        {
            manager = itemBoxManager;
            spawnPoint = point;
        }

        private void OnDisable()
        {
            if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;
            if (manager == null || spawnPoint == null) return;

            manager.OnBoxDestroyed(spawnPoint);
        }
    }
}
