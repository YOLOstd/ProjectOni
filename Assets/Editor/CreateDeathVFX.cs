using UnityEditor;
using UnityEngine;
using ProjectOni.Core;
using ProjectOni.Enemies;

public class CreateDeathVFX
{
    [MenuItem("Tools/Project Oni/Create Death VFX")]
    public static void Execute()
    {
        Debug.Log("[CreateDeathVFX] Starting VFX asset generation...");

        // 1. Setup paths
        string matPath = "Assets/_Project/Art/VFX/Materials/VFX_EnemyDeathExplosion.mat";
        string prefabPath = "Assets/_Project/Prefabs/VFX/VFX_EnemyDeathExplosion.prefab";
        string ghoulPath = "Assets/_Project/Prefabs/Enemies/Ghoul.prefab";

        // Ensure directories exist
        System.IO.Directory.CreateDirectory("Assets/_Project/Art/VFX/Materials");
        System.IO.Directory.CreateDirectory("Assets/_Project/Prefabs/VFX");

        // 2. Find shader and create material
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            Debug.LogWarning("[CreateDeathVFX] URP Particle Unlit shader not found, falling back to Sprites/Default");
            shader = Shader.Find("Sprites/Default");
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, matPath);
            Debug.Log($"[CreateDeathVFX] Created new material at: {matPath}");
        }
        else
        {
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
            Debug.Log($"[CreateDeathVFX] Re-used existing material at: {matPath}");
        }

        // 3. Create temporary GameObject for the particle system
        GameObject tempGO = new GameObject("VFX_EnemyDeathExplosion");
        
        // Add components
        ParticleSystem ps = tempGO.AddComponent<ParticleSystem>();
        PooledVFX pooled = tempGO.AddComponent<PooledVFX>();

        // 4. Configure Particle System Modules
        
        // Main module
        var main = ps.main;
        main.duration = 1.0f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 8.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.gravityModifier = new ParticleSystem.MinMaxCurve(0.3f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission module
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        var burst = new ParticleSystem.Burst(0f, 15, 25);
        emission.SetBursts(new ParticleSystem.Burst[] { burst });

        // Rotate the root object 90 degrees on the X-axis as requested to face the 2D camera viewport
        tempGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Shape module
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        shape.rotation = Vector3.zero; // Clean baseline circle orientation

        // Color over Lifetime module
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.37f, 0f), 0.0f),       // Orange #FF5E00
                new GradientColorKey(new Color(0.83f, 0f, 0f), 0.35f),      // Crimson #D30000
                new GradientColorKey(new Color(0.19f, 0.11f, 0.3f), 0.75f)  // Dark purple/ash #311B4D
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.6f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // Size over Lifetime module
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 1.0f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

        // Renderer module
        var psr = tempGO.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            psr.sharedMaterial = mat;
        }

        // 5. Save as Prefab
        GameObject newVfxPrefab = PrefabUtility.SaveAsPrefabAsset(tempGO, prefabPath);
        Object.DestroyImmediate(tempGO);
        Debug.Log($"[CreateDeathVFX] Successfully saved particle prefab to: {prefabPath}");

        // 6. Wire prefab to Ghoul & Enable PurrNet Pooling
        GameObject ghoulPrefab = PrefabUtility.LoadPrefabContents(ghoulPath);
        if (ghoulPrefab != null)
        {
            // Wire VFX prefab
            EnemyDeathEffect deathEffect = ghoulPrefab.GetComponentInChildren<EnemyDeathEffect>();
            if (deathEffect != null)
            {
                var field = typeof(EnemyDeathEffect).GetField("_deathParticlePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(deathEffect, newVfxPrefab);
                    EditorUtility.SetDirty(deathEffect);
                    Debug.Log($"[CreateDeathVFX] Successfully assigned new VFX prefab to Ghoul in {ghoulPath}");
                }
                else
                {
                    Debug.LogError("[CreateDeathVFX] Could not find private field _deathParticlePrefab on EnemyDeathEffect");
                }
            }
            else
            {
                Debug.LogError("[CreateDeathVFX] EnemyDeathEffect component not found on Ghoul prefab");
            }

            // Find all components and enable _shouldBePooled to force PurrNet pooling
            var allMonoBehaviours = ghoulPrefab.GetComponentsInChildren<MonoBehaviour>(true);
            int poolConfigCount = 0;
            foreach (var mb in allMonoBehaviours)
            {
                if (mb == null) continue;
                System.Type type = mb.GetType();
                System.Reflection.FieldInfo poolField = null;
                while (type != null && poolField == null)
                {
                    poolField = type.GetField("_shouldBePooled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    type = type.BaseType;
                }

                if (poolField != null)
                {
                    poolField.SetValue(mb, true);
                    EditorUtility.SetDirty(mb);
                    poolConfigCount++;
                }
            }
            Debug.Log($"[CreateDeathVFX] Programmatically enabled PurrNet pooling (_shouldBePooled = true) on {poolConfigCount} components in Ghoul prefab.");

            PrefabUtility.SaveAsPrefabAsset(ghoulPrefab, ghoulPath);
            PrefabUtility.UnloadPrefabContents(ghoulPrefab);
        }
        else
        {
            Debug.LogError($"[CreateDeathVFX] Could not load Ghoul prefab at: {ghoulPath}");
        }

        // 7. Auto-configure NetworkPrefabs.asset to enable pooling for Ghoul in the global asset
        string netPrefabsPath = "Assets/_Project/Prefabs/NetworkPrefabs.asset";
        if (System.IO.File.Exists(netPrefabsPath))
        {
            string content = System.IO.File.ReadAllText(netPrefabsPath);
            string targetSection = "guid: e82c687a003642b45b7df5d2fd8edbcb\n    prefab: {fileID: 3988700952566846874, guid: e82c687a003642b45b7df5d2fd8edbcb, type: 3}\n    pooled: 0";
            string replacementSection = "guid: e82c687a003642b45b7df5d2fd8edbcb\n    prefab: {fileID: 3988700952566846874, guid: e82c687a003642b45b7df5d2fd8edbcb, type: 3}\n    pooled: 1";
            
            // Handle both Windows (\r\n) and Unix (\n) line endings
            content = content.Replace(targetSection.Replace("\n", "\r\n"), replacementSection.Replace("\n", "\r\n"));
            content = content.Replace(targetSection, replacementSection);
            
            System.IO.File.WriteAllText(netPrefabsPath, content);
            Debug.Log("[CreateDeathVFX] Programmatically enabled pooling in NetworkPrefabs.asset for Ghoul prefab.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateDeathVFX] VFX generation completed successfully!");
    }
}
