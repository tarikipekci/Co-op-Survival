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
                Debug.LogError("PlayerView component not found!");
        }

        private void Start()
        {
            playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
            SetPlayerData(playerData);
        }

        private void FixedUpdate()
        {
            if (!IsOwner || _camera == null || playerData == null)
                return;

            Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2 lookDir = (mouseWorldPos - transform.position).normalized;

            LookDirection.Value = lookDir;

            view.Move(moveInput, model.MoveSpeed, lookDir);

            SubmitMovementServerRpc(moveInput, lookDir);
        }

        [ServerRpc]
        private void SubmitMovementServerRpc(Vector2 input, Vector2 lookDir)
        {
            if (model == null)
            {
                Debug.LogWarning($"[SERVER] model is null on SubmitMovementServerRpc for client {OwnerClientId}");
                return;
            }

            BroadcastMovementClientRpc(input, lookDir);
        }

        [ClientRpc]
        private void BroadcastMovementClientRpc(Vector2 input, Vector2 lookDir)
        {
            if (IsOwner || view == null || model == null)
                return;

            view.Move(input, model.MoveSpeed, lookDir);
        }

        public PlayerView GetView() => view;

        public void SetPlayerData(PlayerData newPlayerData)
        {
            if (newPlayerData == null)
            {
                Debug.LogError($"PlayerData for client {OwnerClientId} not found!");
                return;
            }

            model = new PlayerModel(newPlayerData);

            var health = GetComponent<PlayerHealth>();
            if (health != null)
                health.InitializeHealth(model.MaxHealth);
            else
                Debug.LogError("PlayerHealth component not found!");

            if (IsOwner)
            {
                GameManager.Instance.AssignCameraToPlayer(transform);
                _camera = GameManager.Instance.GetCamera();

                if (_camera == null)
                    Debug.LogError("Camera could not be assigned!");
            }
        }
    }
}
