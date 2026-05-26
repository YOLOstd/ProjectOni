using UnityEngine;
using UnityEngine.UI;
using ProjectOni.Core;

namespace ProjectOni.UI
{
    /// <summary>
    /// A premium floating health bar for enemies that displays health status in World Space.
    /// Features:
    /// - Dual-slider "damage trail" (foreground snaps, background catches up smoothly).
    /// - Dynamic visibility (hidden at full health/death, fades in on damage).
    /// - Non-intrusive (attached to root, meaning it won't flip/mirror when the enemy turns).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class EnemyHealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The HealthComponent of the enemy. Automatically resolved in parent if left empty.")]
        [SerializeField] private HealthComponent _healthComponent;
        
        [Tooltip("The foreground slider that shows instant health changes.")]
        [SerializeField] private Slider _healthSlider;
        
        [Tooltip("The background slider that catches up slowly after taking damage.")]
        [SerializeField] private Slider _trailSlider;

        [Header("Aesthetics")]
        [Tooltip("Should the health bar be hidden when the enemy is at full health?")]
        [SerializeField] private bool _hideAtFullHealth = true;
        
        [Tooltip("How long (in seconds) the health bar remains fully visible after taking damage.")]
        [SerializeField] private float _visibleDuration = 3f;
        
        [Tooltip("Speed of the fade in/out transitions.")]
        [SerializeField] private float _fadeSpeed = 5f;

        [Header("Damage Trail Tuning")]
        [Tooltip("Delay (in seconds) before the damage trail starts to catch up.")]
        [SerializeField] private float _trailDelay = 0.5f;
        
        [Tooltip("Speed at which the damage trail catches up to current health.")]
        [SerializeField] private float _trailCatchupSpeed = 2f;

        private CanvasGroup _canvasGroup;
        private float _targetAlpha = 0f;
        private float _visibilityTimer;
        private float _trailDelayTimer;
        private bool _isDead;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Auto-resolve HealthComponent if not set in Inspector
            if (_healthComponent == null)
            {
                _healthComponent = GetComponentInParent<HealthComponent>();
            }

            // Start completely transparent to prevent visual pop on spawn
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        private void Start()
        {
            if (_healthComponent != null)
            {
                // Set initial values
                float current = _healthComponent.Current;
                float max = _healthComponent.Max;
                
                UpdateSlidersInstant(current, max);

                // Handle initial visibility
                if (max > 0f)
                {
                    if (_hideAtFullHealth && current >= max)
                    {
                        _targetAlpha = 0f;
                        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
                    }
                    else if (current > 0f)
                    {
                        _targetAlpha = 1f;
                        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
                    }
                }
                else
                {
                    // If not yet initialized/synced, hide for now until synced
                    _targetAlpha = 0f;
                    if (_canvasGroup != null) _canvasGroup.alpha = 0f;
                }
            }
        }

        private void OnEnable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnHealthChanged += HandleHealthChanged;
                _healthComponent.OnDied += HandleDied;
                
                // Refresh state on activation - only mark dead if max health is actually set and current is 0
                float current = _healthComponent.Current;
                float max = _healthComponent.Max;
                _isDead = max > 0f && current <= 0f;
                UpdateSlidersInstant(current, max);
            }
        }

        private void OnDisable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnHealthChanged -= HandleHealthChanged;
                _healthComponent.OnDied -= HandleDied;
            }
        }

        private void Update()
        {
            if (_canvasGroup == null) return;

            // 1. Calculate Target Alpha
            if (_isDead)
            {
                _targetAlpha = 0f;
            }
            else
            {
                if (_visibilityTimer > 0f)
                {
                    _visibilityTimer -= Time.deltaTime;
                    _targetAlpha = 1f;
                }
                else
                {
                    // Hide if at full health or if visibility timer has expired
                    bool shouldHide = _hideAtFullHealth && (_healthComponent == null || _healthComponent.Current >= _healthComponent.Max);
                    _targetAlpha = shouldHide ? 0f : 1f;
                }
            }

            // 2. Smoothly Lerp Canvas Alpha
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, _fadeSpeed * Time.deltaTime);

            // 3. Smoothly Update Damage Trail Slider
            if (_trailSlider != null && _healthSlider != null)
            {
                if (_trailDelayTimer > 0f)
                {
                    _trailDelayTimer -= Time.deltaTime;
                }
                else if (_trailSlider.value > _healthSlider.value)
                {
                    _trailSlider.value = Mathf.MoveTowards(
                        _trailSlider.value, 
                        _healthSlider.value, 
                        _trailCatchupSpeed * Time.deltaTime
                    );
                }
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            Debug.Log($"[EnemyHealthBarUI] HandleHealthChanged received: current={current}, max={max} on '{gameObject.name}'");
            if (max <= 0f) return;

            // Clear death state if health becomes positive
            if (current > 0f)
            {
                _isDead = false;
            }

            float pct = Mathf.Clamp01(current / max);

            // If we took damage (current health decreased), activate visibility and start trail delay
            if (_healthSlider != null && pct < _healthSlider.value)
            {
                _visibilityTimer = _visibleDuration;
                _trailDelayTimer = _trailDelay;
            }
            else if (_healthSlider == null)
            {
                _visibilityTimer = _visibleDuration;
            }

            // Update foreground instantly
            if (_healthSlider != null)
            {
                _healthSlider.value = pct;
            }

            // If healed or sliders got out of sync, snap trail slider up
            if (_trailSlider != null && pct > _trailSlider.value)
            {
                _trailSlider.value = pct;
            }
        }

        private void HandleDied()
        {
            _isDead = true;
            _visibilityTimer = 0f;
            
            // Set sliders to 0 instantly
            if (_healthSlider != null) _healthSlider.value = 0f;
            if (_trailSlider != null) _trailSlider.value = 0f;
        }

        private void UpdateSlidersInstant(float current, float max)
        {
            if (max <= 0f) return;
            float pct = Mathf.Clamp01(current / max);

            if (_healthSlider != null) _healthSlider.value = pct;
            if (_trailSlider != null) _trailSlider.value = pct;
        }
    }
}
