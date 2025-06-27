using Unity.Netcode;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerController : NetworkBehaviour
    {
        private PlayerModel model;
        private PlayerView view;

        private void Awake()
        {
            model = new PlayerModel();
            view = GetComponent<PlayerView>();
        }

        private void Update()
        {
            if (!IsOwner) return;

            model.MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            view.Move(model.MoveInput, model.MoveSpeed);

            SubmitMovementServerRpc(model.MoveInput);
            //Debug.Log(model.MoveInput);
        }

        [ServerRpc]
        private void SubmitMovementServerRpc(Vector2 input)
        {
            model.MoveInput = input;
            BroadcastAnimationClientRpc(input);
        }

        [ClientRpc]
        private void BroadcastAnimationClientRpc(Vector2 input)
        {
            view.Move(input, model.MoveSpeed);
        }
    }
}