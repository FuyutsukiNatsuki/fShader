using System.IO;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor
{
    public sealed class fShaderARMHPackerWindow : EditorWindow
    {
        private enum OutputSize
        {
            SourceMax = 0,
            Size256 = 256,
            Size512 = 512,
            Size1024 = 1024,
            Size2048 = 2048
        }

        private Texture2D aoTexture;
        private Texture2D roughnessTexture;
        private Texture2D metallicTexture;
        private Texture2D heightTexture;
        private float defaultAO = 1f;
        private float defaultRoughness = 0.5f;
        private float defaultMetallic;
        private float defaultHeight = 0.5f;
        private bool invertRoughness;
        private OutputSize outputSize = OutputSize.SourceMax;

        [MenuItem("Tools/fShader/ARMH Texture Packer")]
        public static void ShowWindow()
        {
            fShaderARMHPackerWindow window = GetWindow<fShaderARMHPackerWindow>();
            window.titleContent = new GUIContent("fShader ARMH");
            window.minSize = new Vector2(420f, 360f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("ARMH Texture Packer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "R = Ambient Occlusion, G = Roughness, B = Metallic, A = Height. " +
                "Missing inputs use the fallback values below.",
                MessageType.Info);

            DrawChannel("AO (R)", ref aoTexture, ref defaultAO);
            DrawChannel("Roughness (G)", ref roughnessTexture, ref defaultRoughness);
            invertRoughness = EditorGUILayout.ToggleLeft(
                "Input G texture contains Smoothness (invert)",
                invertRoughness);
            DrawChannel("Metallic (B)", ref metallicTexture, ref defaultMetallic);
            DrawChannel("Height (A)", ref heightTexture, ref defaultHeight);

            EditorGUILayout.Space(6f);
            outputSize = (OutputSize)EditorGUILayout.EnumPopup("Output Size", outputSize);

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Pack and Save PNG", GUILayout.Height(28f)))
            {
                PackAndSave();
            }
        }

        private static void DrawChannel(
            string label,
            ref Texture2D texture,
            ref float fallback)
        {
            EditorGUILayout.BeginHorizontal();
            texture = (Texture2D)EditorGUILayout.ObjectField(
                label,
                texture,
                typeof(Texture2D),
                false);
            fallback = EditorGUILayout.Slider(fallback, 0f, 1f, GUILayout.Width(170f));
            EditorGUILayout.EndHorizontal();
        }

        private void PackAndSave()
        {
            int size = ResolveSize();
            string path = EditorUtility.SaveFilePanelInProject(
                "Save fShader ARMH Texture",
                "fShader_ARMH",
                "png",
                "Choose a project location for the packed texture.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Color32[] ao = ReadChannel(aoTexture, size, defaultAO);
            Color32[] roughness = ReadChannel(roughnessTexture, size, defaultRoughness);
            Color32[] metallic = ReadChannel(metallicTexture, size, defaultMetallic);
            Color32[] height = ReadChannel(heightTexture, size, defaultHeight);
            Color32[] packedPixels = new Color32[size * size];

            for (int i = 0; i < packedPixels.Length; i++)
            {
                byte roughnessValue = roughness[i].r;
                if (invertRoughness)
                {
                    roughnessValue = (byte)(255 - roughnessValue);
                }

                packedPixels[i] = new Color32(
                    ao[i].r,
                    roughnessValue,
                    metallic[i].r,
                    height[i].r);
            }

            Texture2D packed = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            packed.SetPixels32(packedPixels);
            packed.Apply(false, false);
            byte[] png = packed.EncodeToPNG();
            DestroyImmediate(packed);

            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.sRGBTexture = false;
                importer.mipmapEnabled = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private int ResolveSize()
        {
            if (outputSize != OutputSize.SourceMax)
            {
                return (int)outputSize;
            }

            int sourceMax = 0;
            Texture2D[] inputs =
            {
                aoTexture,
                roughnessTexture,
                metallicTexture,
                heightTexture
            };
            foreach (Texture2D input in inputs)
            {
                if (input != null)
                {
                    sourceMax = Mathf.Max(sourceMax, input.width, input.height);
                }
            }

            return Mathf.Clamp(Mathf.NextPowerOfTwo(sourceMax > 0 ? sourceMax : 1024), 256, 2048);
        }

        private static Color32[] ReadChannel(Texture2D source, int size, float fallback)
        {
            if (source == null)
            {
                byte value = (byte)Mathf.RoundToInt(Mathf.Clamp01(fallback) * 255f);
                Color32[] solid = new Color32[size * size];
                Color32 color = new Color32(value, value, value, value);
                for (int i = 0; i < solid.Length; i++)
                {
                    solid[i] = color;
                }
                return solid;
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(
                size,
                size,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;

            Texture2D copy = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            copy.ReadPixels(new Rect(0f, 0f, size, size), 0, 0, false);
            copy.Apply(false, false);
            Color32[] result = copy.GetPixels32();

            DestroyImmediate(copy);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            return result;
        }
    }
}
