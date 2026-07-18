using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace fShader.Editor
{
    public static class fShaderP6SampleTools
    {
        private const string AuthoringRoot = "Assets/fShaderDevelopment/P6SampleAuthoring";
        private const string CoreAuthoring = AuthoringRoot + "/Core";
        private const string PlusAuthoring = AuthoringRoot + "/Plus";
        private const string CoreSample = "Packages/com.fshader.core/Samples~/Gallery";
        private const string PlusSample = "Packages/com.fshader.plus/Samples~/Plus Gallery";
        private const string GenerationRequest = "Temp/fshader-p6-generate.request";

        [InitializeOnLoadMethod]
        private static void RunRequestedGeneration()
        {
            if (!File.Exists(GenerationRequest)) return;
            File.Delete(GenerationRequest);
            EditorApplication.delayCall += () =>
            {
                try
                {
                    GenerateReleaseSamplesInternal();
                    Debug.Log("fShader P6 requested release samples generated.");
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            };
        }
        [MenuItem("Tools/fShader/P6/Generate Release Samples")]
        public static void GenerateReleaseSamples()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            GenerateReleaseSamplesInternal();
            Debug.Log("fShader P6 release samples generated.");
        }

        public static void GenerateReleaseSamplesAndExit()
        {
            int exitCode = 0;
            try
            {
                GenerateReleaseSamplesInternal();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                exitCode = 1;
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static void GenerateReleaseSamplesInternal()
        {
            if (AssetDatabase.IsValidFolder(AuthoringRoot))
            {
                AssetDatabase.DeleteAsset(AuthoringRoot);
            }

            EnsureAssetFolder(CoreAuthoring + "/Textures");
            EnsureAssetFolder(CoreAuthoring + "/Materials");
            EnsureAssetFolder(CoreAuthoring + "/Scenes");
            EnsureAssetFolder(PlusAuthoring + "/Materials");
            EnsureAssetFolder(PlusAuthoring + "/Scenes");
            CopyCoreTexturesForAuthoring();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Material liteWater = CreateLiteWater();
            Material liteIce = CreateLiteIce();
            Material liteGlass = CreateLiteGlass();
            Material liteGround = CreateGround(CoreAuthoring + "/Materials/Gallery Ground.mat");
            CreateGalleryScene(CoreAuthoring + "/Scenes/fShader Lite Gallery.unity", "fShader Lite 1.0.0", liteWater, liteIce, liteGlass, liteGround);

            Material plusWater = CreatePlusMaterial("fShader/Plus/Water", PlusAuthoring + "/Materials/Plus Water Balanced.mat", new Color(0.12f, 0.58f, 0.72f, 0.72f));
            Material plusIce = CreatePlusMaterial("fShader/Plus/Ice", PlusAuthoring + "/Materials/Plus Ice Balanced.mat", new Color(0.62f, 0.9f, 1f, 1f));
            Material plusGlass = CreatePlusMaterial("fShader/Plus/Glass", PlusAuthoring + "/Materials/Plus Glass Balanced.mat", new Color(0.92f, 0.98f, 1f, 0.28f));
            Material plusGround = CreateGround(PlusAuthoring + "/Materials/Gallery Ground.mat");
            CreateGalleryScene(PlusAuthoring + "/Scenes/fShader Plus Gallery.unity", "fShader Plus 1.0.0", plusWater, plusIce, plusGlass, plusGround);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            CopyExportFolder(CoreAuthoring + "/Materials", CoreSample + "/Materials");
            CopyExportFolder(CoreAuthoring + "/Scenes", CoreSample + "/Scenes");
            CopyExportFolder(PlusAuthoring + "/Materials", PlusSample + "/Materials");
            CopyExportFolder(PlusAuthoring + "/Scenes", PlusSample + "/Scenes");

            AssetDatabase.DeleteAsset(AuthoringRoot);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static Material CreateLiteWater()
        {
            Material material = CreateMaterial("fShader/Lite/Water", CoreAuthoring + "/Materials/Lite Water Ocean.mat");
            SetTexture(material, "_BaseMap", "Water_Base_Calm.png");
            SetTexture(material, "_WaveNormalMap", "Water_WaveNormal_Fine.png");
            SetTexture(material, "_FoamMap", "Water_FoamMask.png");
            SetFloat(material, "_FSWaterWaveNormal", 1f);
            SetFloat(material, "_FSWaterFoam", 1f);
            SetFloat(material, "_FoamStrength", 0.55f);
            SetFloat(material, "_Roughness", 0.16f);
            fShaderP2Inspector.SyncKeywords(material);
            return material;
        }

        private static Material CreateLiteIce()
        {
            Material material = CreateMaterial("fShader/Lite/Ice", CoreAuthoring + "/Materials/Lite Ice Frosted.mat");
            SetTexture(material, "_BaseMap", "Ice_Base_Glacial.png");
            SetTexture(material, "_NormalMap", "Ice_Normal_Crystal.png");
            SetTexture(material, "_FrostMap", "Ice_FrostMask.png");
            SetTexture(material, "_CrackMap", "Ice_CrackMask.png");
            SetFloat(material, "_FSIceFrost", 1f);
            SetFloat(material, "_FSIceCracks", 1f);
            SetFloat(material, "_FSIceScatter", 1f);
            SetFloat(material, "_FrostStrength", 0.65f);
            SetFloat(material, "_Roughness", 0.42f);
            fShaderP2Inspector.SyncKeywords(material);
            return material;
        }

        private static Material CreateLiteGlass()
        {
            Material material = CreateMaterial("fShader/Lite/Glass", CoreAuthoring + "/Materials/Lite Glass Condensed.mat");
            SetTexture(material, "_CondensationMap", "Glass_Condensation_RGB.png");
            SetTexture(material, "_CondensationNormal", "Glass_CondensationNormal.png");
            SetFloat(material, "_FSGlassCondensation", 1f);
            SetFloat(material, "_FSGlassDropletNormal", 1f);
            SetFloat(material, "_CondensationAmount", 0.6f);
            SetFloat(material, "_Roughness", 0.32f);
            fShaderP2Inspector.SyncKeywords(material);
            return material;
        }

        private static Material CreatePlusMaterial(string shaderName, string path, Color color)
        {
            Material material = CreateMaterial(shaderName, path);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            SetFloat(material, "_FSScreenRefraction", 0f);
            SetFloat(material, "_LTCGI", 0f);
            SetFloat(material, "_FSBoxProjection", 1f);
            fShaderP3Inspector.SyncKeywords(material);
            fShaderP4Inspector.SyncKeywords(material);
            return material;
        }

        private static Material CreateGround(string path)
        {
            Material material = CreateMaterial("Standard", path);
            material.color = new Color(0.16f, 0.19f, 0.23f, 1f);
            material.SetFloat("_Glossiness", 0.15f);
            return material;
        }

        private static Material CreateMaterial(string shaderName, string path)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null) throw new InvalidOperationException("Shader not found: " + shaderName);
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);
            Material material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void SetTexture(Material material, string property, string fileName)
        {
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(CoreAuthoring + "/Textures/" + fileName);
            if (texture == null) throw new InvalidOperationException("Sample texture not found: " + fileName);
            if (material.HasProperty(property)) material.SetTexture(property, texture);
        }

        private static void SetFloat(Material material, string property, float value)
        {
            if (material.HasProperty(property)) material.SetFloat(property, value);
        }

        private static void CreateGalleryScene(string scenePath, string title, Material water, Material ice, Material glass, Material ground)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.52f, 0.58f, 0.68f, 1f);

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.14f, 0.22f, 1f);
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 200f;
            cameraObject.transform.position = new Vector3(0f, 3.2f, -10f);
            cameraObject.transform.LookAt(new Vector3(0f, 1f, 0f));

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Gallery Floor";
            floor.transform.localScale = new Vector3(1.25f, 1f, 0.75f);
            floor.GetComponent<Renderer>().sharedMaterial = ground;

            CreateDisplay("Water", PrimitiveType.Cube, new Vector3(-3f, 0.35f, 0f), new Vector3(2.5f, 0.18f, 2.5f), water);
            CreateDisplay("Ice", PrimitiveType.Sphere, new Vector3(0f, 1.15f, 0f), Vector3.one * 2.1f, ice);
            CreateDisplay("Glass", PrimitiveType.Cube, new Vector3(3f, 1.15f, 0f), new Vector3(2.2f, 2.2f, 0.18f), glass);
            CreateLabel(title, new Vector3(0f, 3.1f, 0.6f), 0.42f);
            CreateLabel("Water", new Vector3(-3f, 1.1f, -1.5f), 0.25f);
            CreateLabel("Ice", new Vector3(0f, 2.5f, -1f), 0.25f);
            CreateLabel("Glass", new Vector3(3f, 2.5f, -0.4f), 0.25f);

            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException("Could not save sample scene: " + scenePath);
            }
        }

        private static void CreateDisplay(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            GameObject display = GameObject.CreatePrimitive(type);
            display.name = name;
            display.transform.position = position;
            display.transform.localScale = scale;
            display.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateLabel(string text, Vector3 position, float size)
        {
            GameObject label = new GameObject(text + " Label");
            label.transform.position = position;
            label.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = size;
            mesh.fontSize = 64;
            mesh.color = Color.white;
        }

        private static void CopyCoreTexturesForAuthoring()
        {
            string source = Path.GetFullPath(CoreSample + "/Textures");
            string destination = Path.GetFullPath(CoreAuthoring + "/Textures");
            foreach (string file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            }
        }

        private static void CopyExportFolder(string sourceAssetFolder, string destinationAssetFolder)
        {
            string source = Path.GetFullPath(sourceAssetFolder);
            string destination = Path.GetFullPath(destinationAssetFolder);
            if (Directory.Exists(destination)) Directory.Delete(destination, true);
            CopyDirectory(source, destination);
            string sourceMeta = Path.GetFullPath(sourceAssetFolder + ".meta");
            string destinationMeta = Path.GetFullPath(destinationAssetFolder + ".meta");
            if (File.Exists(destinationMeta)) File.Delete(destinationMeta);
            if (File.Exists(sourceMeta)) File.Copy(sourceMeta, destinationMeta, true);
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            }
            foreach (string directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
            }
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
