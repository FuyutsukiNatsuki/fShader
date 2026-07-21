using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor
{
    // Serializable form of a user template. Stored as JSON under Assets/fShader Templates.
    // Textures are referenced by GUID (with a path fallback), so a template resolves
    // within the same project; cross-project use falls back to a name search.
    [Serializable]
    internal sealed class fShaderTemplateData
    {
        public string schemaVersion = "1";
        public string name = string.Empty;
        public string shaderName = string.Empty;
        public int edition;
        public int mode;
        public int renderQueue = -1;
        public List<FloatEntry> floats = new List<FloatEntry>();
        public List<ColorEntry> colors = new List<ColorEntry>();
        public List<VectorEntry> vectors = new List<VectorEntry>();
        public List<TextureEntry> textures = new List<TextureEntry>();

        [Serializable]
        public struct FloatEntry
        {
            public string name;
            public float value;
        }

        [Serializable]
        public struct ColorEntry
        {
            public string name;
            public float r, g, b, a;
        }

        [Serializable]
        public struct VectorEntry
        {
            public string name;
            public float x, y, z, w;
        }

        [Serializable]
        public struct TextureEntry
        {
            public string name;
            public string guid;
            public string path;
        }
    }

    public static class fShaderTemplateIO
    {
        public const string TemplateFolder = "Assets/fShader Templates";
        private const string Extension = ".json";

        public struct UserTemplate
        {
            public string DisplayName;
            public string AssetPath;
        }

        public static List<UserTemplate> ListUserTemplates()
        {
            var result = new List<UserTemplate>();
            string full = Path.GetFullPath(TemplateFolder);
            if (!Directory.Exists(full))
            {
                return result;
            }

            foreach (string file in Directory.GetFiles(full, "*" + Extension))
            {
                fShaderTemplateData data = TryRead(file);
                if (data == null || string.IsNullOrEmpty(data.shaderName))
                {
                    continue;
                }
                result.Add(new UserTemplate
                {
                    DisplayName = string.IsNullOrEmpty(data.name)
                        ? Path.GetFileNameWithoutExtension(file)
                        : data.name,
                    AssetPath = ToAssetPath(file)
                });
            }
            result.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        public static string ExportMaterial(Material material, string templateName)
        {
            if (material == null || material.shader == null)
            {
                return null;
            }

            fShaderShaderCatalog.TryParse(material.shader.name, out fShaderEdition edition, out fShaderMode mode);
            var data = new fShaderTemplateData
            {
                name = templateName,
                shaderName = material.shader.name,
                edition = (int)edition,
                mode = (int)mode,
                renderQueue = material.renderQueue
            };

            Shader shader = material.shader;
            int count = ShaderUtil.GetPropertyCount(shader);
            for (int i = 0; i < count; i++)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                switch (ShaderUtil.GetPropertyType(shader, i))
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        Color color = material.GetColor(propertyName);
                        data.colors.Add(new fShaderTemplateData.ColorEntry
                        {
                            name = propertyName, r = color.r, g = color.g, b = color.b, a = color.a
                        });
                        break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                        Vector4 vector = material.GetVector(propertyName);
                        data.vectors.Add(new fShaderTemplateData.VectorEntry
                        {
                            name = propertyName, x = vector.x, y = vector.y, z = vector.z, w = vector.w
                        });
                        break;
                    case ShaderUtil.ShaderPropertyType.Float:
                    case ShaderUtil.ShaderPropertyType.Range:
                        data.floats.Add(new fShaderTemplateData.FloatEntry
                        {
                            name = propertyName, value = material.GetFloat(propertyName)
                        });
                        break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        Texture texture = material.GetTexture(propertyName);
                        string path = texture != null ? AssetDatabase.GetAssetPath(texture) : string.Empty;
                        if (string.IsNullOrEmpty(path))
                        {
                            break;
                        }
                        data.textures.Add(new fShaderTemplateData.TextureEntry
                        {
                            name = propertyName,
                            guid = AssetDatabase.AssetPathToGUID(path),
                            path = path
                        });
                        break;
                }
            }

            EnsureFolder();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                TemplateFolder + "/" + MakeSafeFileName(templateName) + Extension);
            File.WriteAllText(Path.GetFullPath(assetPath), JsonUtility.ToJson(data, true));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            return assetPath;
        }

        public static bool ImportFromFile(string sourceFullPath)
        {
            if (string.IsNullOrEmpty(sourceFullPath) || !File.Exists(sourceFullPath))
            {
                return false;
            }

            fShaderTemplateData data = TryRead(sourceFullPath);
            if (data == null || string.IsNullOrEmpty(data.shaderName))
            {
                return false;
            }

            EnsureFolder();
            string baseName = string.IsNullOrEmpty(data.name)
                ? Path.GetFileNameWithoutExtension(sourceFullPath)
                : data.name;
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                TemplateFolder + "/" + MakeSafeFileName(baseName) + Extension);
            File.Copy(sourceFullPath, Path.GetFullPath(assetPath), false);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            return true;
        }

        public static bool Apply(MaterialEditor editor, string assetPath)
        {
            fShaderTemplateData data = TryRead(Path.GetFullPath(assetPath));
            if (data == null || string.IsNullOrEmpty(data.shaderName))
            {
                EditorUtility.DisplayDialog("fShader", "Template file is invalid: " + assetPath, "OK");
                return false;
            }
            if (Shader.Find(data.shaderName) == null)
            {
                EditorUtility.DisplayDialog("fShader", "Shader not found: " + data.shaderName, "OK");
                return false;
            }

            bool applied = ApplyToMaterials(editor.targets.OfType<Material>().ToArray(), assetPath);
            if (applied) editor.PropertiesChanged();
            return applied;
        }

        // Editor-independent apply, used by the inspector and by tests.
        public static bool ApplyToMaterials(Material[] materials, string assetPath)
        {
            fShaderTemplateData data = TryRead(Path.GetFullPath(assetPath));
            if (data == null || string.IsNullOrEmpty(data.shaderName))
            {
                return false;
            }

            Shader shader = Shader.Find(data.shaderName);
            if (shader == null || materials == null)
            {
                return false;
            }

            Undo.RecordObjects(materials, "Apply fShader Template");
            foreach (Material material in materials)
            {
                material.shader = shader;
                foreach (fShaderTemplateData.FloatEntry entry in data.floats)
                {
                    if (material.HasProperty(entry.name)) material.SetFloat(entry.name, entry.value);
                }
                foreach (fShaderTemplateData.ColorEntry entry in data.colors)
                {
                    if (material.HasProperty(entry.name)) material.SetColor(entry.name, new Color(entry.r, entry.g, entry.b, entry.a));
                }
                foreach (fShaderTemplateData.VectorEntry entry in data.vectors)
                {
                    if (material.HasProperty(entry.name)) material.SetVector(entry.name, new Vector4(entry.x, entry.y, entry.z, entry.w));
                }
                foreach (fShaderTemplateData.TextureEntry entry in data.textures)
                {
                    if (!material.HasProperty(entry.name)) continue;
                    Texture texture = ResolveTexture(entry);
                    if (texture != null) material.SetTexture(entry.name, texture);
                }
                if (data.renderQueue >= 0)
                {
                    material.renderQueue = data.renderQueue;
                }
                EditorUtility.SetDirty(material);
            }

            foreach (Material material in materials)
            {
                fShaderInspector.SyncMaterial(material);
            }
            return true;
        }

        private static Texture ResolveTexture(fShaderTemplateData.TextureEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(entry.guid);
                if (!string.IsNullOrEmpty(path))
                {
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
                    if (texture != null) return texture;
                }
            }
            if (!string.IsNullOrEmpty(entry.path))
            {
                Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(entry.path);
                if (texture != null) return texture;
                // last resort: match by file name anywhere in the project
                string fileName = Path.GetFileNameWithoutExtension(entry.path);
                foreach (string guid in AssetDatabase.FindAssets(fileName + " t:Texture"))
                {
                    string found = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(found) == fileName)
                    {
                        Texture texture2 = AssetDatabase.LoadAssetAtPath<Texture>(found);
                        if (texture2 != null) return texture2;
                    }
                }
            }
            return null;
        }

        private static fShaderTemplateData TryRead(string fullPath)
        {
            try
            {
                string json = File.ReadAllText(fullPath);
                return JsonUtility.FromJson<fShaderTemplateData>(json);
            }
            catch
            {
                return null;
            }
        }

        private static void EnsureFolder()
        {
            if (AssetDatabase.IsValidFolder(TemplateFolder))
            {
                return;
            }
            AssetDatabase.CreateFolder("Assets", "fShader Templates");
        }

        private static string ToAssetPath(string fullPath)
        {
            string projectRoot = Path.GetFullPath("Assets");
            string normalized = Path.GetFullPath(fullPath);
            if (normalized.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + normalized.Substring(projectRoot.Length).Replace('\\', '/');
            }
            return fullPath.Replace('\\', '/');
        }

        private static string MakeSafeFileName(string value)
        {
            if (string.IsNullOrEmpty(value)) return "fShader Template";
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }
            return value;
        }
    }
}
