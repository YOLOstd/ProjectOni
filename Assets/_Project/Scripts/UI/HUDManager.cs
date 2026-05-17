using UnityEngine;
using ProjectOni.Core;

namespace ProjectOni.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private SkillBarUI skillBar;
        [SerializeField] private HealthUI healthUI;
        [SerializeField] private CharacterStatMenuUI charStatMenu;

        public static HUDManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void BindLocalPlayer(HealthComponent health, StatController stats)
        {
            if (healthUI != null) healthUI.Bind(health);
            if (charStatMenu != null) charStatMenu.Bind(stats);
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
