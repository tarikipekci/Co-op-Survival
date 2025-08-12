using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using CameraBehavior;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private GameObject playerPrefab;

        private Camera _camera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SpawnCameraOnce();

            SceneManager.sceneLoaded += OnSceneLoaded;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
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

        private void SpawnCameraOnce()
        {
            if (_camera != null) return;

            GameObject cam = Instantiate(cameraPrefab);
            _camera = cam.GetComponent<Camera>();
            DontDestroyOnLoad(cam);
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
    }
}