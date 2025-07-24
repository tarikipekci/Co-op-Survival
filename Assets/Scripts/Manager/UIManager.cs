using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Heart UI Elements")] 
        private List<Image> heartImages = new List<Image>();

        [SerializeField] private Transform heartContainer;
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
            int maxHealth = playerHealth.GetMaxHealth();
            Debug.Log(maxHealth);

            while (heartImages.Count < maxHealth)
            {
                GameObject heartGO =
                    new GameObject("Heart", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                heartGO.transform.SetParent(heartContainer, false);

                Image heartImage = heartGO.GetComponent<Image>();
                RectTransform rt = heartGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(50, 50);
                heartImage.preserveAspect = true;
                heartImages.Add(heartImage);
            }

            for (int i = 0; i < heartImages.Count; i++)
            {
                heartImages[i].gameObject.SetActive(i < maxHealth);
                if (i < maxHealth)
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
        
        public UpgradeUIManager GetUpgradeUIManager()
        {
            return upgradeUIManager;
        }
    }
}