using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace fShader.Editor
{
    public static class fShaderP5QATools
    {
        public const string ScenePath = "Assets/fShaderDevelopment/P5/Scenes/fShaderP5QA.unity";
        public const string GoldenFolder = "Assets/fShaderDevelopment/P5/Golden";
        public const string BenchmarkFolder = "Assets/fShaderDevelopment/P5/Benchmark";
        private const string MaterialFolder = "Assets/fShaderDevelopment/P5/Materials";
        private const string TextureFolder = "Assets/fShaderDevelopment/P5/Textures";
        private const string DefaultWorldScene = "Assets/Scenes/VRCDefaultWorldScene.unity";
        private const string GeneratedRootName = "fShader P5 QA (Generated)";
        private const string CaptureCameraName = "fShader P5 Golden Camera";

        [MenuItem("Tools/fShader/P5/Create or Refresh QA Scene")]
        public static void CreateOrRefreshScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (CreateOrRefreshSceneInternal())
            {
                Debug.Log("fShader P5 QA scene ready: " + ScenePath);
            }
        }

        [MenuItem("Tools/fShader/P5/Capture Golden Screenshots")]
        public static void CaptureGoldenScreenshots()
        {
            Scene scene = SceneManager.GetActiveScene();
            Camera camera = FindComponentInScene<Camera>(scene, CaptureCameraName);
            if (camera == null)
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "fShader P5",
                        "Create or open the P5 QA scene before capturing golden screenshots.",
                        "OK");
                }
                Debug.LogError("fShader P5 capture camera was not found.");
                return;
            }

            Directory.CreateDirectory(GoldenFolder);
            Directory.CreateDirectory(BenchmarkFolder);
            Vector3 originalPosition = camera.transform.position;
            Quaternion originalRotation = camera.transform.rotation;
            List<GameObject> disabledMirrors = DisableGameObjectsWithComponentType(scene, "MirrorReflection");
            Debug.Log("fShader P5 Golden capture disabled mirror objects: " + disabledMirrors.Count);
            List<Renderer> labelRenderers = null;

            try
            {
                Capture(camera, GoldenFolder + "/P5_Overview.png");
                labelRenderers = SetLabelRenderers(scene, false);
                camera.transform.position = new Vector3(0f, 6f, 10.8f);
                LookAt(camera.transform, new Vector3(0f, 1.25f, 15.5f));
                Capture(camera, GoldenFolder + "/P5_Refraction_LTCGI.png");

                foreach (Renderer renderer in labelRenderers) renderer.enabled = true;
                camera.transform.position = new Vector3(0f, 2.4f, 21.5f);
                LookAt(camera.transform, new Vector3(0f, 3.0f, 30.0f));
                Capture(camera, GoldenFolder + "/P5_Refraction_AB.png");
            }
            finally
            {
                if (labelRenderers != null)
                {
                    foreach (Renderer renderer in labelRenderers) renderer.enabled = true;
                }
                foreach (GameObject mirror in disabledMirrors) mirror.SetActive(true);
                camera.transform.position = originalPosition;
                camera.transform.rotation = originalRotation;
            }

            WriteEnvironmentSnapshot();
            AssetDatabase.Refresh();
            Debug.Log("fShader P5 golden screenshots captured in " + Path.GetFullPath(GoldenFolder));
        }
        // Entry point for an isolated batch-mode validation project.
        public static void CreateCaptureAndExit()
        {
            int exitCode = 0;
            try
            {
                if (!CreateOrRefreshSceneInternal())
                {
                    exitCode = 1;
                }
                else
                {
                    CaptureGoldenScreenshots();
                }
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

        private static bool CreateOrRefreshSceneInternal()
        {
            EnsureFolders();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DefaultWorldScene) != null)
                {
                    if (!AssetDatabase.CopyAsset(DefaultWorldScene, ScenePath))
                    {
                        Debug.LogError("Could not copy the default VRChat world scene to " + ScenePath);
                        return false;
                    }
                }
                else
                {
                    Scene created = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    TryAddComponent("VRC SDK World Descriptor", "VRC.SDK3.Components.VRCSceneDescriptor", "VRC.SDK3");
                    if (!EditorSceneManager.SaveScene(created, ScenePath))
                    {
                        return false;
                    }
                }
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject previousRoot = FindRoot(scene, GeneratedRootName);
            if (previousRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(previousRoot);
            }

            GameObject root = new GameObject(GeneratedRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            CreateEnvironment(root.transform);
            Camera camera = CreateCaptureCamera(root.transform);
            CreateMaterialGallery(root.transform);
            CreateRefractionDiagnosticStage(root.transform);
            CreateMeasurementMarkers(root.transform);
            CreateMirror(root.transform);
            CreateLTCGIStage(root.transform, scene);
            LookAt(camera.transform, new Vector3(0f, 1.7f, 11f));

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            return saved;
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/fShaderDevelopment/P5/Scenes");
            Directory.CreateDirectory(MaterialFolder);
            Directory.CreateDirectory(TextureFolder);
            Directory.CreateDirectory(GoldenFolder);
            Directory.CreateDirectory(BenchmarkFolder);
            AssetDatabase.Refresh();
        }

        private static void CreateEnvironment(Transform parent)
        {
            Material floorMaterial = GetEnvironmentMaterial();
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "QA Neutral Floor";
            floor.transform.SetParent(parent, false);
            floor.transform.position = new Vector3(0f, -0.2f, 16f);
            floor.transform.localScale = new Vector3(18f, 0.2f, 36f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;

            GameObject keyLight = new GameObject("QA Directional Light");
            keyLight.transform.SetParent(parent, false);
            keyLight.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            Light light = keyLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.78f, 0.88f, 1f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;

            GameObject fill = new GameObject("QA Fill Light");
            fill.transform.SetParent(parent, false);
            fill.transform.position = new Vector3(-6f, 4f, 8f);
            Light fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.color = new Color(0.3f, 0.58f, 1f);
            fillLight.intensity = 2.2f;
            fillLight.range = 13f;
        }

        private static Camera CreateCaptureCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject(CaptureCameraName);
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, 7.5f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 50f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.allowHDR = true;
            return camera;
        }

        private static void CreateMaterialGallery(Transform parent)
        {
            Material liteWater = GetOrCreateMaterial("Lite_Water_Balanced", "fShader/Lite/Water", new Color(0.12f, 0.62f, 0.78f, 0.78f), false, false, false);
            Material liteIce = GetOrCreateMaterial("Lite_Ice_Balanced", "fShader/Lite/Ice", new Color(0.65f, 0.88f, 1f, 1f), false, false, false);
            Material liteGlass = GetOrCreateMaterial("Lite_Glass_Balanced", "fShader/Lite/Glass", new Color(0.78f, 0.94f, 1f, 0.32f), false, false, false);
            Material plusWater = GetOrCreateMaterial("Plus_Water_Balanced", "fShader/Plus/Water", new Color(0.06f, 0.48f, 0.72f, 0.8f), false, false, false);
            Material plusIce = GetOrCreateMaterial("Plus_Ice_Balanced", "fShader/Plus/Ice", new Color(0.52f, 0.82f, 1f, 1f), false, false, false);
            Material plusGlass = GetOrCreateMaterial("Plus_Glass_Balanced", "fShader/Plus/Glass", new Color(0.7f, 0.9f, 1f, 0.28f), false, false, false);
            Material heavyWater = GetOrCreateMaterial("Plus_Water_Heavy", "fShader/Plus/Water", new Color(0.04f, 0.45f, 0.7f, 0.86f), true, true, true);
            Material heavyGlass = GetOrCreateMaterial("Plus_Glass_Heavy", "fShader/Plus/Glass", new Color(0.74f, 0.9f, 1f, 0.34f), true, true, true);
            Material ltcgiOff = GetOrCreateMaterial("Plus_Glass_LTCGI_OFF", "fShader/Plus/Glass", new Color(0.72f, 0.88f, 1f, 0.34f), false, false, true);
            Material ltcgiOn = GetOrCreateMaterial("Plus_Glass_LTCGI_ON", "fShader/Plus/Glass", new Color(0.72f, 0.88f, 1f, 0.34f), false, true, true);

            CreateDisplay(parent, "Lite Water / Balanced", PrimitiveType.Cube, new Vector3(-5f, 0.35f, 6f), new Vector3(3.5f, 0.12f, 3.5f), liteWater);
            CreateDisplay(parent, "Lite Ice / Balanced", PrimitiveType.Sphere, new Vector3(0f, 1.35f, 6f), new Vector3(2.4f, 2.4f, 2.4f), liteIce);
            CreateDisplay(parent, "Lite Glass / Balanced", PrimitiveType.Cube, new Vector3(5f, 1.4f, 6f), new Vector3(2.6f, 2.8f, 0.18f), liteGlass);

            CreateDisplay(parent, "Plus Water / Balanced", PrimitiveType.Cube, new Vector3(-5f, 0.35f, 10f), new Vector3(3.5f, 0.12f, 3.5f), plusWater);
            CreateDisplay(parent, "Plus Ice / Balanced", PrimitiveType.Sphere, new Vector3(0f, 1.35f, 10f), new Vector3(2.4f, 2.4f, 2.4f), plusIce);
            CreateDisplay(parent, "Plus Glass / Balanced", PrimitiveType.Cube, new Vector3(5f, 1.4f, 10f), new Vector3(2.6f, 2.8f, 0.18f), plusGlass);

            CreateDisplay(parent, "Plus Water / Heavy Screen + LTCGI", PrimitiveType.Cube, new Vector3(-4f, 0.35f, 14f), new Vector3(4.2f, 0.12f, 3.5f), heavyWater);
            CreateDisplay(parent, "Plus Glass / Heavy Screen + LTCGI", PrimitiveType.Cube, new Vector3(4f, 1.4f, 14f), new Vector3(3.2f, 2.8f, 0.18f), heavyGlass);

            CreateDisplay(parent, "LTCGI OFF", PrimitiveType.Sphere, new Vector3(-2.2f, 1.2f, 18f), new Vector3(2.1f, 2.1f, 2.1f), ltcgiOff);
            CreateDisplay(parent, "LTCGI ON", PrimitiveType.Sphere, new Vector3(2.2f, 1.2f, 18f), new Vector3(2.1f, 2.1f, 2.1f), ltcgiOn);
        }

        private static void CreateRefractionDiagnosticStage(Transform parent)
        {
            Texture2D diagnosticNormal = GetOrCreateDiagnosticNormal();
            Material waterOff = GetOrCreateMaterial("Plus_Water_Refraction_OFF_Test", "fShader/Plus/Water", new Color(0.08f, 0.55f, 0.8f, 0.42f), false, false, true);
            Material waterOn = GetOrCreateMaterial("Plus_Water_Refraction_ON_Test", "fShader/Plus/Water", new Color(0.08f, 0.55f, 0.8f, 0.42f), true, false, true);
            Material glassOff = GetOrCreateMaterial("Plus_Glass_Refraction_OFF_Test", "fShader/Plus/Glass", new Color(0.76f, 0.94f, 1f, 0.22f), false, false, true);
            Material glassOn = GetOrCreateMaterial("Plus_Glass_Refraction_ON_Test", "fShader/Plus/Glass", new Color(0.76f, 0.94f, 1f, 0.22f), true, false, true);

            ConfigureRefractionDiagnosticMaterial(waterOff, diagnosticNormal, false);
            ConfigureRefractionDiagnosticMaterial(waterOn, diagnosticNormal, true);
            ConfigureRefractionDiagnosticMaterial(glassOff, diagnosticNormal, false);
            ConfigureRefractionDiagnosticMaterial(glassOn, diagnosticNormal, true);

            CreateDiagnosticBackdrop(parent);
            CreateDisplay(parent, "WATER OFF", PrimitiveType.Cube, new Vector3(-5.25f, 2.15f, 29.2f), new Vector3(2.8f, 3.7f, 0.12f), waterOff);
            CreateDisplay(parent, "WATER ON", PrimitiveType.Cube, new Vector3(-1.75f, 2.15f, 29.2f), new Vector3(2.8f, 3.7f, 0.12f), waterOn);
            CreateDisplay(parent, "GLASS OFF", PrimitiveType.Cube, new Vector3(1.75f, 2.15f, 29.2f), new Vector3(2.8f, 3.7f, 0.12f), glassOff);
            CreateDisplay(parent, "GLASS ON", PrimitiveType.Cube, new Vector3(5.25f, 2.15f, 29.2f), new Vector3(2.8f, 3.7f, 0.12f), glassOn);
            CreateLabel(parent, "REFRACTION A/B  |  LEFT=OFF  RIGHT=ON  |  TEST STRENGTH 0.50", new Vector3(0f, 6.45f, 30.2f));
        }

        private static void ConfigureRefractionDiagnosticMaterial(Material material, Texture2D normal, bool screen)
        {
            SetFloat(material, "_RefractionStrength", 0.50f);
            SetFloat(material, "_FSScreenRefraction", screen ? 1f : 0f);
            if (material.HasProperty("_NormalMap")) material.SetTexture("_NormalMap", normal);
            if (material.HasProperty("_WaveNormalMap")) material.SetTexture("_WaveNormalMap", normal);
            if (material.HasProperty("_WaveNormalMap2")) material.SetTexture("_WaveNormalMap2", normal);
            SetFloat(material, "_FSWaterWaveNormal", 1f);
            SetKeyword(material, "FSHADER_NORMALMAP", material.HasProperty("_NormalMap"));
            fShaderP2Inspector.SyncKeywords(material);
            fShaderP3Inspector.SyncKeywords(material);
            fShaderP4Inspector.SyncKeywords(material);
            EditorUtility.SetDirty(material);
        }

        private static void CreateDiagnosticBackdrop(Transform parent)
        {
            Material black = GetDiagnosticMaterial("QA_Diagnostic_Black", new Color(0.012f, 0.012f, 0.016f, 1f));
            Material white = GetDiagnosticMaterial("QA_Diagnostic_White", Color.white);
            Material cyan = GetDiagnosticMaterial("QA_Diagnostic_Cyan", new Color(0f, 1f, 1f, 1f));
            Material magenta = GetDiagnosticMaterial("QA_Diagnostic_Magenta", new Color(1f, 0f, 0.8f, 1f));
            Material yellow = GetDiagnosticMaterial("QA_Diagnostic_Yellow", new Color(1f, 0.9f, 0f, 1f));

            CreateDiagnosticBox(parent, "Diagnostic Backdrop", new Vector3(0f, 2.4f, 31f), new Vector3(14.5f, 6.2f, 0.12f), black);
            for (int index = 0; index < 17; index++)
            {
                float x = -6.8f + index * 0.85f;
                CreateDiagnosticBox(parent, "Diagnostic Vertical " + index, new Vector3(x, 2.4f, 30.91f), new Vector3(0.18f, 5.9f, 0.04f), white);
            }
            CreateDiagnosticBox(parent, "Diagnostic Cyan Line", new Vector3(0f, 1.15f, 30.86f), new Vector3(14.1f, 0.16f, 0.035f), cyan);
            CreateDiagnosticBox(parent, "Diagnostic Yellow Line", new Vector3(0f, 2.4f, 30.85f), new Vector3(14.1f, 0.16f, 0.035f), yellow);
            CreateDiagnosticBox(parent, "Diagnostic Magenta Line", new Vector3(0f, 3.65f, 30.84f), new Vector3(14.1f, 0.16f, 0.035f), magenta);
        }

        private static void CreateMeasurementMarkers(Transform parent)
        {
            Material visual = GetDiagnosticMaterial("QA_Marker_Visual", new Color(1f, 0.82f, 0.05f, 1f));
            Material off = GetDiagnosticMaterial("QA_Marker_OFF", new Color(0.1f, 0.85f, 1f, 1f));
            Material on = GetDiagnosticMaterial("QA_Marker_ON", new Color(1f, 0.15f, 0.75f, 1f));
            Material mirror = GetDiagnosticMaterial("QA_Marker_Mirror", new Color(0.35f, 1f, 0.25f, 1f));

            CreateMeasurementPad(parent, "MEASURE VISUAL A/B", new Vector3(0f, 0.02f, 24.2f), visual);
            CreateMeasurementPad(parent, "MEASURE WATER OFF", new Vector3(-5.25f, 0.02f, 25.3f), off);
            CreateMeasurementPad(parent, "MEASURE WATER ON", new Vector3(-1.75f, 0.02f, 25.3f), on);
            CreateMeasurementPad(parent, "MEASURE GLASS OFF", new Vector3(1.75f, 0.02f, 25.3f), off);
            CreateMeasurementPad(parent, "MEASURE GLASS ON", new Vector3(5.25f, 0.02f, 25.3f), on);
            CreateMeasurementPad(parent, "MEASURE MIRROR", new Vector3(5.8f, 0.02f, 6.6f), mirror);
            CreateLabel(parent, "STAND ON MARKER - RSHIFT + ~ + 1 - PERFORMANCE - SAMPLING ON", new Vector3(0f, 1.0f, 24.9f));
            CreateLabel(parent, "FRAME ONE PANEL ONLY FOR 30 SECONDS", new Vector3(0f, 0.45f, 24.9f));
        }

        private static void CreateMeasurementPad(Transform parent, string name, Vector3 position, Material material)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = name;
            pad.transform.SetParent(parent, false);
            pad.transform.position = position;
            pad.transform.localScale = new Vector3(0.75f, 0.025f, 0.75f);
            pad.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(pad);
            CreateLabel(parent, name, position + new Vector3(0f, 0.22f, 0.45f));
        }

        private static void CreateDiagnosticBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(box);
        }

        private static void RemoveCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
        }

        private static Texture2D GetOrCreateDiagnosticNormal()
        {
            const int size = 64;
            string path = TextureFolder + "/QA_DiagnosticNormal.png";
            Texture2D source = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Sin((x + y * 0.35f) * Mathf.PI * 0.125f) * 0.42f;
                    float ny = Mathf.Cos((y - x * 0.2f) * Mathf.PI * 0.125f) * 0.42f;
                    float nz = Mathf.Sqrt(Mathf.Max(0.001f, 1f - nx * nx - ny * ny));
                    pixels[y * size + x] = new Color(nx * 0.5f + 0.5f, ny * 0.5f + 0.5f, nz * 0.5f + 0.5f, 1f);
                }
            }
            source.SetPixels(pixels);
            source.Apply(false, false);
            File.WriteAllBytes(path, source.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(source);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = size;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void CreateDisplay(Transform parent, string label, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material)
        {
            GameObject display = GameObject.CreatePrimitive(primitive);
            display.name = label;
            display.transform.SetParent(parent, false);
            display.transform.position = position;
            display.transform.localScale = scale;
            display.GetComponent<Renderer>().sharedMaterial = material;

            GameObject textObject = new GameObject(label + " Label");
            textObject.transform.SetParent(parent, false);
            textObject.transform.position = position + new Vector3(0f, scale.y * 0.55f + 1.15f, -0.35f);
            textObject.transform.rotation = Quaternion.identity;
            TextMesh text = textObject.AddComponent<TextMesh>();
            text.text = label;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 54;
            text.characterSize = 0.045f;
            text.color = Color.white;
        }

        private static void CreateMirror(Transform parent)
        {
            GameObject mirror = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mirror.name = "VRChat Mirror QA";
            mirror.transform.SetParent(parent, false);
            mirror.transform.position = new Vector3(8f, 2.3f, 10f);
            mirror.transform.rotation = Quaternion.Euler(0f, -35f, 0f);
            mirror.transform.localScale = new Vector3(4.5f, 3.2f, 1f);
            TryAddComponent(mirror, "VRC.SDK3.Components.VRCMirrorReflection", "VRC.SDK3");
            CreateLabel(parent, "Mirror: Fresnel / sparkle / condensation distance", new Vector3(7.4f, 4.4f, 9.2f));
        }

        private static void CreateLTCGIStage(Transform parent, Scene scene)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/at.pimaker.ltcgi/LTCGI Controller.prefab");
            GameObject controller = null;
            if (prefab != null)
            {
                controller = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
                if (controller != null)
                {
                    controller.name = "LTCGI Controller (P5 QA)";
                    controller.transform.SetParent(parent, true);
                }
            }

            Material emission = GetEmissionMaterial();
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = "LTCGI Screen (P5 QA)";
            screen.transform.SetParent(parent, false);
            screen.transform.position = new Vector3(0f, 4.5f, 21f);
            screen.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            screen.transform.localScale = new Vector3(6f, 2.5f, 1f);
            screen.GetComponent<Renderer>().sharedMaterial = emission;
            Component screenComponent = TryAddComponent(screen, "pi.LTCGI.LTCGI_Screen", "at.pimaker.ltcgi.Editor");
            SetSerializedColor(screenComponent, "Color", new Color(0.25f, 0.65f, 1f, 1f));

            GameObject emitter = GameObject.CreatePrimitive(PrimitiveType.Quad);
            emitter.name = "LTCGI Emitter (P5 QA)";
            emitter.transform.SetParent(parent, false);
            emitter.transform.position = new Vector3(-5.5f, 2.2f, 18f);
            emitter.transform.rotation = Quaternion.Euler(0f, 140f, 0f);
            emitter.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            emitter.GetComponent<Renderer>().sharedMaterial = emission;
            Component emitterComponent = TryAddComponent(emitter, "pi.LTCGI.LTCGI_Emitter", "at.pimaker.ltcgi.Editor");
            ConfigureEmitter(emitterComponent, emitter.GetComponent<Renderer>());

            if (controller != null)
            {
                Component[] components = controller.GetComponents<Component>();
                foreach (Component component in components)
                {
                    if (component != null && component.GetType().FullName == "pi.LTCGI.LTCGI_Controller")
                    {
                        System.Reflection.MethodInfo update = component.GetType().GetMethod("UpdateMaterials", Type.EmptyTypes);
                        if (update != null)
                        {
                            update.Invoke(component, null);
                        }
                        break;
                    }
                }
            }
        }

        private static Material GetOrCreateMaterial(string fileName, string shaderName, Color color, bool screen, bool ltcgi, bool showcase)
        {
            string path = MaterialFolder + "/" + fileName + ".mat";
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException("Missing shader: " + shaderName);
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            SetColor(material, "_BaseColor", color);
            SetFloat(material, "_Opacity", color.a);
            SetFloat(material, "_Roughness", shaderName.Contains("Ice") ? 0.28f : 0.09f);
            SetFloat(material, "_ReflectionStrength", 0.9f);
            SetFloat(material, "_FSBoxProjection", shaderName.Contains("Plus") ? 1f : 0f);
            SetFloat(material, "_FSScreenRefraction", screen ? 1f : 0f);
            SetFloat(material, "_LTCGI", ltcgi ? 1f : 0f);
            ConfigureTexturesAndMode(material, shaderName, showcase);

            material.enableInstancing = true;
            fShaderP2Inspector.SyncKeywords(material);
            fShaderP3Inspector.SyncKeywords(material);
            fShaderP4Inspector.SyncKeywords(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTexturesAndMode(Material material, string shaderName, bool showcase)
        {
            if (shaderName.Contains("Water"))
            {
                AssignTexture(material, "_BaseMap", "Water_Base_Calm");
                bool wave = AssignTexture(material, "_WaveNormalMap", "Water_WaveNormal_Fine");
                if (material.HasProperty("_WaveNormalMap2") && wave)
                {
                    material.SetTexture("_WaveNormalMap2", material.GetTexture("_WaveNormalMap"));
                }
                bool foam = AssignTexture(material, "_FoamMap", "Water_FoamMask");
                SetFloat(material, "_FSWaterWaveNormal", wave ? 1f : 0f);
                SetFloat(material, "_FSWaterFoam", foam && showcase ? 1f : 0f);
                SetFloat(material, "_FSWaterVertexWaves", showcase ? 1f : 0f);
                SetFloat(material, "_WaveCount", showcase ? 4f : 2f);
            }
            else if (shaderName.Contains("Ice"))
            {
                AssignTexture(material, "_BaseMap", "Ice_Base_Glacial");
                AssignTexture(material, "_NormalMap", "Ice_Normal_Crystal");
                bool frost = AssignTexture(material, "_FrostMap", "Ice_FrostMask");
                bool cracks = AssignTexture(material, "_CrackMap", "Ice_CrackMask");
                SetFloat(material, "_FSIceFrost", frost ? 1f : 0f);
                SetFloat(material, "_FSIceCracks", cracks ? 1f : 0f);
                SetFloat(material, "_FSIceScatter", 1f);
                SetFloat(material, "_FSIceBackLight", 1f);
                SetFloat(material, "_FSIceSparkle", showcase ? 1f : 0f);
            }
            else
            {
                bool condensation = AssignTexture(material, "_CondensationMap", "Glass_Condensation_RGB");
                bool condensationNormal = AssignTexture(material, "_CondensationNormal", "Glass_CondensationNormal");
                SetFloat(material, "_FSGlassCondensation", condensation ? 1f : 0f);
                SetFloat(material, "_FSGlassDropletNormal", condensation && condensationNormal ? 1f : 0f);
            }

            SetKeyword(material, "FSHADER_BASEMAP", HasAssetTexture(material, "_BaseMap"));
            SetKeyword(material, "FSHADER_NORMALMAP", HasAssetTexture(material, "_NormalMap"));
        }

        private static bool AssignTexture(Material material, string property, string assetName)
        {
            if (!material.HasProperty(property))
            {
                return false;
            }

            string[] guids = AssetDatabase.FindAssets(assetName + " t:Texture2D", new[] { "Assets" });
            if (guids.Length == 0)
            {
                material.SetTexture(property, null);
                return false;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
            material.SetTexture(property, texture);
            return texture != null;
        }

        private static bool HasAssetTexture(Material material, string property)
        {
            if (!material.HasProperty(property))
            {
                return false;
            }
            Texture texture = material.GetTexture(property);
            return texture != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture));
        }

        private static Material GetDiagnosticMaterial(string fileName, Color color)
        {
            string path = MaterialFolder + "/" + fileName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Unlit/Color");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.color = color;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetEnvironmentMaterial()
        {
            string path = MaterialFolder + "/QA_NeutralFloor.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.color = new Color(0.075f, 0.085f, 0.11f, 1f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Glossiness", 0.25f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetEmissionMaterial()
        {
            string path = MaterialFolder + "/QA_LTCGI_Emission.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            Color emission = new Color(0.12f, 0.65f, 1f, 1f) * 2.4f;
            material.color = new Color(0.08f, 0.35f, 0.7f, 1f);
            material.SetColor("_EmissionColor", emission);
            material.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Capture(Camera camera, string path)
        {
            const int width = 1600;
            const int height = 900;
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1
            };
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false, false);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                image.Apply(false, false);
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(image);
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void WriteEnvironmentSnapshot()
        {
            fShaderP5EnvironmentSnapshot snapshot = new fShaderP5EnvironmentSnapshot
            {
                generatedUtc = DateTime.UtcNow.ToString("O"),
                fShaderVersion = fShaderShaderCatalog.Version,
                unityVersion = Application.unityVersion,
                buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
                colorSpace = PlayerSettings.colorSpace.ToString(),
                stereoRenderingPathRaw = ReadStereoRenderingPath(),
                graphicsDevice = SystemInfo.graphicsDeviceName,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                graphicsMemoryMB = SystemInfo.graphicsMemorySize,
                processor = SystemInfo.processorType,
                qualityLevel = QualitySettings.names[QualitySettings.GetQualityLevel()],
                scene = SceneManager.GetActiveScene().path,
                overview = GoldenFolder + "/P5_Overview.png",
                refractionLtcgi = GoldenFolder + "/P5_Refraction_LTCGI.png",
                refractionAB = GoldenFolder + "/P5_Refraction_AB.png",
                note = "Editor golden images are deterministic desktop/photo-camera baselines. Validate SPSI, Multi Pass, Mirror and VRChat Photo Camera in Build & Test."
            };
            File.WriteAllText(BenchmarkFolder + "/P5_ENVIRONMENT.json", JsonUtility.ToJson(snapshot, true));
        }

        private static int ReadStereoRenderingPath()
        {
            UnityEngine.Object[] settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (settings.Length == 0)
            {
                return -1;
            }
            SerializedObject serialized = new SerializedObject(settings[0]);
            SerializedProperty property = serialized.FindProperty("m_StereoRenderingPath");
            return property != null ? property.intValue : -1;
        }

        private static Component TryAddComponent(string objectName, string typeName, string assemblyName)
        {
            GameObject gameObject = new GameObject(objectName);
            return TryAddComponent(gameObject, typeName, assemblyName);
        }

        private static Component TryAddComponent(GameObject gameObject, string typeName, string assemblyName)
        {
            Type type = Type.GetType(typeName + ", " + assemblyName);
            if (type == null)
            {
                foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName, false);
                    if (type != null)
                    {
                        break;
                    }
                }
            }
            return type != null && typeof(Component).IsAssignableFrom(type) ? gameObject.AddComponent(type) : null;
        }

        private static void SetSerializedColor(Component component, string propertyName, Color color)
        {
            if (component == null)
            {
                return;
            }
            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.colorValue = color;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void ConfigureEmitter(Component component, Renderer renderer)
        {
            if (component == null)
            {
                return;
            }
            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty color = serialized.FindProperty("Color");
            SerializedProperty channel = serialized.FindProperty("LightmapChannel");
            SerializedProperty initialized = serialized.FindProperty("Initialized");
            SerializedProperty renderers = serialized.FindProperty("EmissiveRenderers");
            if (color != null) color.colorValue = new Color(0.2f, 0.62f, 1f, 1f);
            if (channel != null) channel.intValue = 1;
            if (initialized != null) initialized.boolValue = true;
            if (renderers != null)
            {
                renderers.arraySize = 1;
                renderers.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root;
                }
            }
            return null;
        }

        private static List<GameObject> DisableGameObjectsWithComponentType(Scene scene, string typeNameFragment)
        {
            var gameObjects = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Component[] components = root.GetComponentsInChildren<Component>(true);
                foreach (Component component in components)
                {
                    if (component == null || !component.gameObject.activeSelf)
                    {
                        continue;
                    }
                    string fullName = component.GetType().FullName;
                    if (!string.IsNullOrEmpty(fullName) && fullName.Contains(typeNameFragment))
                    {
                        component.gameObject.SetActive(false);
                        gameObjects.Add(component.gameObject);
                    }
                }
            }
            return gameObjects;
        }
        private static List<Renderer> SetLabelRenderers(Scene scene, bool enabled)
        {
            var renderers = new List<Renderer>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (TextMesh text in root.GetComponentsInChildren<TextMesh>(true))
                {
                    Renderer renderer = text.GetComponent<Renderer>();
                    if (renderer != null && renderer.enabled != enabled)
                    {
                        renderer.enabled = enabled;
                        renderers.Add(renderer);
                    }
                }
            }
            return renderers;
        }

        private static T FindComponentInScene<T>(Scene scene, string objectName) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T[] components = root.GetComponentsInChildren<T>(true);
                foreach (T component in components)
                {
                    if (component.gameObject.name == objectName)
                    {
                        return component;
                    }
                }
            }
            return null;
        }

        private static void CreateLabel(Transform parent, string textValue, Vector3 position)
        {
            GameObject label = new GameObject(textValue);
            label.transform.SetParent(parent, false);
            label.transform.position = position;
            label.transform.rotation = Quaternion.identity;
            TextMesh text = label.AddComponent<TextMesh>();
            text.text = textValue;
            text.anchor = TextAnchor.MiddleCenter;
            text.fontSize = 48;
            text.characterSize = 0.04f;
            text.color = Color.white;
        }

        private static void LookAt(Transform transform, Vector3 target)
        {
            transform.rotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        }

        private static void SetFloat(Material material, string name, float value)
        {
            if (material.HasProperty(name)) material.SetFloat(name, value);
        }

        private static void SetColor(Material material, string name, Color value)
        {
            if (material.HasProperty(name)) material.SetColor(name, value);
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
        }
    }

    [Serializable]
    internal sealed class fShaderP5EnvironmentSnapshot
    {
        public string generatedUtc;
        public string fShaderVersion;
        public string unityVersion;
        public string buildTarget;
        public string colorSpace;
        public int stereoRenderingPathRaw;
        public string graphicsDevice;
        public string graphicsApi;
        public int graphicsMemoryMB;
        public string processor;
        public string qualityLevel;
        public string scene;
        public string overview;
        public string refractionLtcgi;
        public string refractionAB;
        public string note;
    }
}
