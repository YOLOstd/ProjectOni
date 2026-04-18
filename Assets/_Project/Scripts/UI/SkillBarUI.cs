using UnityEngine;

namespace ProjectOni.UI
{
    public class SkillBarUI : MonoBehaviour
    {
        [Header("Skill Slots")]
        [SerializeField] private SkillSlotUI weaponSlot1;
        [SerializeField] private SkillSlotUI weaponSlot2;
        [SerializeField] private SkillSlotUI utilitySlot1;
        [SerializeField] private SkillSlotUI utilitySlot2;

        [Header("Debug Placeholders")]
        [SerializeField] private Sprite placeholderWeaponIcon;
        [SerializeField] private Sprite placeholderUtilityIcon;

        private void Start()
        {
            // Initial placeholder setup
            // In a real scenario, these would be populated by an EquipmentManager or SkillSystem
            InitializePlaceholderUI();
        }

        private void InitializePlaceholderUI()
        {
            if (weaponSlot1 != null) weaponSlot1.SetSkill(null, "LMB");
            if (weaponSlot2 != null) weaponSlot2.SetSkill(null, "RMB");
            if (utilitySlot1 != null) utilitySlot1.SetSkill(null, "Q");
            if (utilitySlot2 != null) utilitySlot2.SetSkill(null, "E");
        }

        // Methods to be called by external systems to update slots
        public void UpdateSlot(int index, Sprite icon, string label)
        {
            switch (index)
            {
                case 0: weaponSlot1?.SetSkill(icon, label); break;
                case 1: weaponSlot2?.SetSkill(icon, label); break;
                case 2: utilitySlot1?.SetSkill(icon, label); break;
                case 3: utilitySlot2?.SetSkill(icon, label); break;
            }
        }
    }
}
