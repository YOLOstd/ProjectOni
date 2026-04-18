#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectOni.UI;

public class SkillBarCreator : EditorWindow
{
    [MenuItem("Project Oni/UI/Create Skill Bar Prefab")]
    public static void CreateSkillBar()
    {
        // 1. Create Root
        GameObject root = new GameObject("SkillBar", typeof(RectTransform), typeof(SkillBarUI));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        
        // 2. Create Layout Group
        HorizontalLayoutGroup layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.LowerRight;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        // 3. Create 4 Slots
        SkillSlotUI[] slots = new SkillSlotUI[4];
        string[] labels = { "LMB", "RMB", "Q", "E" };
        string[] names = { "WeaponSlot1", "WeaponSlot2", "UtilitySlot1", "UtilitySlot2" };

        for (int i = 0; i < 4; i++)
        {
            GameObject slotObj = CreateSlot(names[i], labels[i]);
            slotObj.transform.SetParent(root.transform);
            slots[i] = slotObj.GetComponent<SkillSlotUI>();
        }

        // 4. Assign Slots to SkillBarUI
        SkillBarUI barUI = root.GetComponent<SkillBarUI>();
        var fields = typeof(SkillBarUI).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // Using Reflection is a bit messy here but we can just useSerializedObject for a cleaner editor script
        SerializedObject so = new SerializedObject(barUI);
        so.FindProperty("weaponSlot1").objectReferenceValue = slots[0];
        so.FindProperty("weaponSlot2").objectReferenceValue = slots[1];
        so.FindProperty("utilitySlot1").objectReferenceValue = slots[2];
        so.FindProperty("utilitySlot2").objectReferenceValue = slots[3];
        so.ApplyModifiedProperties();

        // 5. Position Root (Bottom Right)
        rootRect.anchorMin = new Vector2(1, 0);
        rootRect.anchorMax = new Vector2(1, 0);
        rootRect.pivot = new Vector2(1, 0);
        rootRect.anchoredPosition = new Vector2(-20, 20);
        rootRect.sizeDelta = new Vector2(400, 100);

        // 6. Save as Prefab
        string path = "Assets/_Project/Art/UI/Prefabs";
        if (!AssetDatabase.IsValidFolder(path))
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/_Project/Art/UI/Prefabs");
            AssetDatabase.Refresh();
        }
        
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Art/UI/Prefabs/SkillBar.prefab");
        GameObject.DestroyImmediate(root);
        
        Debug.Log("Skill Bar Prefab created at Assets/_Project/Art/UI/Prefabs/SkillBar.prefab");
    }

    private static GameObject CreateSlot(string name, string label)
    {
        GameObject slot = new GameObject(name, typeof(RectTransform), typeof(SkillSlotUI), typeof(Image));
        RectTransform slotRect = slot.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(80, 80);

        // Frame
        Image frame = slot.GetComponent<Image>();
        frame.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark frame

        // Icon
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(slot.transform);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-10, -10); // Padding
        Image iconImage = iconObj.GetComponent<Image>();
        iconImage.enabled = false;

        // Cooldown
        GameObject cdObj = new GameObject("Cooldown", typeof(RectTransform), typeof(Image));
        cdObj.transform.SetParent(slot.transform);
        RectTransform cdRect = cdObj.GetComponent<RectTransform>();
        cdRect.anchorMin = Vector2.zero;
        cdRect.anchorMax = Vector2.one;
        cdRect.sizeDelta = Vector2.zero;
        Image cdImage = cdObj.GetComponent<Image>();
        cdImage.color = new Color(0, 0, 0, 0.5f);
        cdImage.type = Image.Type.Filled;
        cdImage.fillMethod = Image.FillMethod.Radial360;
        cdImage.fillOrigin = (int)Image.Origin360.Top;
        cdImage.fillAmount = 0;

        // Keybind Text
        GameObject textObj = new GameObject("Keybind", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(slot.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0);
        textRect.anchorMax = new Vector2(0.5f, 0);
        textRect.pivot = new Vector2(0.5f, 0);
        textRect.anchoredPosition = new Vector2(0, 5);
        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;

        // Setup SkillSlotUI references
        SkillSlotUI slotUI = slot.GetComponent<SkillSlotUI>();
        SerializedObject so = new SerializedObject(slotUI);
        so.FindProperty("frameImage").objectReferenceValue = frame;
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("cooldownOverlay").objectReferenceValue = cdImage;
        so.FindProperty("keybindText").objectReferenceValue = text;
        so.ApplyModifiedProperties();

        return slot;
    }
}
#endif
