using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button selectButton;

        private Upgrade upgrade;
        private System.Action onSelectedCallback;

        public void Initialize(Upgrade currentUpgrade, System.Action onSelected)
        {
            upgrade = currentUpgrade;
            onSelectedCallback = onSelected;

            titleText.text = upgrade.Name;
            descriptionText.text = upgrade.Description;

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnUpgradeSelected);
        }

        private void OnUpgradeSelected()
        {
            onSelectedCallback?.Invoke();
        }
    }
}