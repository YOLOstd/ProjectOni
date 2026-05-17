using UnityEngine;
using ProjectOni.Core;
using ProjectOni.Player;
using ProjectOni.Data;

namespace ProjectOni.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private SkillBarUI skillBar;
        [SerializeField] private HealthUI healthUI;
        [SerializeField] private CharacterStatMenuUI charStatMenu;

        public static HUDManager Instance { get; private set; }

        private EquipmentManager _localEquipmentManager;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.OnWeaponSwapped += OnWeaponSwapped;
        }

        private void OnDisable()
        {
            GameEvents.OnWeaponSwapped -= OnWeaponSwapped;
        }

        public void BindLocalPlayer(HealthComponent health, StatController stats, EquipmentManager equipmentManager)
        {
            Debug.Log($"[HUDManager] BindLocalPlayer. healthUI is null? {healthUI == null}, charStatMenu is null? {charStatMenu == null}");
            if (healthUI != null) healthUI.Bind(health);
            if (charStatMenu != null) charStatMenu.Bind(stats);

            _localEquipmentManager = equipmentManager;
            UpdateWeaponSkills();
        }

        private void OnWeaponSwapped(EquipmentManager manager, EquipmentInstance weapon)
        {
            if (manager != null && manager == _localEquipmentManager)
            {
                UpdateWeaponSkills();
            }
        }

        public void UpdateWeaponSkills()
        {
            if (_localEquipmentManager == null) return;

            var activeWeapon = _localEquipmentManager.GetActiveWeapon();
            if (activeWeapon.IsValid)
            {
                var weaponTrait = activeWeapon.GetTrait<WeaponTrait>();
                if (weaponTrait != null && weaponTrait.attackData != null)
                {
                    UpdateSkillSlot(0, weaponTrait.attackData.skillIcon, weaponTrait.attackData.attackName);
                }
                else
                {
                    UpdateSkillSlot(0, null, "LMB");
                }
            }
            else
            {
                UpdateSkillSlot(0, null, "LMB");
            }
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
