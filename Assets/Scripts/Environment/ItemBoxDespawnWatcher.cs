using UnityEngine;

namespace Environment
{
    public class ItemBoxDespawnWatcher : MonoBehaviour
    {
        private Manager.ItemBoxManager manager;
        private Transform spawnPoint;

        public void Initialize(Manager.ItemBoxManager itemBoxManager, Transform point)
        {
            this.manager = itemBoxManager;
            this.spawnPoint = point;
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnBoxDestroyed(spawnPoint);
            }
        }
    }
}
