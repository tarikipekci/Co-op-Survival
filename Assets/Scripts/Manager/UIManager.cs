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

        private PlayerHealth playerHealth;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
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