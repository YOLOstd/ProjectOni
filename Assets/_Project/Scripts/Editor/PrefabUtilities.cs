using UnityEditor;
using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Editor
{
    [InitializeOnLoad]
    public static class PrefabUtilities
    {
        static PrefabUtilities()
        {
            EditorApplication.delayCall += CheckAndAttachPlayerCombat;
        }

        [MenuItem("Tools/Oni/Attach Player Combat Component")]
        public static void CheckAndAttachPlayerCombat()
        {
            string prefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"PrefabUtilities: Player prefab not found at {prefabPath}");
                return;
            }

            // Check if PlayerCombat component is already on the root
            PlayerCombat combat = prefab.GetComponent<PlayerCombat>();
            if (combat == null)
            {
                // We must open, modify, and save the prefab contents
                GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    if (instance.GetComponent<PlayerCombat>() == null)
                    {
                        instance.AddComponent<PlayerCombat>();
                        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                        Debug.Log("PrefabUtilities: Successfully attached PlayerCombat to Player prefab root!");
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(instance);
                }
            }
            else
            {
                Debug.Log("PrefabUtilities: PlayerCombat is already attached to Player prefab.");
            }
        }
    }
}
