using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace fShader.Editor
{
    public sealed class fShaderInspector : ShaderGUI
    {
        private const string LanguageKey = "fShader.Inspector.Language";
        private const string SurfaceFoldoutKey = "fShader.Inspector.Surface";
        private const string PBRFoldoutKey = "fShader.Inspector.PBR";
        private const string DiagnosticsFoldoutKey = "fShader.Inspector.Diagnostics";

        private static bool japanese;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material firstMaterial = materialEditor.target as Material;
            if (firstMaterial == null)
            {
                return;
            }

            japanese = EditorPrefs.GetBool(LanguageKey, true);
            DrawHeader();

            if (!fShaderShaderCatalog.TryParse(
                    firstMaterial.shader != null ? firstMaterial.shader.name : string.Empty,
                    out fShaderEdition edition,
                    out fShaderMode mode))
            {
                EditorGUILayout.HelpBox(
                    T("fShaderの公開バリアントではありません。", "This is not a public fShader variant."),
                    MessageType.Error);
                base.OnGUI(materialEditor, properties);
                return;
            }

            DrawVariantToolbar(materialEditor, ref properties, ref firstMaterial, edition, mode);

            EditorGUI.BeginChangeCheck();
            DrawSurface(materialEditor, properties);
            DrawPBR(materialEditor, properties);
            fShaderP2Inspector.Draw(materialEditor, properties, edition, mode, firstMaterial, japanese);
            fShaderP3Inspector.Draw(materialEditor, properties, edition, mode, firstMaterial, japanese);
            fShaderP4Inspector.Draw(materialEditor, properties, edition, mode, firstMaterial, japanese);
            DrawDiagnostics(materialEditor, properties, firstMaterial);
            materialEditor.EnableInstancingField();
            if (EditorGUI.EndChangeCheck())
            {
                SyncKeywords(materialEditor.targets.OfType<Material>().ToArray());
                materialEditor.PropertiesChanged();
            }
            else
            {
                SyncKeywords(materialEditor.targets.OfType<Material>().ToArray());
            }

            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(T("バージョン", "Version"), fShaderShaderCatalog.Version);
                EditorGUILayout.TextField(T("描画経路", "Render Path"), "BRP ForwardBase / 1 pass");
                EditorGUILayout.IntField(T("Shaderパス総数", "Total Shader Passes"), firstMaterial.shader.passCount);
            }
        }

        private static void DrawHeader()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("fShader", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            bool nextJapanese = GUILayout.Toolbar(japanese ? 0 : 1, new[] { "日本語", "English" }, GUILayout.Width(150f)) == 0;
            if (nextJapanese != japanese)
            {
                japanese = nextJapanese;
                EditorPrefs.SetBool(LanguageKey, japanese);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label(
                T("VRChat Worlds向け Water / Ice / Glass", "Water / Ice / Glass for VRChat Worlds"),
                EditorStyles.miniLabel);
            EditorGUILayout.Space(4f);
        }

        private static void DrawVariantToolbar(
            MaterialEditor materialEditor,
            ref MaterialProperty[] properties,
            ref Material firstMaterial,
            fShaderEdition edition,
            fShaderMode mode)
        {
            EditorGUI.BeginChangeCheck();
            int nextEdition = GUILayout.Toolbar((int)edition, fShaderShaderCatalog.EditionLabels);
            int nextMode = GUILayout.Toolbar((int)mode, fShaderShaderCatalog.ModeLabels);
            if (EditorGUI.EndChangeCheck())
            {
                SwitchShader(materialEditor, (fShaderEdition)nextEdition, (fShaderMode)nextMode);
                firstMaterial = materialEditor.target as Material;
                properties = MaterialEditor.GetMaterialProperties(materialEditor.targets);
            }
        }

        private static void DrawSurface(MaterialEditor editor, MaterialProperty[] properties)
        {
            bool expanded = DrawFoldout(SurfaceFoldoutKey, T("基本サーフェス", "Surface"), true);
            if (!expanded)
            {
                return;
            }

            MaterialProperty baseMap = FindProperty(fShaderPropertyNames.BaseMap, properties, false);
            MaterialProperty baseColor = FindProperty(fShaderPropertyNames.BaseColor, properties, false);
            MaterialProperty opacity = FindProperty(fShaderPropertyNames.Opacity, properties, false);
            if (baseMap != null)
            {
                editor.TexturePropertySingleLine(
                    new GUIContent(T("ベースマップ", "Base Map")),
                    baseMap,
                    baseColor);
                editor.TextureScaleOffsetProperty(baseMap);
            }
            else if (baseColor != null)
            {
                editor.ShaderProperty(baseColor, T("ベースカラー", "Base Color"));
            }
            if (opacity != null)
            {
                editor.ShaderProperty(opacity, T("不透明度", "Opacity"));
            }
            EditorGUILayout.Space(3f);
        }

        private static void DrawPBR(MaterialEditor editor, MaterialProperty[] properties)
        {
            bool expanded = DrawFoldout(PBRFoldoutKey, T("PBR入力", "PBR Inputs"), true);
            if (!expanded)
            {
                return;
            }

            MaterialProperty armh = FindProperty(fShaderPropertyNames.ARMHMap, properties, false);
            MaterialProperty ao = FindProperty(fShaderPropertyNames.AOStrength, properties, false);
            MaterialProperty roughness = FindProperty(fShaderPropertyNames.Roughness, properties, false);
            MaterialProperty metallic = FindProperty(fShaderPropertyNames.Metallic, properties, false);
            MaterialProperty height = FindProperty(fShaderPropertyNames.HeightScale, properties, false);
            MaterialProperty normal = FindProperty(fShaderPropertyNames.NormalMap, properties, false);
            MaterialProperty normalScale = FindProperty(fShaderPropertyNames.NormalScale, properties, false);
            MaterialProperty reflection = FindProperty(fShaderPropertyNames.ReflectionStrength, properties, false);
            MaterialProperty ior = FindProperty(fShaderPropertyNames.IOR, properties, false);

            if (armh != null)
            {
                editor.TexturePropertySingleLine(new GUIContent("ARMH (R/G/B/A)"), armh);
            }
            if (ao != null) editor.ShaderProperty(ao, T("AO強度", "AO Strength"));
            if (roughness != null) editor.ShaderProperty(roughness, T("ラフネス（マップ未使用時）", "Roughness (map fallback)"));
            if (metallic != null) editor.ShaderProperty(metallic, T("メタリック（マップ未使用時）", "Metallic (map fallback)"));
            if (height != null) editor.ShaderProperty(height, T("ハイト強度（予約）", "Height Scale (reserved)"));
            if (normal != null)
            {
                editor.TexturePropertySingleLine(
                    new GUIContent(T("ノーマルマップ", "Normal Map")),
                    normal,
                    normalScale);
            }
            if (reflection != null) editor.ShaderProperty(reflection, T("反射強度", "Reflection Strength"));
            if (ior != null) editor.ShaderProperty(ior, T("屈折率", "Index of Refraction"));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(T("簡易プリセット", "Quick Presets"), GUILayout.Width(90f));
            if (GUILayout.Button(T("マット", "Matte"))) ApplyPreset(editor, 0.8f, 0f, 0.25f);
            if (GUILayout.Button(T("標準", "Balanced"))) ApplyPreset(editor, 0.35f, 0f, 0.65f);
            if (GUILayout.Button(T("研磨", "Polished"))) ApplyPreset(editor, 0.08f, 0f, 1f);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(T("ARMHパッカーを開く", "Open ARMH Texture Packer")))
            {
                fShaderARMHPackerWindow.ShowWindow();
            }
            EditorGUILayout.Space(3f);
        }

        private static void DrawDiagnostics(
            MaterialEditor editor,
            MaterialProperty[] properties,
            Material material)
        {
            bool expanded = DrawFoldout(DiagnosticsFoldoutKey, T("検証・デバッグ", "Validation & Debug"), false);
            if (!expanded)
            {
                return;
            }

            MaterialProperty debug = FindProperty(fShaderP1PropertyNames.DebugView, properties, false);
            if (debug != null)
            {
                string[] labels = japanese
                    ? new[] { "最終表示", "ベースカラー", "AO", "ラフネス", "メタリック", "ハイト", "ワールドノーマル", "Vertex R", "Vertex G", "Vertex B", "Vertex A" }
                    : new[] { "Final", "Base Color", "AO", "Roughness", "Metallic", "Height", "World Normal", "Vertex R", "Vertex G", "Vertex B", "Vertex A" };
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = debug.hasMixedValue;
                int next = EditorGUILayout.Popup(T("デバッグ表示", "Debug View"), Mathf.RoundToInt(debug.floatValue), labels);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    debug.floatValue = next;
                }
            }

            if (PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                EditorGUILayout.HelpBox(
                    T("Linear Color Spaceを推奨します。", "Linear Color Space is recommended."),
                    MessageType.Warning);
            }
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                EditorGUILayout.HelpBox(
                    T("fShader P3はBuilt-in Render Pipeline専用です。", "fShader P4 targets the Built-in Render Pipeline."),
                    MessageType.Error);
            }

            ValidateTextureImport(
                material,
                fShaderPropertyNames.ARMHMap,
                false,
                T("ARMHはsRGBを無効にしてください。", "ARMH must have sRGB disabled."));
            ValidateTextureImport(
                material,
                fShaderPropertyNames.NormalMap,
                true,
                T("ノーマルマップとしてインポートしてください。", "Import this texture as a Normal Map."));

            int samples = 1;
            if (HasAssignedTexture(material, fShaderPropertyNames.ARMHMap)) samples++;
            if (HasAssignedTexture(material, fShaderPropertyNames.NormalMap)) samples++;
            EditorGUILayout.HelpBox(
                string.Format(
                    T("概算テクスチャ参照: {0} + 反射プローブ + ライトマップ（使用時）", "Estimated texture reads: {0} + reflection probe + lightmap (when used)"),
                    samples),
                MessageType.Info);
            EditorGUILayout.Space(3f);
        }

        private static bool DrawFoldout(string key, string label, bool defaultValue)
        {
            bool current = EditorPrefs.GetBool(key, defaultValue);
            bool next = EditorGUILayout.Foldout(current, label, true, EditorStyles.foldoutHeader);
            if (next != current)
            {
                EditorPrefs.SetBool(key, next);
            }
            return next;
        }

        private static void ApplyPreset(
            MaterialEditor editor,
            float roughness,
            float metallic,
            float reflection)
        {
            Material[] materials = editor.targets.OfType<Material>().ToArray();
            Undo.RecordObjects(materials, "Apply fShader PBR Preset");
            foreach (Material material in materials)
            {
                if (material.HasProperty(fShaderPropertyNames.Roughness)) material.SetFloat(fShaderPropertyNames.Roughness, roughness);
                if (material.HasProperty(fShaderPropertyNames.Metallic)) material.SetFloat(fShaderPropertyNames.Metallic, metallic);
                if (material.HasProperty(fShaderPropertyNames.ReflectionStrength)) material.SetFloat(fShaderPropertyNames.ReflectionStrength, reflection);
                EditorUtility.SetDirty(material);
            }
            editor.PropertiesChanged();
        }

        private static void ValidateTextureImport(
            Material material,
            string propertyName,
            bool requireNormalMap,
            string warning)
        {
            Texture texture = material.GetTexture(propertyName);
            string path = texture != null ? AssetDatabase.GetAssetPath(texture) : string.Empty;
            TextureImporter importer = string.IsNullOrEmpty(path)
                ? null
                : AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool invalid = requireNormalMap
                ? importer.textureType != TextureImporterType.NormalMap
                : importer.sRGBTexture;
            if (!invalid)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(warning, MessageType.Warning);
            if (GUILayout.Button(T("修正", "Fix"), GUILayout.Width(52f), GUILayout.Height(38f)))
            {
                Undo.RecordObject(importer, "Fix fShader Texture Import");
                if (requireNormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                }
                else
                {
                    importer.sRGBTexture = false;
                }
                importer.SaveAndReimport();
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void SyncKeywords(Material[] materials)
        {
            foreach (Material material in materials)
            {
                SetKeyword(material, "FSHADER_BASEMAP", HasAssignedTexture(material, fShaderPropertyNames.BaseMap));
                SetKeyword(material, "FSHADER_MASKMAP", HasAssignedTexture(material, fShaderPropertyNames.ARMHMap));
                SetKeyword(material, "FSHADER_NORMALMAP", HasAssignedTexture(material, fShaderPropertyNames.NormalMap));
                SetKeyword(
                    material,
                    "FSHADER_DEBUG",
                    material.HasProperty(fShaderP1PropertyNames.DebugView) &&
                    material.GetFloat(fShaderP1PropertyNames.DebugView) > 0.5f);
                fShaderP2Inspector.SyncKeywords(material);
                fShaderP3Inspector.SyncKeywords(material);
                fShaderP4Inspector.SyncKeywords(material);
            }
        }

        private static bool HasAssignedTexture(Material material, string propertyName)
        {
            if (!material.HasProperty(propertyName))
            {
                return false;
            }
            Texture texture = material.GetTexture(propertyName);
            return texture != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture));
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (material.IsKeywordEnabled(keyword) == enabled)
            {
                return;
            }
            if (enabled) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
            EditorUtility.SetDirty(material);
        }

        private static void SwitchShader(
            MaterialEditor materialEditor,
            fShaderEdition edition,
            fShaderMode mode)
        {
            string shaderName = fShaderShaderCatalog.GetShaderName(edition, mode);
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("fShader", "Shader not found: " + shaderName, "OK");
                return;
            }

            Material[] materials = materialEditor.targets.OfType<Material>().ToArray();
            Undo.RecordObjects(materials, "Change fShader Variant");
            foreach (Material material in materials)
            {
                material.shader = shader;
                material.SetFloat(fShaderPropertyNames.Version, 0.5f);
                material.SetFloat(fShaderPropertyNames.Edition, (float)edition);
                material.SetFloat(fShaderPropertyNames.Mode, (float)mode);
                EditorUtility.SetDirty(material);
            }
            SyncKeywords(materials);
            materialEditor.PropertiesChanged();
        }

        private static string T(string ja, string en)
        {
            return japanese ? ja : en;
        }
    }
}
