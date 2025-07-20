using Manager;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerController : NetworkBehaviour
    {
        private PlayerModel model;
        private PlayerView view;
        private Camera _camera;
        private PlayerData playerData;

        public NetworkVariable<Vector2> LookDirection = new(writePerm: NetworkVariableWritePermission.Owner);

        [SerializeField] private GameObject cameraPrefab;

        public override void OnNetworkSpawn()
        {
            view = GetComponent<PlayerView>();
            if (view == null)
            {
                Debug.LogError("PlayerView component not found!");
            }
        }

        private void Start()
        {
            if (IsOwner)
            {
                GameManager.Instance.AssignCameraToPlayer(transform);
                _camera = GameManager.Instance.GetCamera();
                if (_camera == null)
                {
                    Debug.LogError("Camera could not be assigned!");
                }
            }
            
            playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
            if (playerData == null)
            {
                Debug.LogError($"PlayerData for client {OwnerClientId} not found!");
                return;
            }

            model = new PlayerModel(playerData);
            var health = GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.InitializeHealth(model.MaxHealth);
            }
            else
            {
                Debug.LogError("PlayerHealth component not found!");
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner || _camera == null || playerData == null) return;

            model.MoveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;

            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2 lookDir = (mouseWorldPos - transform.position).normalized;

            LookDirection.Value = lookDir;

            view.Move(model.MoveInput, model.MoveSpeed, lookDir);

            SubmitMovementServerRpc(model.MoveInput, lookDir);
        }

        [ServerRpc]
        private void SubmitMovementServerRpc(Vector2 input, Vector2 lookDir)
        {
            model.MoveInput = input;
            BroadcastMovementClientRpc(input, lookDir);
        }

        [ClientRpc]
        private void BroadcastMovementClientRpc(Vector2 input, Vector2 lookDir)
        {
            if (IsOwner) return;
            view.Move(input, model.MoveSpeed, lookDir);
        }

        public PlayerView GetView()
        {
            return view;
        }
    }
}