using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using CameraBehavior;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Player & Camera")]
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private GameObject playerPrefab;

        [Header("Enemies for Pooling")]
        [SerializeField] private GameObject skinnyZombiePrefab;
        [SerializeField] private GameObject babyZombiePrefab;
        [SerializeField] private GameObject bigZombiePrefab;
        [SerializeField] private GameObject turretZombiePrefab;
        [SerializeField] private GameObject bossZombiePrefab;

        [Header("Projectiles for Pooling")]
        [SerializeField] private GameObject playerBulletPrefab;
        [SerializeField] private GameObject enemyProjectilePrefab;

        private Camera _camera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            SpawnCameraOnce();

            SceneManager.sceneLoaded += OnSceneLoaded;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void Start()
        {
            PreloadPools();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }

        #region Pool Preload
        private void PreloadPools()
        {
            if (NetworkPoolManager.Instance == null) return;

            // Enemies
            NetworkPoolManager.Instance.Preload(skinnyZombiePrefab, 50);
            NetworkPoolManager.Instance.Preload(babyZombiePrefab, 50);
            NetworkPoolManager.Instance.Preload(bigZombiePrefab, 15);
            NetworkPoolManager.Instance.Preload(turretZombiePrefab, 20);
            NetworkPoolManager.Instance.Preload(bossZombiePrefab, 1);

            // Projectiles
            NetworkPoolManager.Instance.Preload(playerBulletPrefab, 50);
            NetworkPoolManager.Instance.Preload(enemyProjectilePrefab, 50);

            Debug.Log("[GameManager] All prefabs preloaded in NetworkPoolManager");
        }
        #endregion

        #region Camera & Player
        private void SpawnCameraOnce()
        
        {
            if (_camera != null) return;

            GameObject cam = Instantiate(cameraPrefab);
            _camera = cam.GetComponent<Camera>();
            DontDestroyOnLoad(cam);
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            Debug.Log($"Client connected: {clientId}");
            if (SceneManager.GetActiveScene().name == "Level")
            {
                SpawnPlayerForClient(clientId);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Level" && NetworkManager.Singleton.IsServer)
            {
                SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId);

                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (clientId != NetworkManager.Singleton.LocalClientId)
                    {
                        SpawnPlayerForClient(clientId);
                    }
                }
            }
        }

        private void SpawnPlayerForClient(ulong clientId)
        {
            NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObject == null)
            {
                Debug.Log($"Spawning player for client {clientId}");
                var playerInstance = Instantiate(playerPrefab);
                var networkObject = playerInstance.GetComponent<NetworkObject>();
                networkObject.SpawnAsPlayerObject(clientId);
            }
            else
            {
                Debug.Log($"Player already exists for client {clientId}");
            }
        }

        public Camera GetCamera()
        {
            return _camera;
        }

        public void AssignCameraToPlayer(Transform playerTransform)
        {
            if (_camera != null && _camera.TryGetComponent(out CameraBehaviour cameraFollow))
            {
                cameraFollow.Follow(playerTransform);
            }
        }
        #endregion
    }
}
