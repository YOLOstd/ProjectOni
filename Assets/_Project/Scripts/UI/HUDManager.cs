using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private SkillBarUI skillBar;
        [SerializeField] private HealthUI healthUI;

        public static HUDManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void UpdateSkillSlot(int index, Sprite icon, string label)
        {
            if (skillBar != null)
            {
                skillBar.UpdateSlot(index, icon, label);
            }
        }
    }
}
