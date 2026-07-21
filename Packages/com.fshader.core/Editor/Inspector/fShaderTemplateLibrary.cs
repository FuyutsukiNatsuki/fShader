using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace fShader.Editor
{
    /// <summary>
    /// One-click material templates. Each template switches the shader, applies a
    /// tuned set of feature toggles and PBR values, and assigns matching textures
    /// from the "fShader Lite Gallery" sample, importing that sample automatically
    /// when it has not been imported yet.
    /// </summary>
    internal static class fShaderTemplateLibrary
    {
        private const string FoldoutKey = "fShader.Inspector.Templates";
        private const string CorePackageName = "com.fshader.core";
        private const string CorePackageManifest = "Packages/com.fshader.core/package.json";
        private const string GallerySampleName = "fShader Lite Gallery";
        private const string ProbeTexture = "Water_Base_Calm";

        private struct TextureAssignment
        {
            public string Property;
            public string FileName;
            public TextureAssignment(string property, string fileName)
            {
                Property = property;
                FileName = fileName;
            }
        }

        private struct FloatAssignment
        {
            public string Property;
            public float Value;
            public FloatAssignment(string property, float value)
            {
                Property = property;
                Value = value;
            }
        }

        private sealed class Template
        {
            public string LabelJa;
            public string LabelEn;
            public fShaderEdition Edition;
            public fShaderMode Mode;
            public TextureAssignment[] Textures;
            public FloatAssignment[] Floats;
        }

        private static readonly Template[] Templates =
        {
            new Template
            {
                LabelJa = "水", LabelEn = "Water",
                Edition = fShaderEdition.Lite, Mode = fShaderMode.Water,
                Textures = new[]
                {
                    new TextureAssignment("_BaseMap", "Water_Base_Calm"),
                    new TextureAssignment("_WaveNormalMap", "Water_WaveNormal_Fine"),
                    new TextureAssignment("_FoamMap", "Water_FoamMask")
                },
                Floats = new[]
                {
                    new FloatAssignment("_FSWaterWaveNormal", 1f),
                    new FloatAssignment("_FSWaterFoam", 1f),
                    new FloatAssignment("_FoamStrength", 0.55f),
                    new FloatAssignment("_Roughness", 0.16f)
                }
            },
            new Template
            {
                LabelJa = "氷", LabelEn = "Ice",
                Edition = fShaderEdition.Lite, Mode = fShaderMode.Ice,
                Textures = new[]
                {
                    new TextureAssignment("_BaseMap", "Ice_Base_Glacial"),
                    new TextureAssignment("_NormalMap", "Ice_Normal_Crystal"),
                    new TextureAssignment("_FrostMap", "Ice_FrostMask"),
                    new TextureAssignment("_CrackMap", "Ice_CrackMask")
                },
                Floats = new[]
                {
                    new FloatAssignment("_FSIceFrost", 1f),
                    new FloatAssignment("_FSIceCracks", 1f),
                    new FloatAssignment("_FSIceScatter", 1f),
                    new FloatAssignment("_FrostStrength", 0.65f),
                    new FloatAssignment("_Roughness", 0.42f)
                }
            },
            new Template
            {
                LabelJa = "ガラス", LabelEn = "Glass",
                Edition = fShaderEdition.Lite, Mode = fShaderMode.Glass,
                Textures = new[]
                {
                    new TextureAssignment("_CondensationMap", "Glass_Condensation_RGB"),
                    new TextureAssignment("_CondensationNormal", "Glass_CondensationNormal")
                },
                Floats = new[]
                {
                    new FloatAssignment("_FSGlassCondensation", 1f),
                    new FloatAssignment("_FSGlassDropletNormal", 1f),
                    new FloatAssignment("_CondensationAmount", 0.6f),
                    new FloatAssignment("_Roughness", 0.32f)
                }
            },
            new Template
            {
                LabelJa = "標準", LabelEn = "Standard",
                Edition = fShaderEdition.Lite, Mode = fShaderMode.Standard,
                Textures = new TextureAssignment[0],
                Floats = new[]
                {
                    new FloatAssignment("_Roughness", 0.5f),
                    new FloatAssignment("_Metallic", 0f),
                    new FloatAssignment("_ReflectionStrength", 0.65f)
                }
            },
            new Template
            {
                LabelJa = "水", LabelEn = "Water",
                Edition = fShaderEdition.Plus, Mode = fShaderMode.Water,
                Textures = new[]
                {
                    new TextureAssignment("_BaseMap", "Water_Base_Calm"),
                    new TextureAssignment("_WaveNormalMap", "Water_WaveNormal_Fine"),
                    new TextureAssignment("_WaveNormalMap2", "Water_WaveNormal_Fine"),
                    new TextureAssignment("_FoamMap", "Water_FoamMask")
                },
                Floats = new[]
                {
                    new FloatAssignment("_FSWaterWaveNormal", 1f),
                    new FloatAssignment("_FSWaterFoam", 1f),
                    new FloatAssignment("_FoamStrength", 0.75f),
                    new FloatAssignment("_Roughness", 0.12f),
                    new FloatAssignment("_FSBoxProjection", 1f),
                    new FloatAssignment("_FSScreenRefraction", 0f),
                    new FloatAssignment("_LTCGI", 0f)
                }
            },
            new Template
            {
                LabelJa = "氷", LabelEn = "Ice",
                Edition = fShaderEdition.Plus, Mode = fShaderMode.Ice,
                Textures = new[]
                {
                    new TextureAssignment("_BaseMap", "Ice_Base_Glacial"),
                    new TextureAssignment("_NormalMap", "Ice_Normal_Crystal"),
                    new TextureAssignment("_FrostMap", "Ice_FrostMask"),
                    new TextureAssignment("_CrackMap", "Ice_CrackMask")
                },
                Floats = new[]
                {
                    new FloatAssignment("_FSIceTransparent", 0f),
                    new FloatAssignment("_FSIceFrost", 1f),
                    new FloatAssignment("_FSIceCracks", 1f),
                    new FloatAssignment("_FSIceBackLight", 1f),
                    new FloatAssignment("_FrostStrength", 0.8f),
                    new FloatAssignment("_Roughness", 0.34f),
                    new FloatAssignment("_FSBoxProjection", 1f),
                    new FloatAssignment("_FSScreenRefraction", 0f)
                }
            },
            new Template
            {
                LabelJa = "ガラス", LabelEn = "Glass",
                Edition = fShaderEdition.Plus, Mode = fShaderMode.Glass,
                Textures = new[]
                {
                    new TextureAssignment("_CondensationMap", "Glass_Condensation_RGB"),
                    new TextureAssignment("_CondensationNormal", "Glass_CondensationNormal")
                },
                Floats = new[]
                {
                    new FloatAssignment("_FSGlassCondensation", 1f),
                    new FloatAssignment("_FSGlassDropletNormal", 1f),
                    new FloatAssignment("_CondensationAmount", 0.6f),
                    new FloatAssignment("_Roughness", 0.12f),
                    new FloatAssignment("_FSBoxProjection", 1f),
                    new FloatAssignment("_FSScreenRefraction", 0f),
                    new FloatAssignment("_LTCGI", 0f)
                }
            },
            new Template
            {
                LabelJa = "標準", LabelEn = "Standard",
                Edition = fShaderEdition.Plus, Mode = fShaderMode.Standard,
                Textures = new TextureAssignment[0],
                Floats = new[]
                {
                    new FloatAssignment("_Roughness", 0.5f),
                    new FloatAssignment("_Metallic", 0f),
                    new FloatAssignment("_ReflectionStrength", 0.75f),
                    new FloatAssignment("_FSBoxProjection", 1f),
                    new FloatAssignment("_LTCGI", 0f)
                }
            }
        };

        public static void DrawSection(MaterialEditor editor, bool japanese)
        {
            bool expanded = EditorPrefs.GetBool(FoldoutKey, true);
            bool next = EditorGUILayout.Foldout(
                expanded,
                japanese ? "テンプレート" : "Templates",
                true,
                EditorStyles.foldoutHeader);
            if (next != expanded) EditorPrefs.SetBool(FoldoutKey, next);
            if (!next) return;

            EditorGUILayout.HelpBox(
                japanese
                    ? "ワンクリックでシェーダーとサンプルテクスチャを割り当てます。サンプルが未インポートの場合は自動でインポートします（Standardはテクスチャなし）。"
                    : "One click assigns the shader plus matching sample textures, importing the sample automatically if needed (Standard assigns no textures).",
                MessageType.None);

            DrawEditionRow(editor, fShaderEdition.Lite, japanese);
            DrawEditionRow(editor, fShaderEdition.Plus, japanese);
            EditorGUILayout.Space(3f);
        }

        private static void DrawEditionRow(MaterialEditor editor, fShaderEdition edition, bool japanese)
        {
            Template selected = null;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(edition == fShaderEdition.Lite ? "Lite" : "Plus", GUILayout.Width(40f));
            foreach (Template template in Templates.Where(candidate => candidate.Edition == edition))
            {
                if (GUILayout.Button(japanese ? template.LabelJa : template.LabelEn))
                {
                    selected = template;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (selected != null)
            {
                Apply(editor, selected, japanese);
                GUIUtility.ExitGUI();
            }
        }

        private static void Apply(MaterialEditor editor, Template template, bool japanese)
        {
            Shader shader = Shader.Find(fShaderShaderCatalog.GetShaderName(template.Edition, template.Mode));
            if (shader == null)
            {
                EditorUtility.DisplayDialog(
                    "fShader",
                    "Shader not found: " + fShaderShaderCatalog.GetShaderName(template.Edition, template.Mode),
                    "OK");
                return;
            }

            bool needTextures = template.Textures != null && template.Textures.Length > 0;
            bool texturesReady = !needTextures || EnsureGalleryTextures();

            Material[] materials = editor.targets.OfType<Material>().ToArray();
            Undo.RecordObjects(materials, "Apply fShader Template");
            foreach (Material material in materials)
            {
                material.shader = shader;
                material.SetFloat(fShaderPropertyNames.Version, 0.5f);
                material.SetFloat(fShaderPropertyNames.Edition, (float)template.Edition);
                material.SetFloat(fShaderPropertyNames.Mode, (float)template.Mode);

                if (template.Floats != null)
                {
                    foreach (FloatAssignment assignment in template.Floats)
                    {
                        if (material.HasProperty(assignment.Property))
                        {
                            material.SetFloat(assignment.Property, assignment.Value);
                        }
                    }
                }

                if (texturesReady && template.Textures != null)
                {
                    foreach (TextureAssignment assignment in template.Textures)
                    {
                        if (!material.HasProperty(assignment.Property)) continue;
                        Texture texture = FindSampleTexture(assignment.FileName);
                        if (texture != null) material.SetTexture(assignment.Property, texture);
                    }
                }

                EditorUtility.SetDirty(material);
            }

            foreach (Material material in materials)
            {
                fShaderInspector.SyncMaterial(material);
            }
            editor.PropertiesChanged();

            if (needTextures && !texturesReady)
            {
                EditorUtility.DisplayDialog(
                    "fShader",
                    japanese
                        ? "Gallery Texturesを自動インポートできなかったため、テクスチャなしでテンプレートを適用しました。Package Managerから\"fShader Lite Gallery\"をインポート後、もう一度テンプレートを押してください。"
                        : "Applied the template without textures because \"fShader Lite Gallery\" could not be imported automatically. Import it from the Package Manager and click the template again.",
                    "OK");
            }
        }

        private static bool EnsureGalleryTextures()
        {
            if (FindSampleTexture(ProbeTexture) != null)
            {
                return true;
            }

            UnityEditor.PackageManager.PackageInfo package =
                UnityEditor.PackageManager.PackageInfo.FindForAssetPath(CorePackageManifest);
            if (package == null || string.IsNullOrEmpty(package.version))
            {
                return false;
            }

            var samples = Sample.FindByPackage(CorePackageName, package.version);
            if (samples == null)
            {
                return false;
            }

            foreach (Sample sample in samples)
            {
                if (sample.displayName != GallerySampleName)
                {
                    continue;
                }

                sample.Import(Sample.ImportOptions.OverridePreviousImports | Sample.ImportOptions.HideImportWindow);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                return FindSampleTexture(ProbeTexture) != null;
            }

            return false;
        }

        private static Texture FindSampleTexture(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets(fileName + " t:Texture");
            Texture fallback = null;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != fileName)
                {
                    continue;
                }
                Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (texture == null)
                {
                    continue;
                }
                if (path.Replace('\\', '/').Contains("/Samples/"))
                {
                    return texture;
                }
                fallback = fallback ?? texture;
            }
            return fallback;
        }
    }
}
