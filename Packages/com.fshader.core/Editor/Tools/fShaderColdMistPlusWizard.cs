using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor
{
    public static class fShaderColdMistPlusWizard
    {
        private const string InstanceName = "fShader Cold Mist Plus";
        private const string GeneratedRoot = "Assets/fShader Generated";

        [MenuItem("Tools/fShader/Create Cold Mist Plus for Selected Ice", true)]
        private static bool ValidateCreate()
        {
            return Selection.activeGameObject != null &&
                   Selection.activeGameObject.GetComponent<MeshRenderer>() != null &&
                   Selection.activeGameObject.GetComponent<MeshFilter>() != null;
        }

        [MenuItem("Tools/fShader/Create Cold Mist Plus for Selected Ice")]
        public static void Create()
        {
            GameObject selected = Selection.activeGameObject;
            MeshRenderer sourceRenderer = selected != null ? selected.GetComponent<MeshRenderer>() : null;
            if (sourceRenderer == null)
            {
                EditorUtility.DisplayDialog("fShader Cold Mist Plus", "Select an Ice GameObject with MeshRenderer and MeshFilter.", "OK");
                return;
            }

            Transform existing = selected.transform.Find(InstanceName);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                EditorUtility.DisplayDialog("fShader Cold Mist Plus", "This Ice object already has a Cold Mist Plus emitter.", "OK");
                return;
            }

            int emitterCount = Object.FindObjectsOfType<ParticleSystem>(true)
                .Count(system => system.gameObject.name == InstanceName);
            if (emitterCount >= 6 && !EditorUtility.DisplayDialog(
                    "fShader Cold Mist Plus",
                    "The scene already contains " + emitterCount +
                    " Cold Mist Plus emitters (up to 64 particles each). Transparent overdraw may become expensive. Continue?",
                    "Create",
                    "Cancel"))
            {
                return;
            }

            EnsureFolder(GeneratedRoot);
            EnsureFolder(GeneratedRoot + "/Materials");
            EnsureFolder(GeneratedRoot + "/Prefabs");
            string safeName = MakeSafeFileName(selected.name);
            Material mistMaterial = GetOrCreateMaterial(sourceRenderer, safeName);

            GameObject emitter = new GameObject(InstanceName);
            Undo.RegisterCreatedObjectUndo(emitter, "Create fShader Cold Mist Plus");
            emitter.transform.SetParent(selected.transform, false);

            ParticleSystem particles = emitter.AddComponent<ParticleSystem>();
            ParticleSystemRenderer particleRenderer = emitter.GetComponent<ParticleSystemRenderer>();
            ConfigureParticles(particles, particleRenderer, sourceRenderer, mistMaterial);

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                GeneratedRoot + "/Prefabs/" + safeName + " Cold Mist Plus.prefab");
            PrefabUtility.SaveAsPrefabAssetAndConnect(emitter, prefabPath, InteractionMode.UserAction);
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
            main.maxParticles = 64;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.34f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(ResolveMistColor(sourceRenderer));

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 12f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
            shape.meshRenderer = sourceRenderer;
            shape.normalOffset = 0.02f;

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = new ParticleSystem.MinMaxCurve(0.055f, 0.15f);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.72f, 0.18f),
                    new GradientAlphaKey(0.46f, 0.58f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            noise.frequency = 0.18f;
            noise.scrollSpeed = 0.04f;
            noise.damping = true;
            noise.octaveCount = 1;

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
            Bounds bounds = sourceRenderer.localBounds;
            bounds.Expand(new Vector3(0.8f, 1.8f, 0.8f));
            particleRenderer.localBounds = bounds;
        }

        private static Material GetOrCreateMaterial(MeshRenderer sourceRenderer, string safeName)
        {
            string path = GeneratedRoot + "/Materials/" + safeName + " Cold Mist Plus.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            Shader shader = Shader.Find("fShader/Effects/ColdMistPlus");
            if (shader == null) throw new System.InvalidOperationException("fShader/Effects/ColdMistPlus was not found.");

            material = new Material(shader);
            material.name = safeName + " Cold Mist Plus";
            material.SetColor("_TintColor", ResolveMistColor(sourceRenderer));
            material.SetFloat("_Opacity", 0.62f);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Color ResolveMistColor(MeshRenderer renderer)
        {
            Material source = renderer.sharedMaterial;
            Color color = new Color(0.55f, 0.86f, 1f, 0.34f);
            if (source != null)
            {
                string property = source.HasProperty("_IceColor") ? "_IceColor" : fShaderPropertyNames.BaseColor;
                if (source.HasProperty(property))
                {
                    color = Color.Lerp(source.GetColor(property), Color.white, 0.32f);
                    color.a = 0.34f;
                }
            }
            return color;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }

        private static string MakeSafeFileName(string value)
        {
            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return value;
        }
    }
}
