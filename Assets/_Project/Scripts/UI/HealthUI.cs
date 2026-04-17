using UnityEngine;
using UnityEngine.UI;
using ProjectOni.Core;

namespace ProjectOni.UI
{
    public class HealthUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged += UpdateHealthUI;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged -= UpdateHealthUI;
        }

        private void UpdateHealthUI(float current, float max)
        {
            if (healthSlider == null) return;

            float percentage = current / max;
            healthSlider.value = percentage;

            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, percentage);
            }
        }
    }
}
