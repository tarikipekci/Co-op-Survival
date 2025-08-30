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

        private Vector2 moveInput;
        private Vector2 lookDir;
        private Vector2 lastSentLookDir;

        private float movementTimer;
        private const float movementSendInterval = 0.05f;

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
                StartCoroutine(WaitForPlayerData());
                return;
            }

            InitializePlayerData();
        }

        private IEnumerator WaitForPlayerData()
        {
            yield return new WaitUntil(() =>
                PlayerDataManager.Instance != null &&
                PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId) != null
            );

            InitializePlayerData();
        }

        private void InitializePlayerData()
        {
            playerData = PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId);
            model = new PlayerModel(playerData);

            var health = GetComponent<PlayerHealth>();
            if (health != null)
                health.InitializeHealth(model.MaxHealth);

            if (IsOwner)
            {
                GameManager.Instance.AssignCameraToPlayer(transform);
                _camera = GameManager.Instance.GetCamera();
                if (_camera == null)
                    Debug.LogError("Camera could not be assigned!");
            }
        }

        private void Update()
        {
            if (!IsOwner || playerData == null || _camera == null) return;

            moveInput.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            moveInput.Normalize();

            Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            lookDir.Set(mouseWorldPos.x - transform.position.x, mouseWorldPos.y - transform.position.y);
            lookDir.Normalize();

            if ((lastSentLookDir - lookDir).sqrMagnitude > 0.05f)
            {
                lastSentLookDir = lookDir;
                LookDirection.Value = lookDir;
            }

            view.Move(moveInput, model.MoveSpeed, lookDir);

            movementTimer += Time.deltaTime;
            if (movementTimer >= movementSendInterval)
            {
                movementTimer = 0;
                SubmitMovementServerRpc(moveInput, lookDir, model.MoveSpeed);
            }
        }

        [ServerRpc]
        private void SubmitMovementServerRpc(Vector2 input, Vector2 lookDirection, float moveSpeed)
        {
            if (model == null) return;
            BroadcastMovementClientRpc(input, lookDirection, moveSpeed);
        }

        [ClientRpc]
        private void BroadcastMovementClientRpc(Vector2 input, Vector2 lookDirection, float moveSpeed)
        {
            if (IsOwner) return;
            view.Move(input, moveSpeed, lookDirection);
        }

        public PlayerView GetView() => view;
    }
}
