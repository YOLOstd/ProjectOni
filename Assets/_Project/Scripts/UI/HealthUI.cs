using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectOni.Core;

namespace ProjectOni.UI
{
    /// <summary>
    /// Displays health for the LOCAL player only.
    /// Subscribes directly to HealthComponent.OnHealthChanged instead of a global event,
    /// so multiple players on the same client never overwrite each other's HUD.
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image  fillImage;
        [SerializeField] private TMP_Text _hpText;

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
            Debug.Log($"[HealthUI] Binding to health component on {health?.gameObject.name}");
            if (_target != null)
                _target.OnHealthChanged -= UpdateHealthUI;

            _target = health;

            if (_target != null)
            {
                _target.OnHealthChanged += UpdateHealthUI;
                UpdateHealthUI(_target.Current, _target.Max);
            }
        }


        private void OnEnable()
        {
            GameEvents.OnStatsRecalculated += HandleStatsRecalculated;
            if (_target != null)
            {
                _target.OnHealthChanged -= UpdateHealthUI;
                _target.OnHealthChanged += UpdateHealthUI;
                UpdateHealthUI(_target.Current, _target.Max);
            }
        }

        private void OnDisable()
        {
            GameEvents.OnStatsRecalculated -= HandleStatsRecalculated;
            if (_target != null)
                _target.OnHealthChanged -= UpdateHealthUI;
        }


        private void HandleStatsRecalculated(StatController stats)
        {
            if (_target == null) return;
            // Force refresh when stats (and thus Max HP) might have changed
            UpdateHealthUI(_target.Current, _target.Max);
        }

        private void UpdateHealthUI(float current, float max)
        {
            Debug.Log($"[HealthUI] Updating UI: {current} / {max}");
            if (healthSlider == null || max <= 0f) return;


            float pct = current / max;
            healthSlider.value = pct;

            if (fillImage != null)
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, pct);

            if (_hpText != null)
                _hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }
}
