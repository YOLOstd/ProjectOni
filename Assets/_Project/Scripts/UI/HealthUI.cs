using UnityEngine;
using UnityEngine.UI;
using ProjectOni.Core;

namespace ProjectOni.UI
{
    /// <summary>
    /// Displays health for the LOCAL player only.
    /// Subscribes directly to HealthComponent.OnHealthChanged instead of a global event,
    /// so multiple players on the same client never overwrite each other's HUD.
    /// 
    /// Usage: Assign _healthTarget in the Inspector (wire it to the player prefab's
    ///        HealthComponent) or let it auto-find via the OnSpawned callback on the
    ///        player prefab calling HealthUI.Bind().
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image  fillImage;

        [Header("Colors")]
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor  = Color.red;

        private HealthComponent _target;

        /// <summary>
        /// Called by the player prefab's NetworkBehaviour.OnSpawned (isOwner branch)
        /// to wire this HUD to the correct HealthComponent instance.
        /// </summary>
        public void Bind(HealthComponent health)
        {
            if (_target != null)
                _target.OnHealthChanged -= UpdateHealthUI;

            _target = health;

            if (_target != null)
            {
                _target.OnHealthChanged += UpdateHealthUI;
                UpdateHealthUI(_target.Current, _target.Max);
            }
        }

        private void OnDestroy()
        {
            if (_target != null)
                _target.OnHealthChanged -= UpdateHealthUI;
        }

        private void UpdateHealthUI(float current, float max)
        {
            if (healthSlider == null || max <= 0f) return;

            float pct = current / max;
            healthSlider.value = pct;

            if (fillImage != null)
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, pct);
        }
    }
}
