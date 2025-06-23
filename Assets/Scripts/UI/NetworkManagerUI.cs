using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NetworkManagerUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;

        private void Awake()
        {
            hostButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
                HideUI();
            });

            clientButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
                HideUI();
            });
        }

        private void HideUI()
        {
            gameObject.SetActive(false);
        }
    }
}
