using UnityEngine;
using TMPro;
using ProjectOni.Player;

namespace ProjectOni.UI
{
    public class DodgeDebugUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _debugText;
        [SerializeField] private DodgeController _dodgeController;

        [Header("Colors")]
        [SerializeField] private Color _dodgingColor = new Color(0.2f, 1f, 0.2f); // Vibrant Neon Green
        [SerializeField] private Color _notDodgingColor = new Color(1f, 0.2f, 0.2f); // Vibrant Neon Red

        private void Start()
        {
            if (_debugText == null)
            {
                _debugText = GetComponent<TextMeshProUGUI>();
            }

            FindDodgeController();
        }

        private void Update()
        {
            if (_dodgeController == null)
            {
                FindDodgeController();
                if (_dodgeController == null)
                {
                    if (_debugText != null)
                    {
                        _debugText.text = "isDodging: no player found";
                        _debugText.color = Color.yellow;
                    }
                    return;
                }
            }

            if (_debugText != null)
            {
                bool isDodging = _dodgeController.IsDodging;
                
                if (isDodging)
                {
                    _debugText.text = "isDodging: yes";
                    _debugText.color = _dodgingColor;
                }
                else
                {
                    _debugText.text = "isDodging: no";
                    _debugText.color = _notDodgingColor;
                }
            }
        }

        private void FindDodgeController()
        {
            _dodgeController = FindFirstObjectByType<DodgeController>();
        }
    }
}
