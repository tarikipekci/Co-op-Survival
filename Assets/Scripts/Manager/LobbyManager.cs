using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class LobbyManager : MonoBehaviour
    {
        [Header("UI")] [SerializeField] private TMP_InputField createLobbyNameInput;
        [SerializeField] private TMP_InputField maxPlayersInput;
        [SerializeField] private TMP_InputField joinCodeInput;
        [SerializeField] private TMP_Text createdLobbyCodeText;

        private Lobby myLobby;
        private float heartbeatTimer;
        private const float heartbeatInterval = 15f;

        private async void Awake()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }

        public void OnCreateLobbyButton()
        {
            string lobbyName = string.IsNullOrEmpty(createLobbyNameInput.text)
                ? "New Lobby"
                : createLobbyNameInput.text;
            int maxPlayers = int.TryParse(maxPlayersInput.text, out int result) ? result : 4;

            _ = CreateLobby(lobbyName, maxPlayers);
        }

        public void OnJoinLobbyButton()
        {
            string code = joinCodeInput.text;
            if (!string.IsNullOrEmpty(code))
            {
                _ = JoinLobbyByCode(code);
            }
        }

        private async Task CreateLobby(string lobbyName, int maxPlayers)
        {
            var options = new CreateLobbyOptions
            {
                IsPrivate = true
            };

            myLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log($"Lobby created: {myLobby.Name} ({myLobby.LobbyCode})");

            createdLobbyCodeText.text = $"Lobby Code: {myLobby.LobbyCode}";
            NetworkManager.Singleton.StartHost();
        }

        private async Task JoinLobbyByCode(string code)
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
            Debug.Log($"Joined lobby: {joinedLobby.Name} ({joinedLobby.LobbyCode})");
            NetworkManager.Singleton.StartClient();
        }

        private void Update()
        {
            if (myLobby != null)
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer <= 0f)
                {
                    heartbeatTimer = heartbeatInterval;
                    _ = LobbyService.Instance.SendHeartbeatPingAsync(myLobby.Id);
                }
            }
        }

        public void StartGame()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            NetworkManager.Singleton.SceneManager.LoadScene("Level", LoadSceneMode.Single);
        }
    }
}