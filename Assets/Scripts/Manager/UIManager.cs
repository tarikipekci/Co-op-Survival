using System.Collections;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Heart UI Elements")] [SerializeField]
        private Image[] heartImages;

        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite emptyHeart;

        private PlayerHealth playerHealth;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
        }

        private void Start()
        {
            StartCoroutine(SetupUI());
        }

        private IEnumerator SetupUI()
        {
            while (NetworkManager.Singleton.LocalClient.PlayerObject == null)
                yield return null;

            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            playerHealth = localPlayer.GetComponent<PlayerHealth>();

            if (playerHealth == null)
            {
                Debug.LogError("PlayerHealth component not found on local player!");
                yield break;
            }

            Debug.Log("Subscribing to OnHealthChanged");
            playerHealth.OnHealthChanged += UpdateHearts;

            UpdateHearts(playerHealth.CurrentHealth);
        }

        private void UpdateHearts(int currentHealth)
        {
            Debug.Log($"UpdateHearts called with currentHealth={currentHealth}");
            for (int i = 0; i < heartImages.Length; i++)
            {
                heartImages[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
            }
        }

        public void RegisterPlayerHealth(PlayerHealth health)
        {
            if (playerHealth != null)
                playerHealth.OnHealthChanged -= UpdateHearts;

            playerHealth = health;
            playerHealth.OnHealthChanged += UpdateHearts;
            UpdateHearts(playerHealth.CurrentHealth);
        }


        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.OnHealthChanged -= UpdateHearts;
        }

        public void UnregisterPlayerHealth(PlayerHealth health)
        {
            if (playerHealth == health)
            {
                playerHealth.OnHealthChanged -= UpdateHearts;
                playerHealth = null;
                // optionally clear UI
            }
        }
    }
}