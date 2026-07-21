using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor
{
    public static class fShaderColdMistWizard
    {
        private const string InstanceName = "fShader Cold Mist Lite";
        private const string GeneratedRoot = "Assets/fShader Generated";
        private const string MaterialPath = GeneratedRoot + "/Materials/fShader Cold Mist Lite.mat";

        [MenuItem("Tools/fShader/Create Cold Mist Lite for Selected Ice", true)]
        private static bool ValidateCreate()
        {
            return Selection.activeGameObject != null &&
                   Selection.activeGameObject.GetComponent<MeshRenderer>() != null &&
                   Selection.activeGameObject.GetComponent<MeshFilter>() != null;
        }

        [MenuItem("Tools/fShader/Create Cold Mist Lite for Selected Ice")]
        public static void Create()
        {
            GameObject selected = Selection.activeGameObject;
            MeshRenderer sourceRenderer = selected != null
                ? selected.GetComponent<MeshRenderer>()
                : null;
            if (sourceRenderer == null)
            {
                EditorUtility.DisplayDialog(
                    "fShader Cold Mist",
                    "Select an Ice GameObject with MeshRenderer and MeshFilter.",
                    "OK");
                return;
            }

            Transform existing = selected.transform.Find(InstanceName);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                EditorUtility.DisplayDialog(
                    "fShader Cold Mist",
                    "This Ice object already has a Cold Mist Lite emitter.",
                    "OK");
                return;
            }

            int existingEmitterCount = Object.FindObjectsOfType<ParticleSystem>(true)
                .Count(system => system.gameObject.name == InstanceName);
            if (existingEmitterCount >= 8 && !EditorUtility.DisplayDialog(
                    "fShader Cold Mist",
                    "The scene already contains " + existingEmitterCount +
                    " Cold Mist Lite emitters. More transparent emitters can increase overdraw. Continue?",
                    "Create",
                    "Cancel"))
            {
                return;
            }

            EnsureFolder(GeneratedRoot);
            EnsureFolder(GeneratedRoot + "/Materials");
            EnsureFolder(GeneratedRoot + "/Prefabs");
            Material mistMaterial = GetOrCreateMaterial(sourceRenderer);

            GameObject emitter = new GameObject(InstanceName);
            Undo.RegisterCreatedObjectUndo(emitter, "Create fShader Cold Mist Lite");
            emitter.transform.SetParent(selected.transform, false);

            ParticleSystem particles = emitter.AddComponent<ParticleSystem>();
            ParticleSystemRenderer particleRenderer = emitter.GetComponent<ParticleSystemRenderer>();
            ConfigureParticles(particles, particleRenderer, sourceRenderer, mistMaterial);

            string safeName = MakeSafeFileName(selected.name);
            string prefabPath = GeneratedRoot + "/Prefabs/" + safeName + " Cold Mist Lite.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(
                emitter,
                prefabPath,
                InteractionMode.UserAction);

            Selection.activeGameObject = emitter;
            EditorGUIUtility.PingObject(emitter);
        }

        private static void ConfigureParticles(
            ParticleSystem particles,
            ParticleSystemRenderer particleRenderer,
            MeshRenderer sourceRenderer,
            Material material)
        {
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.maxParticles = 24;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.015f, 0.06f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.24f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(ResolveMistColor(sourceRenderer));

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 6f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
            shape.meshRenderer = sourceRenderer;
            shape.normalOffset = 0.015f;

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = new ParticleSystem.MinMaxCurve(0.045f, 0.12f);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient alphaGradient = new Gradient();
            alphaGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.65f, 0.22f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(alphaGradient);

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = false;
            ParticleSystem.CollisionModule collision = particles.collision;
            collision.enabled = false;
            ParticleSystem.TrailModule trails = particles.trails;
            trails.enabled = false;
            ParticleSystem.SubEmittersModule subEmitters = particles.subEmitters;
            subEmitters.enabled = false;

            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.alignment = ParticleSystemRenderSpace.View;
            particleRenderer.sortMode = ParticleSystemSortMode.OldestInFront;
            particleRenderer.material = material;
            Bounds sourceBounds = sourceRenderer.localBounds;
            sourceBounds.Expand(new Vector3(0.5f, 1.2f, 0.5f));
            particleRenderer.localBounds = sourceBounds;
        }

        private static Material GetOrCreateMaterial(MeshRenderer sourceRenderer)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("fShader/Effects/ColdMist");
            if (shader == null)
            {
                throw new System.InvalidOperationException("fShader/Effects/ColdMist was not found.");
            }

            material = new Material(shader);
            material.name = "fShader Cold Mist Lite";
            material.SetColor("_TintColor", ResolveMistColor(sourceRenderer));
            material.SetFloat("_Opacity", 0.55f);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        private static Color ResolveMistColor(MeshRenderer renderer)
        {
            Material source = renderer.sharedMaterial;
            Color color = new Color(0.45f, 0.8f, 1f, 0.32f);
            if (source != null && source.HasProperty(fShaderPropertyNames.BaseColor))
            {
                color = source.GetColor(fShaderPropertyNames.BaseColor);
                color = Color.Lerp(color, Color.white, 0.28f);
                color.a = 0.32f;
            }
            return color;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string name = path.Substring(slash + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static string MakeSafeFileName(string value)
        {
            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }
            return value;
        }
    }
}
