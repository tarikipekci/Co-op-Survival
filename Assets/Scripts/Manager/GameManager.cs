using CameraBehavior;
using UnityEngine;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameObject cameraPrefab;

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
        }

        public Camera GetCamera()
        {
            return _camera;
        }
        
        private void SpawnCameraOnce()
        {
            if (_camera != null) return;

            GameObject cam = Instantiate(cameraPrefab);
             _camera = cam.GetComponent<Camera>();
            DontDestroyOnLoad(_camera);
        }

        public void AssignCameraToPlayer(Transform playerTransform)
        {
            if (_camera != null)
            {
                _camera.TryGetComponent(out CameraBehaviour cameraFollow);
                cameraFollow.Follow(playerTransform);
            }
        }
    }
}