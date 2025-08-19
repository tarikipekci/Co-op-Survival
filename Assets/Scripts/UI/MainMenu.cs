using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject multiplayerPanel;
        [SerializeField] private GameObject hostPanel;
        [SerializeField] private GameObject clientPanel;

        public void OpenMultiplayerPanel()
        {
            mainMenuPanel.SetActive(false);
            multiplayerPanel.SetActive(true);
        }

        public void OpenHostPanel()
        {
            multiplayerPanel.SetActive(false);
            hostPanel.SetActive(true);
        }

        public void OpenClientPanel()
        {
            multiplayerPanel.SetActive(false);
            clientPanel.SetActive(true);
        }

        public void StartGameOnSinglePlayer()
        {
            NetworkManager.Singleton.StartHost();
            if (!NetworkManager.Singleton.IsHost) return;
            NetworkManager.Singleton.SceneManager.LoadScene("Level", LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}