using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Button selectButton;

        private Upgrade upgrade;
        private System.Action onSelectedCallback;

        public void Initialize(Upgrade currentUpgrade, System.Action onSelected)
        {
            upgrade = currentUpgrade;
            onSelectedCallback = onSelected;

            titleText.text = upgrade.Name;
            descriptionText.text = upgrade.Description;
            itemIcon.sprite = upgrade.Icon;
            itemIcon.SetNativeSize();

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnUpgradeSelected);
        }

        private void OnUpgradeSelected()
        {
            onSelectedCallback?.Invoke();
        }
    }
}