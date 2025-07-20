using Player;
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

        [Header("Upgrade UI")] [SerializeField]
        private UpgradeUIManager upgradeUIManager;

        private PlayerHealth playerHealth;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
        }

        private void UpdateHearts(int currentHealth)
        {
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

        public void UnregisterPlayerHealth(PlayerHealth health)
        {
            if (playerHealth == health)
            {
                playerHealth.OnHealthChanged -= UpdateHearts;
                playerHealth = null;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.OnHealthChanged -= UpdateHearts;
        }

        public void ShowUpgradeOptions(PlayerData playerData)
        {
            upgradeUIManager.ShowUpgradeOptions(playerData);
        }

        public UpgradeUIManager GetUpgradeUIManager()
        {
            return upgradeUIManager;
        }
    }
}