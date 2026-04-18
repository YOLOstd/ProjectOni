using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine;

namespace ProjectOni.Editor
{
    public static class InputActionSetup
    {
        [MenuItem("Tools/Project Oni/Finalize Input Setup")]
        public static void FinalizeSetup()
        {
            string path = "Assets/_Project/Settings/Inputs/PlayerInputActions.inputactions";
            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);

            if (asset == null)
            {
                Debug.LogError($"Could not find Input Action asset at {path}");
                return;
            }

            var playerMap = asset.FindActionMap("Player");
            if (playerMap == null)
            {
                Debug.LogError("Player Action Map not found in asset!");
                return;
            }

            // Add ToggleMenu action if it doesn't exist
            var toggleAction = playerMap.FindAction("ToggleMenu");
            if (toggleAction == null)
            {
                toggleAction = playerMap.AddAction("ToggleMenu", InputActionType.Button);
                Debug.Log("Added ToggleMenu action.");
            }

            // Add binding if it doesn't exist
            bool hasTabBinding = false;
            foreach (var binding in toggleAction.bindings)
            {
                if (binding.path == "<Keyboard>/tab")
                {
                    hasTabBinding = true;
                    break;
                }
            }

            if (!hasTabBinding)
            {
                toggleAction.AddBinding("<Keyboard>/tab")
                    .WithGroup("Keyboard&Mouse");
                Debug.Log("Added Tab binding to ToggleMenu.");
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Input Setup Finalized! You can now use the TAB key to toggle the menu.");
        }
    }
}
