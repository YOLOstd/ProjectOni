using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ProjectOni.Data;

namespace ProjectOni.Editor
{
    [CustomEditor(typeof(ModularEquipmentData))]
    public class ModularEquipmentEditor : UnityEditor.Editor
    {
        private List<Type> _traitTypes;
        private bool _showTraits = true;

        private void OnEnable()
        {
            // Find all types that implement IEquipmentTrait and are not abstract
            _traitTypes = Assembly.GetAssembly(typeof(IEquipmentTrait))
                .GetTypes()
                .Where(t => typeof(IEquipmentTrait).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ModularEquipmentData data = (ModularEquipmentData)target;

            // Draw default properties excluding the traits list
            DrawPropertiesExcluding(serializedObject, "m_Script", "traits");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Equipment Traits", EditorStyles.boldLabel);

            SerializedProperty traitsProp = serializedObject.FindProperty("traits");

            _showTraits = EditorGUILayout.BeginFoldoutHeaderGroup(_showTraits, $"Traits ({traitsProp.arraySize})");
            if (_showTraits)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < traitsProp.arraySize; i++)
                {
                    SerializedProperty trait = traitsProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginHorizontal();
                    // Display the name of the trait type
                    string label = trait.managedReferenceFullTypename.Split('.').Last();
                    EditorGUILayout.PropertyField(trait, new GUIContent(label), true);
                    
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        traitsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                // Dropdown to add new traits
                if (GUILayout.Button("Add Trait"))
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (var type in _traitTypes)
                    {
                        menu.AddItem(new GUIContent(type.Name), false, () => AddTrait(data, type));
                    }
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void AddTrait(ModularEquipmentData data, Type type)
        {
            // We use serializedObject to ensure undo support
            serializedObject.Update();
            SerializedProperty traitsProp = serializedObject.FindProperty("traits");
            
            int index = traitsProp.arraySize;
            traitsProp.InsertArrayElementAtIndex(index);
            SerializedProperty newElem = traitsProp.GetArrayElementAtIndex(index);
            
            // Set the managed reference to a new instance of the type
            newElem.managedReferenceValue = Activator.CreateInstance(type);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
