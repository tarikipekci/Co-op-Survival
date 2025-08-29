using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class LobbyManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField createLobbyNameInput;
        [SerializeField] private TMP_InputField maxPlayersInput;
        [SerializeField] private TMP_InputField joinCodeInput;
        [SerializeField] private TMP_Text createdLobbyCodeText;
        [SerializeField] private TMP_Text logText;

        private Lobby myLobby;
        private float heartbeatTimer;
        private const float heartbeatInterval = 15f;
        private bool isAuthenticated;

        private async void Start()
        {
            await InitializeServices();
        }

        private async Task InitializeServices()
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                isAuthenticated = true;
                Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
            }
            catch (System.Exception e)
            {
                LogError("Initialization failed: " + e);
            }
        }

        public void OnCreateLobbyButton()
        {
            if (!isAuthenticated)
            {
                LogError("Authentication not ready yet!");
                return;
            }

            string lobbyName = string.IsNullOrEmpty(createLobbyNameInput.text) ? "New Lobby" : createLobbyNameInput.text;
            int maxPlayers = int.TryParse(maxPlayersInput.text, out int result) ? result : 4;

            _ = CreateLobby(lobbyName, maxPlayers);
        }

        public void OnJoinLobbyButton()
        {
            if (!isAuthenticated)
            {
                LogError("Authentication not ready yet!");
                return;
            }

            string code = joinCodeInput.text;
            if (!string.IsNullOrEmpty(code))
                _ = JoinLobbyByCode(code);
        }

        private async Task CreateLobby(string lobbyName, int maxPlayers)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                    }
                };

                myLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                Log($"Lobby created: {myLobby.Name} ({myLobby.LobbyCode})");
                createdLobbyCodeText.text = $"Lobby Code: {myLobby.LobbyCode}";

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                NetworkManager.Singleton.StartHost();
                Log("Host started.");
            }
            catch (LobbyServiceException e)
            {
                LogError("Create Lobby failed: " + e);
            }
            catch (RelayServiceException e)
            {
                LogError("Relay Allocation failed: " + e);
            }
        }

        private async Task JoinLobbyByCode(string code)
        {
            try
            {
                Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
                Log($"Joined lobby: {joinedLobby.Name} ({joinedLobby.LobbyCode})");

                string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

                NetworkManager.Singleton.StartClient();
                Log("Client started and attempting to join host.");
            }
            catch (LobbyServiceException e)
            {
                LogError("Join Lobby failed: " + e);
            }
            catch (RelayServiceException e)
            {
                LogError("Relay Join failed: " + e);
            }
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

        private void Log(string message)
        {
            Debug.Log(message);
            if (logText != null) logText.text += message + "\n";
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
            if (logText != null) logText.text += "<color=red>" + message + "</color>\n";
        }
    }
}
