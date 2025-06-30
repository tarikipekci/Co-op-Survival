using CameraBehavior;
using UnityEngine;
using Unity.Netcode;

namespace Player
{
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerController : NetworkBehaviour
    {
        private PlayerModel model;
        private PlayerView view;

        private Camera _camera;

        public NetworkVariable<Vector2> LookDirection = new NetworkVariable<Vector2>(
            writePerm: NetworkVariableWritePermission.Owner);

        [SerializeField] private GameObject cameraPrefab;

        private void Awake()
        {
            model = new PlayerModel();
            view = GetComponent<PlayerView>();
        }

        private void Start()
        {
            if (IsOwner)
            {
                GameObject cam = Instantiate(cameraPrefab);
                if (cam.TryGetComponent(out CameraBehaviour cameraFollow))
                {
                    cameraFollow.Follow(transform);
                    _camera = cam.GetComponent<Camera>();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

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

        public PlayerView GetView()
        {
            return view;
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
    }
}