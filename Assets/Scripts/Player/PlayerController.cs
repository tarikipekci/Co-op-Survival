using Manager;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

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
            if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId) == null)
            {
                Debug.Log($"[PlayerController] Player Data is not ready yet! Waiting... (ClientId: {OwnerClientId})");
                StartCoroutine(WaitForPlayerData());
                return;
            }

            playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
            SetPlayerData(playerData);
        }

        private IEnumerator WaitForPlayerData()
        {
            yield return new WaitUntil(() =>
                PlayerDataManager.Instance != null &&
                PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId) != null
            );

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

            SubmitMovementServerRpc(moveInput, lookDir, model.MoveSpeed);
        }

        [ServerRpc]
        private void SubmitMovementServerRpc(Vector2 input, Vector2 lookDir, float moveSpeed)
        {
            if (model == null)
            {
                Debug.LogWarning($"[SERVER] model is null on SubmitMovementServerRpc for client {OwnerClientId}");
                return;
            }

            BroadcastMovementClientRpc(input, lookDir, moveSpeed);
        }

        [ClientRpc]
        private void BroadcastMovementClientRpc(Vector2 input, Vector2 lookDir, float moveSpeed)
        {
            if (IsOwner || view == null)
                return;

            view.Move(input, moveSpeed, lookDir);
        }

        public PlayerView GetView() => view;

        private void SetPlayerData(PlayerData newPlayerData)
        {
            if (newPlayerData == null)
            {
                Debug.LogError($"PlayerData for client {OwnerClientId} not found!");
                return;
            }

            model = new PlayerModel(newPlayerData);

            var health = GetComponent<PlayerHealth>();
            if (health != null)
                health.InitializeHealth(model.MaxHealth
                );
            else
                Debug.LogError("PlayerHealth component not found!");

            if (IsOwner)
            {
                GameManager.Instance.AssignCameraToPlayer(transform);
                _camera = GameManager.Instance.GetCamera();

                if (_camera == null)
                    Debug.LogError("Camera could not be assigned!");
            }

            Debug.Log($"[PlayerController] Player Data is ready (ClientId: {OwnerClientId})");
        }
    }
}
