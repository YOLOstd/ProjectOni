using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using ProjectOni.Data;

namespace ProjectOni.Editor
{
    public class TraitCreatorWindow : EditorWindow
    {
        private string _traitName = "New Trait";
        private string _savePath = "Assets/_Project/Data/Items/Traits";
        private int _selectedTraitTypeIndex = 0;
        private Type[] _traitTypes;
        private string[] _traitTypeNames;

        [MenuItem("Project Oni/Trait Creator")]
        public static void ShowWindow()
        {
            GetWindow<TraitCreatorWindow>("Trait Creator");
        }

        private void OnEnable()
        {
            // Find all non-abstract types that inherit from EquipmentTraitSO
            _traitTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(EquipmentTraitSO).IsAssignableFrom(p) && !p.IsAbstract)
                .ToArray();

            _traitTypeNames = _traitTypes.Select(t => t.Name).ToArray();
        }

        private void OnGUI()
        {
            GUILayout.Label("Create New Equipment Trait", EditorStyles.boldLabel);

            _traitName = EditorGUILayout.TextField("Trait Name", _traitName);

            if (_traitTypeNames != null && _traitTypeNames.Length > 0)
            {
                _selectedTraitTypeIndex = EditorGUILayout.Popup("Trait Type", _selectedTraitTypeIndex, _traitTypeNames);
            }
            else
            {
                EditorGUILayout.HelpBox("No trait types found. Make sure they inherit from EquipmentTraitSO.", MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", _savePath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        _savePath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        Debug.LogError("Path must be within the Assets folder.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Trait"))
            {
                CreateTrait();
            }
        }

        private void CreateTrait()
        {
            if (_traitTypes == null || _traitTypes.Length == 0) return;

            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
                AssetDatabase.Refresh();
            }

            Type selectedType = _traitTypes[_selectedTraitTypeIndex];
            EquipmentTraitSO asset = CreateInstance(selectedType) as EquipmentTraitSO;
            asset.traitName = _traitName;

            string fullPath = Path.Combine(_savePath, _traitName + ".asset");
            fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            
            Debug.Log($"Created new {selectedType.Name} at {fullPath}");
        }
    }
}
