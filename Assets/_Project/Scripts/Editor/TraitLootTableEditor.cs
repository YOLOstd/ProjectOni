using UnityEditor;
using UnityEngine;
using ProjectOni.Data;

namespace ProjectOni.Editor
{
    [CustomEditor(typeof(TraitLootTable))]
    public class TraitLootTableEditor : UnityEditor.Editor
    {
        private bool _showTraits = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Possible Traits", EditorStyles.boldLabel);

            SerializedProperty possibleTraitsProp = serializedObject.FindProperty("possibleTraits");

            _showTraits = EditorGUILayout.BeginFoldoutHeaderGroup(_showTraits, $"Traits ({possibleTraitsProp.arraySize})");
            if (_showTraits)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < possibleTraitsProp.arraySize; i++)
                {
                    SerializedProperty entry = possibleTraitsProp.GetArrayElementAtIndex(i);
                    SerializedProperty trait = entry.FindPropertyRelative("trait");
                    SerializedProperty weight = entry.FindPropertyRelative("weight");
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    
                    string label = trait.objectReferenceValue != null ? trait.objectReferenceValue.name : "Empty Trait";
                    EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                    
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        possibleTraitsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(trait, new GUIContent("Trait Asset"));
                    EditorGUILayout.PropertyField(weight);
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add New Entry"))
                {
                    possibleTraitsProp.arraySize++;
                    SerializedProperty newEntry = possibleTraitsProp.GetArrayElementAtIndex(possibleTraitsProp.arraySize - 1);
                    newEntry.FindPropertyRelative("weight").intValue = 10;
                    newEntry.FindPropertyRelative("trait").objectReferenceValue = null;
                }

                if (GUILayout.Button("Open Trait Creator", GUILayout.Width(130)))
                {
                    TraitCreatorWindow.ShowWindow();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
