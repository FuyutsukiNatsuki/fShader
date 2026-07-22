using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor
{
    internal static class fShaderP2Inspector
    {
        private const string FoldoutKey = "fShader.Inspector.ModeFeatures";
        private const string ScreenGlassShader = "Hidden/fShader/Lite/GlassScreenRefraction";
        private const string StandardGlassShader = "fShader/Lite/Glass";

        public static void Draw(
            MaterialEditor editor,
            MaterialProperty[] properties,
            fShaderEdition edition,
            fShaderMode mode,
            Material material,
            bool japanese)
        {
            if (edition != fShaderEdition.Lite)
            {
                return;
            }

            bool expanded = EditorPrefs.GetBool(FoldoutKey, true);
            bool nextExpanded = EditorGUILayout.Foldout(
                expanded,
                japanese ? "Liteモード機能" : "Lite Mode Features",
                true,
                EditorStyles.foldoutHeader);
            if (nextExpanded != expanded)
            {
                EditorPrefs.SetBool(FoldoutKey, nextExpanded);
            }
            if (!nextExpanded)
            {
                return;
            }

            if (mode == fShaderMode.Water)
            {
                DrawWater(editor, properties, material, japanese);
            }
            else if (mode == fShaderMode.Ice)
            {
                DrawIce(editor, properties, material, japanese);
            }
            else if (mode == fShaderMode.Glass)
            {
                DrawGlass(editor, properties, material, japanese);
            }
            else
            {
                DrawStandard(editor, properties, japanese);
            }

            DrawCostSummary(material, mode, japanese);
            EditorGUILayout.Space(3f);
        }

        public static void SyncKeywords(Material material)
        {
            if (material == null ||
                !material.HasProperty(fShaderPropertyNames.Mode) ||
                !material.HasProperty(fShaderPropertyNames.Edition) ||
                Mathf.RoundToInt(material.GetFloat(fShaderPropertyNames.Edition)) != (int)fShaderEdition.Lite)
            {
                return;
            }

            fShaderMode mode = (fShaderMode)Mathf.RoundToInt(material.GetFloat(fShaderPropertyNames.Mode));
            SetKeyword(material, "FSHADER_VERTEX_COLOR", IsEnabled(material, "_FSVertexColor"));
            if (mode == fShaderMode.Water)
            {
                SetKeyword(material, "FSHADER_WATER_WAVE_NORMAL",
                    IsEnabled(material, "_FSWaterWaveNormal") && HasAssignedTexture(material, fShaderPropertyNames.WaveNormalMap));
                SetKeyword(material, "FSHADER_WATER_VERTEX_WAVES", IsEnabled(material, "_FSWaterVertexWaves"));
                SetKeyword(material, "FSHADER_WATER_FOAM",
                    IsEnabled(material, "_FSWaterFoam") && HasAssignedTexture(material, fShaderPropertyNames.FoamMap));
            }
            else if (mode == fShaderMode.Ice)
            {
                SetKeyword(material, "FSHADER_ICE_FROST",
                    IsEnabled(material, "_FSIceFrost") && HasAssignedTexture(material, fShaderPropertyNames.FrostMap));
                SetKeyword(material, "FSHADER_ICE_CRACKS",
                    IsEnabled(material, "_FSIceCracks") && HasAssignedTexture(material, fShaderPropertyNames.CrackMap));
                SetKeyword(material, "FSHADER_ICE_SCATTER", IsEnabled(material, "_FSIceScatter"));
                SetKeyword(material, "FSHADER_ICE_SPARKLE", IsEnabled(material, "_FSIceSparkle"));
                fShaderIceSurfaceState.Sync(material);
            }
            else if (mode == fShaderMode.Glass)
            {
                bool condensation = IsEnabled(material, "_FSGlassCondensation") &&
                                    HasAssignedTexture(material, fShaderPropertyNames.CondensationMap);
                SetKeyword(material, "FSHADER_GLASS_CONDENSATION", condensation);
                SetKeyword(material, "FSHADER_GLASS_DROPLET_NORMAL",
                    condensation && IsEnabled(material, "_FSGlassDropletNormal") &&
                    HasAssignedTexture(material, fShaderPropertyNames.CondensationNormal));
                SyncGlassScreenShader(material);
            }
        }

        private static void DrawWater(
            MaterialEditor editor,
            MaterialProperty[] properties,
            Material material,
            bool japanese)
        {
            DrawPresetButtons(editor, fShaderMode.Water, japanese, "Water Clear", "Water Ocean");
            DrawProperty(editor, properties, fShaderPropertyNames.ShallowColor, japanese ? "浅瀬カラー" : "Shallow Color");
            DrawProperty(editor, properties, fShaderPropertyNames.DeepColor, japanese ? "深部カラー" : "Deep Color");
            DrawToggleWithTexture(editor, properties, "_FSWaterWaveNormal", fShaderPropertyNames.WaveNormalMap, fShaderPropertyNames.WaveNormalScale, japanese ? "2方向の水面Normal" : "Two-direction Wave Normal");
            DrawProperty(editor, properties, fShaderPropertyNames.WaveSpeedA, japanese ? "波速度A" : "Wave Speed A");
            DrawProperty(editor, properties, fShaderPropertyNames.WaveSpeedB, japanese ? "波速度B" : "Wave Speed B");
            DrawProperty(editor, properties, "_FSWaterVertexWaves", japanese ? "頂点波（最大2波）" : "Vertex Waves (max 2)");
            if (IsEnabled(material, "_FSWaterVertexWaves"))
            {
                DrawProperty(editor, properties, fShaderPropertyNames.WaveAmplitude, japanese ? "波高" : "Wave Amplitude");
                DrawProperty(editor, properties, fShaderPropertyNames.WaveLength, japanese ? "波長" : "Wave Length");
                DrawProperty(editor, properties, fShaderPropertyNames.WaveDirection, japanese ? "波方向" : "Wave Direction");
                MeshFilter selectedMesh = Selection.activeGameObject != null
                    ? Selection.activeGameObject.GetComponent<MeshFilter>()
                    : null;
                if (selectedMesh != null && selectedMesh.sharedMesh != null && selectedMesh.sharedMesh.vertexCount < 64)
                {
                    EditorGUILayout.HelpBox(
                        japanese
                            ? "選択メッシュの頂点数が64未満です。頂点波が角張る可能性があります。"
                            : "The selected mesh has fewer than 64 vertices; vertex waves may look faceted.",
                        MessageType.Warning);
                }
            }
            DrawToggleWithTexture(editor, properties, "_FSWaterFoam", fShaderPropertyNames.FoamMap, fShaderPropertyNames.FoamStrength, japanese ? "軽量フォーム" : "Lite Foam");
            DrawProperty(editor, properties, fShaderPropertyNames.FresnelStrength, "Fresnel");
            DrawProperty(editor, properties, fShaderPropertyNames.RefractionStrength, japanese ? "Probe疑似屈折" : "Probe Distortion");
            DrawProperty(editor, properties, "_FSVertexColor", japanese ? "Vertex Color契約を使用" : "Use Vertex Color Contract");
        }

        private static void DrawIce(
            MaterialEditor editor,
            MaterialProperty[] properties,
            Material material,
            bool japanese)
        {
            DrawPresetButtons(editor, fShaderMode.Ice, japanese, "Ice Clear", "Ice Frosted");
            DrawProperty(editor, properties, fShaderIceSurfaceState.TransparentProperty,
                japanese ? "透過Ice" : "Transparent Ice");
            EditorGUILayout.HelpBox(
                fShaderIceSurfaceState.IsTransparent(material)
                    ? (japanese
                        ? "TransparentはZWrite OFF・半透明キューで描画します。重なった透明面やMirrorでは描画順とOverdrawに注意してください。"
                        : "Transparent uses ZWrite Off and the transparent queue. Watch sorting and overdraw with stacked surfaces and mirrors.")
                    : (japanese
                        ? "Opaqueは1.0.0互換の最軽量設定です。OpacityはTransparent時だけ有効です。"
                        : "Opaque is the lightweight 1.0.0-compatible default. Opacity is used only in Transparent mode."),
                fShaderIceSurfaceState.IsTransparent(material) ? MessageType.Warning : MessageType.Info);
            DrawProperty(editor, properties, fShaderPropertyNames.IceColor, japanese ? "氷カラー" : "Ice Color");
            DrawProperty(editor, properties, fShaderPropertyNames.IceThickness, japanese ? "厚み近似" : "Thickness Approximation");
            DrawToggleWithTexture(editor, properties, "_FSIceFrost", fShaderPropertyNames.FrostMap, fShaderPropertyNames.FrostStrength, "Frost");
            DrawToggleWithTexture(editor, properties, "_FSIceCracks", fShaderPropertyNames.CrackMap, fShaderPropertyNames.CrackDepth, "Cracks");
            DrawProperty(editor, properties, "_FSIceScatter", "Fake Subsurface");
            DrawProperty(editor, properties, fShaderPropertyNames.ScatterColor, japanese ? "散乱カラー" : "Scatter Color");
            DrawProperty(editor, properties, fShaderPropertyNames.ScatterStrength, japanese ? "散乱強度" : "Scatter Strength");
            DrawProperty(editor, properties, "_FSIceSparkle", "Sparkle");
            DrawProperty(editor, properties, fShaderPropertyNames.SparkleStrength, japanese ? "輝点強度" : "Sparkle Strength");
            DrawProperty(editor, properties, "_SparkleDistance", japanese ? "輝点Fade距離" : "Sparkle Fade Distance");
            DrawProperty(editor, properties, "_FSVertexColor", japanese ? "Vertex Color契約を使用" : "Use Vertex Color Contract");
        }

        private static void DrawGlass(
            MaterialEditor editor,
            MaterialProperty[] properties,
            Material material,
            bool japanese)
        {
            DrawPresetButtons(editor, fShaderMode.Glass, japanese, "Glass Clear", "Glass Condensed");
            DrawProperty(editor, properties, fShaderPropertyNames.TransmissionColor, japanese ? "透過カラー" : "Transmission Color");
            DrawProperty(editor, properties, fShaderPropertyNames.GlassThickness, japanese ? "厚み近似" : "Thickness Approximation");
            DrawProperty(editor, properties, fShaderPropertyNames.RefractionStrength, japanese ? "Probe疑似屈折" : "Probe Distortion");
            DrawToggleWithTexture(editor, properties, "_FSGlassCondensation", fShaderPropertyNames.CondensationMap, fShaderPropertyNames.CondensationAmount, japanese ? "結露" : "Condensation");
            DrawToggleWithTexture(editor, properties, "_FSGlassDropletNormal", fShaderPropertyNames.CondensationNormal, null, japanese ? "水滴Normal" : "Droplet Normal");
            DrawProperty(editor, properties, fShaderPropertyNames.DropletSpeed, japanese ? "水滴UV速度" : "Droplet UV Speed");
            DrawProperty(editor, properties, "_FSVertexColor", japanese ? "Vertex Color契約を使用" : "Use Vertex Color Contract");
            EditorGUILayout.HelpBox(
                japanese
                    ? "Screen Refractionは共有GrabPassを使うHeavy機能です。透明面の重なりを避け、必要な材質だけで使用してください。"
                    : "Screen Refraction is a Heavy shared GrabPass feature. Use it only where needed and avoid stacked transparent surfaces.",
                IsEnabled(material, "_FSScreenRefraction") ? MessageType.Warning : MessageType.Info);
            DrawProperty(editor, properties, "_FSScreenRefraction", japanese ? "Screen Refraction（Heavy）" : "Screen Refraction (Heavy)");
        }

        private static void DrawStandard(
            MaterialEditor editor,
            MaterialProperty[] properties,
            bool japanese)
        {
            EditorGUILayout.HelpBox(
                japanese
                    ? "Standardは特殊機能を持たない不透明PBRサーフェスです。透過・波・結露などはありません。色やテクスチャは「基本サーフェス」「PBR入力」で設定してください。"
                    : "Standard is an opaque PBR surface with no special features (no transparency, waves, or condensation). Set colors and textures in the Surface and PBR sections.",
                MessageType.Info);
            DrawProperty(editor, properties, "_FSVertexColor", japanese ? "Vertex Color契約を使用" : "Use Vertex Color Contract");
        }

        private static void DrawPresetButtons(
            MaterialEditor editor,
            fShaderMode mode,
            bool japanese,
            string first,
            string second)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(japanese ? "モードPreset" : "Mode Presets", GUILayout.Width(90f));
            if (GUILayout.Button(first)) ApplyModePreset(editor, mode, false);
            if (GUILayout.Button(second)) ApplyModePreset(editor, mode, true);
            EditorGUILayout.EndHorizontal();
        }

        private static void ApplyModePreset(MaterialEditor editor, fShaderMode mode, bool rich)
        {
            Material[] materials = editor.targets.OfType<Material>().ToArray();
            Undo.RecordObjects(materials, "Apply fShader Lite Mode Preset");
            foreach (Material material in materials)
            {
                if (mode == fShaderMode.Water)
                {
                    SetFloat(material, "_FSWaterWaveNormal", 1f);
                    SetFloat(material, "_FSWaterVertexWaves", rich ? 1f : 0f);
                    SetFloat(material, "_FSWaterFoam", rich ? 1f : 0f);
                    SetFloat(material, fShaderPropertyNames.Roughness, rich ? 0.16f : 0.08f);
                    SetFloat(material, fShaderPropertyNames.WaveAmplitude, rich ? 0.08f : 0.025f);
                }
                else if (mode == fShaderMode.Ice)
                {
                    SetFloat(material, fShaderIceSurfaceState.TransparentProperty, rich ? 0f : 1f);
                    SetFloat(material, fShaderPropertyNames.Opacity, rich ? 1f : 0.42f);
                    SetFloat(material, "_FSIceFrost", rich ? 1f : 0f);
                    SetFloat(material, "_FSIceCracks", rich ? 1f : 0f);
                    SetFloat(material, "_FSIceScatter", 1f);
                    SetFloat(material, "_FSIceSparkle", rich ? 1f : 0f);
                    SetFloat(material, fShaderPropertyNames.Roughness, rich ? 0.48f : 0.12f);
                }
                else
                {
                    SetFloat(material, "_FSGlassCondensation", rich ? 1f : 0f);
                    SetFloat(material, "_FSGlassDropletNormal", rich ? 1f : 0f);
                    SetFloat(material, "_FSScreenRefraction", 0f);
                    SetFloat(material, fShaderPropertyNames.Roughness, rich ? 0.38f : 0.05f);
                    SetFloat(material, fShaderPropertyNames.CondensationAmount, rich ? 0.8f : 0f);
                }
                SyncKeywords(material);
                EditorUtility.SetDirty(material);
            }
            editor.PropertiesChanged();
        }

        private static void DrawCostSummary(Material material, fShaderMode mode, bool japanese)
        {
            int samples = 1;
            if (HasAssignedTexture(material, fShaderPropertyNames.ARMHMap)) samples++;
            if (HasAssignedTexture(material, fShaderPropertyNames.NormalMap)) samples++;
            int vertexWaves = 0;
            bool grab = false;
            if (mode == fShaderMode.Water)
            {
                if (IsEnabled(material, "_FSWaterWaveNormal") && HasAssignedTexture(material, fShaderPropertyNames.WaveNormalMap)) samples += 2;
                if (IsEnabled(material, "_FSWaterFoam") && HasAssignedTexture(material, fShaderPropertyNames.FoamMap)) samples++;
                vertexWaves = IsEnabled(material, "_FSWaterVertexWaves") ? 2 : 0;
            }
            else if (mode == fShaderMode.Ice)
            {
                if (IsEnabled(material, "_FSIceFrost") && HasAssignedTexture(material, fShaderPropertyNames.FrostMap)) samples++;
                if (IsEnabled(material, "_FSIceCracks") && HasAssignedTexture(material, fShaderPropertyNames.CrackMap)) samples++;
            }
            else if (mode == fShaderMode.Glass)
            {
                if (IsEnabled(material, "_FSGlassCondensation") && HasAssignedTexture(material, fShaderPropertyNames.CondensationMap)) samples++;
                if (IsEnabled(material, "_FSGlassDropletNormal") && HasAssignedTexture(material, fShaderPropertyNames.CondensationNormal)) samples++;
                grab = IsEnabled(material, "_FSScreenRefraction");
                if (grab) samples++;
            }
            string rank = grab ? "Heavy" : samples >= 6 ? "Moderate" : "Light";
            EditorGUILayout.HelpBox(
                string.Format(
                    japanese
                        ? "Cost: Base Pass 1 / GrabPass {0} / 2D Sample概算 {1} / Cubemap 1 / Vertex Wave {2} / Rank {3}"
                        : "Cost: Base Pass 1 / GrabPass {0} / Estimated 2D Samples {1} / Cubemap 1 / Vertex Waves {2} / Rank {3}",
                    grab ? "ON" : "OFF",
                    samples,
                    vertexWaves,
                    rank),
                grab ? MessageType.Warning : MessageType.Info);
        }

        private static void DrawToggleWithTexture(
            MaterialEditor editor,
            MaterialProperty[] properties,
            string toggleName,
            string textureName,
            string extraName,
            string label)
        {
            DrawProperty(editor, properties, toggleName, label);
            MaterialProperty toggle = Find(properties, toggleName);
            using (new EditorGUI.DisabledScope(toggle == null || toggle.floatValue < 0.5f))
            {
                MaterialProperty texture = Find(properties, textureName);
                MaterialProperty extra = string.IsNullOrEmpty(extraName) ? null : Find(properties, extraName);
                if (texture != null)
                {
                    editor.TexturePropertySingleLine(new GUIContent(label + " Map"), texture, extra);
                }
            }
        }

        private static void DrawProperty(
            MaterialEditor editor,
            MaterialProperty[] properties,
            string propertyName,
            string label)
        {
            MaterialProperty property = Find(properties, propertyName);
            if (property != null)
            {
                editor.ShaderProperty(property, label);
            }
        }

        private static MaterialProperty Find(MaterialProperty[] properties, string name)
        {
            return properties.FirstOrDefault(property => property.name == name);
        }

        private static bool IsEnabled(Material material, string propertyName)
        {
            return material.HasProperty(propertyName) && material.GetFloat(propertyName) > 0.5f;
        }

        private static bool HasAssignedTexture(Material material, string propertyName)
        {
            if (!material.HasProperty(propertyName)) return false;
            Texture texture = material.GetTexture(propertyName);
            return texture != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture));
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName)) material.SetFloat(propertyName, value);
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
        }

        private static void SyncGlassScreenShader(Material material)
        {
            bool enabled = IsEnabled(material, "_FSScreenRefraction");
            bool isScreenShader = material.shader != null && material.shader.name == ScreenGlassShader;
            string targetName = enabled ? ScreenGlassShader : StandardGlassShader;
            if (enabled == isScreenShader)
            {
                return;
            }

            Shader target = Shader.Find(targetName);
            if (target != null)
            {
                material.shader = target;
                material.SetFloat("_FSScreenRefraction", enabled ? 1f : 0f);
                EditorUtility.SetDirty(material);
            }
        }
    }
}
