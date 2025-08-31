using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Manager
{
    public class NetworkPoolManager : NetworkBehaviour
    {
        public static NetworkPoolManager Instance { get; private set; }

        private Dictionary<GameObject, Queue<NetworkObject>> pools = new();
        private Dictionary<NetworkObject, GameObject> prefabMap = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Preload(GameObject prefab, int count)
        {
            if (!IsServer) return;

            if (!pools.ContainsKey(prefab))
                pools[prefab] = new Queue<NetworkObject>();

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);

                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                if (netObj == null)
                {
                    Debug.LogError($"Prefab {prefab.name} missing NetworkObject component!");
                    Destroy(obj);
                    continue;
                }

                prefabMap[netObj] = prefab;
                pools[prefab].Enqueue(netObj);
            }
        }

        public NetworkObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!IsServer) return null;

            if (!pools.ContainsKey(prefab))
                pools[prefab] = new Queue<NetworkObject>();

            NetworkObject obj;

            if (pools[prefab].Count > 0)
            {
                obj = pools[prefab].Dequeue();
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.gameObject.SetActive(true);

                if (!obj.IsSpawned)
                    obj.Spawn(true); 
            }
            else
            {
                GameObject go = Instantiate(prefab, position, rotation);
                obj = go.GetComponent<NetworkObject>();
                if (obj != null && !obj.IsSpawned)
                    obj.Spawn(true);

                prefabMap[obj] = prefab;
            }

            return obj;
        }

        public void Despawn(NetworkObject obj)
        {
            if (!IsServer || obj == null) return;

            if (obj.IsSpawned)
                obj.Despawn(false); 

            obj.gameObject.SetActive(false);

            if (!prefabMap.TryGetValue(obj, out GameObject prefab))
                prefab = obj.gameObject;

            if (!pools.ContainsKey(prefab))
                pools[prefab] = new Queue<NetworkObject>();

            pools[prefab].Enqueue(obj);
        }
    }
}
