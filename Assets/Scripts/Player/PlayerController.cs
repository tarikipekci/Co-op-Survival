using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Manager;

namespace Player
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        private Rigidbody2D rb;
        private PlayerView view;
        private Camera _camera;
        private PlayerData playerData;
        private PlayerModel model;

        public NetworkVariable<Vector2> MoveInput = new(writePerm: NetworkVariableWritePermission.Owner);
        public NetworkVariable<Vector2> LookDir = new(writePerm: NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            view = GetComponent<PlayerView>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            if (IsOwner)
                _camera = Camera.main;

            if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.GetOrCreatePlayerData(OwnerClientId) == null)
            {
                StartCoroutine(WaitForPlayerData());
            }
            else
            {
                InitializePlayerData();
            }
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
            }
        }

        private void Update()
        {
            if (!IsOwner || model == null) return;

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            MoveInput.Value = input;

            Vector3 mouseWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            LookDir.Value = (mouseWorld - transform.position).normalized;

            view.UpdateLookDirection(LookDir.Value, MoveInput.Value);
        }

        private void FixedUpdate()
        {
            if (rb == null || model == null) return;

            // Owner client prediction
            if (IsOwner)
                rb.linearVelocity = MoveInput.Value * model.MoveSpeed;

            // Server authoritative
            if (IsServer)
                rb.linearVelocity = MoveInput.Value * model.MoveSpeed;
        }

        public PlayerView GetView() => view;
    }
}
