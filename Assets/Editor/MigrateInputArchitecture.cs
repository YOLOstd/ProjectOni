using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using ProjectOni.Managers;
using ProjectOni.Player;

public class MigrateInputArchitecture
{
    [MenuItem("Project Oni/Setup/Migrate Input Architecture")]
    public static void Run()
    {
        string inputActionPath = "Assets/_Project/Settings/Inputs/PlayerInputActions.inputactions";
        InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputActionPath);

        if (actions == null)
        {
            Debug.LogError($"Could not find InputActionAsset at {inputActionPath}");
            return;
        }

        // 1. Setup InputManager in Scene
        GameObject managerObj = GameObject.Find("InputManager");
        if (managerObj == null)
        {
            managerObj = new GameObject("InputManager");
        }

        var playerInput = managerObj.GetComponent<PlayerInput>();
        if (playerInput == null) playerInput = managerObj.AddComponent<PlayerInput>();
        playerInput.actions = actions;

        if (managerObj.GetComponent<InputManager>() == null)
        {
            managerObj.AddComponent<InputManager>();
        }

        Debug.Log("InputManager setup in scene.");

        // 2. Update Player Prefab
        string playerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        GameObject prefabContents = PrefabUtility.LoadPrefabContents(playerPrefabPath);

        try
        {
            bool changed = false;
            Component reader = null;
            Component pInput = null;

            foreach (var comp in prefabContents.GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;
                string typeName = comp.GetType().Name;
                
                if (typeName == "InputReader")
                    reader = comp;
                else if (typeName == "PlayerInput")
                    pInput = comp;
            }

            if (reader != null)
            {
                Object.DestroyImmediate(reader, true);
                changed = true;
                Debug.Log("Removed InputReader from Player prefab.");
            }

            if (pInput != null)
            {
                Object.DestroyImmediate(pInput, true);
                changed = true;
                Debug.Log("Removed PlayerInput from Player prefab.");
            }

            // 3. Enable Owner Authority on StateMachine
            var stateMachine = prefabContents.GetComponentInChildren<PurrNet.StateMachine.StateMachine>();
            if (stateMachine != null)
            {
                SerializedObject so = new SerializedObject(stateMachine);
                var prop = so.FindProperty("_ownerAuth");
                if (prop != null && !prop.boolValue)
                {
                    prop.boolValue = true;
                    so.ApplyModifiedProperties();
                    changed = true;
                    Debug.Log("Enabled Owner Authority on Player StateMachine.");
                }
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabContents, playerPrefabPath);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Input Architecture Migration Complete!");
    }
}
