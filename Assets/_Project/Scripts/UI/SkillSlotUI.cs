using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectOni.UI
{
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image frameImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI keybindText;

        [Header("Settings")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color equippedColor = Color.white;

        private void Awake()
        {
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;
            
            UpdateEmptyState();
        }

        public void SetSkill(Sprite icon, string keybindLabel)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = (icon != null);
                iconImage.color = equippedColor;
            }

            if (keybindText != null)
            {
                keybindText.text = keybindLabel;
            }
        }

        public void UpdateCooldown(float progress)
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = progress;
            }
        }

        public void UpdateEmptyState()
        {
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }
        }
    }
}
