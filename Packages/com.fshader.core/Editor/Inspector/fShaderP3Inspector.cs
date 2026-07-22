using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor
{
    internal static class fShaderP3Inspector
    {
        private const string FoldoutKey = "fShader.Inspector.PlusModeFeatures";
        private const string PlusWaterShader = "fShader/Plus/Water";
        private const string PlusIceShader = "fShader/Plus/Ice";
        private const string PlusGlassShader = "fShader/Plus/Glass";
        private const string ScreenWaterShader = "Hidden/fShader/Plus/WaterScreenRefraction";
        private const string ScreenIceShader = "Hidden/fShader/Plus/IceScreenRefraction";
        private const string ScreenGlassShader = "Hidden/fShader/Plus/GlassScreenRefraction";

        public static void Draw(
            MaterialEditor editor,
            MaterialProperty[] properties,
            fShaderEdition edition,
            fShaderMode mode,
            Material material,
            bool japanese)
        {
            if (edition != fShaderEdition.Plus)
            {
                return;
            }

            bool expanded = EditorPrefs.GetBool(FoldoutKey, true);
            bool nextExpanded = EditorGUILayout.Foldout(
                expanded,
                japanese ? "Plusモード機能" : "Plus Mode Features",
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

            if (mode != fShaderMode.Standard)
            {
                DrawPresetButtons(editor, mode, japanese);
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
                !material.HasProperty(fShaderPropertyNames.Edition) ||
                Mathf.RoundToInt(material.GetFloat(fShaderPropertyNames.Edition)) != (int)fShaderEdition.Plus)
            {
                return;
            }

            fShaderMode mode = (fShaderMode)Mathf.RoundToInt(material.GetFloat(fShaderPropertyNames.Mode));
            SetKeyword(material, "FSHADER_VERTEX_COLOR", IsEnabled(material, "_FSVertexColor"));
            SetKeyword(material, "FSHADER_BOX_PROJECTION", IsEnabled(material, "_FSBoxProjection"));

            if (mode == fShaderMode.Water)
            {
                SetKeyword(material, "FSHADER_WATER_WAVE_NORMAL",
                    IsEnabled(material, "_FSWaterWaveNormal") &&
                    HasAssignedTexture(material, "_WaveNormalMap") &&
                    HasAssignedTexture(material, "_WaveNormalMap2"));
                SetKeyword(material, "FSHADER_WATER_VERTEX_WAVES", IsEnabled(material, "_FSWaterVertexWaves"));
                SetKeyword(material, "FSHADER_WATER_FOAM",
                    IsEnabled(material, "_FSWaterFoam") && HasAssignedTexture(material, "_FoamMap"));
                SetKeyword(material, "FSHADER_WATER_CAUSTICS",
                    IsEnabled(material, "_FSWaterCaustics") && HasAssignedTexture(material, "_CausticsMap"));
                SyncScreenShader(material, fShaderMode.Water);
            }
            else if (mode == fShaderMode.Ice)
            {
                SetKeyword(material, "FSHADER_ICE_FROST",
                    IsEnabled(material, "_FSIceFrost") && HasAssignedTexture(material, "_FrostMap"));
                SetKeyword(material, "FSHADER_ICE_CRACKS",
                    IsEnabled(material, "_FSIceCracks") && HasAssignedTexture(material, "_CrackMap"));
                SetKeyword(material, "FSHADER_ICE_BACKLIGHT", IsEnabled(material, "_FSIceBackLight"));
                SetKeyword(material, "FSHADER_ICE_SPARKLE", IsEnabled(material, "_FSIceSparkle"));
                fShaderIceSurfaceState.Sync(material);
                SyncScreenShader(material, fShaderMode.Ice);
            }
            else if (mode == fShaderMode.Glass)
            {
                bool condensation = IsEnabled(material, "_FSGlassCondensation") &&
                                    HasAssignedTexture(material, "_CondensationMap");
                SetKeyword(material, "FSHADER_GLASS_CONDENSATION", condensation);
                SetKeyword(material, "FSHADER_GLASS_DROPLET_NORMAL",
                    condensation && IsEnabled(material, "_FSGlassDropletNormal") &&
                    HasAssignedTexture(material, "_CondensationNormal"));
                SyncScreenShader(material, fShaderMode.Glass);
            }
            else
            {
                // Standard: opaque PBR only, no mode-specific keywords or screen shader.
                fShaderIceSurfaceState.Sync(material);
            }
        }

        private static void DrawPresetButtons(MaterialEditor editor, fShaderMode mode, bool japanese)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(japanese ? "Plus Preset" : "Plus Preset", GUILayout.Width(90f));
            if (GUILayout.Button("Balanced")) ApplyPreset(editor, mode, false);
            if (GUILayout.Button(japanese ? "Showcase（Heavy）" : "Showcase (Heavy)")) ApplyPreset(editor, mode, true);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawWater(MaterialEditor editor, MaterialProperty[] properties, Material material, bool japanese)
        {
            Header(japanese ? "色・吸収" : "Color & Absorption");
            DrawProperty(editor, properties, "_ShallowColor", japanese ? "浅瀬カラー" : "Shallow Color");
            DrawProperty(editor, properties, "_DeepColor", japanese ? "深部カラー" : "Deep Color");
            DrawProperty(editor, properties, "_AbsorptionColor", japanese ? "吸収カラー" : "Absorption Color");
            DrawProperty(editor, properties, "_AbsorptionStrength", japanese ? "吸収強度" : "Absorption Strength");
            DrawProperty(editor, properties, "_WaterThickness", japanese ? "水の厚み" : "Water Thickness");
            DrawProperty(editor, properties, "_DepthStrength", japanese ? "Height/Vertex深度強度" : "Height/Vertex Depth");
            DrawProperty(editor, properties, "_DepthBias", japanese ? "深度バイアス" : "Depth Bias");

            Header(japanese ? "水面ディテール" : "Surface Detail");
            DrawProperty(editor, properties, "_FSWaterWaveNormal", japanese ? "2レイヤーNormal" : "Dual-layer Normal");
            using (new EditorGUI.DisabledScope(!IsEnabled(material, "_FSWaterWaveNormal")))
            {
                DrawTexture(editor, properties, "_WaveNormalMap", "_WaveNormalScale", "Wave Normal A");
                DrawTexture(editor, properties, "_WaveNormalMap2", "_WaveNormalScale2", "Wave Normal B");
                DrawProperty(editor, properties, "_WaveScaleA", "Scale A");
                DrawProperty(editor, properties, "_WaveScaleB", "Scale B");
                DrawProperty(editor, properties, "_WaveSpeedA", "Speed A");
                DrawProperty(editor, properties, "_WaveSpeedB", "Speed B");
            }

            DrawProperty(editor, properties, "_FSWaterVertexWaves", japanese ? "World Space頂点波" : "World-space Vertex Waves");
            using (new EditorGUI.DisabledScope(!IsEnabled(material, "_FSWaterVertexWaves")))
            {
                DrawProperty(editor, properties, "_WaveCount", japanese ? "波数（最大4）" : "Wave Count (max 4)");
                DrawProperty(editor, properties, "_WaveAmplitude", japanese ? "波高" : "Amplitude");
                DrawProperty(editor, properties, "_WaveLength", japanese ? "波長" : "Length");
                DrawProperty(editor, properties, "_WaveDirection", japanese ? "方向" : "Direction");
                DrawProperty(editor, properties, "_WaveTimeScale", japanese ? "時間倍率" : "Time Scale");
                WarnForCoarseWaterMesh(japanese);
            }

            DrawProperty(editor, properties, "_FSWaterFoam", japanese ? "マルチスケール泡" : "Multi-scale Foam");
            using (new EditorGUI.DisabledScope(!IsEnabled(material, "_FSWaterFoam")))
            {
                DrawTexture(editor, properties, "_FoamMap", "_FoamStrength", japanese ? "泡" : "Foam");
                DrawProperty(editor, properties, "_FoamColor", japanese ? "泡カラー" : "Foam Color");
                DrawProperty(editor, properties, "_FoamDetailScale", japanese ? "細部スケール" : "Detail Scale");
                DrawProperty(editor, properties, "_FoamCrestStrength", japanese ? "波頂強調" : "Crest Strength");
            }

            DrawProperty(editor, properties, "_FSWaterCaustics", japanese ? "表面コースティクス" : "Surface Caustics");
            using (new EditorGUI.DisabledScope(!IsEnabled(material, "_FSWaterCaustics")))
            {
                DrawTexture(editor, properties, "_CausticsMap", "_CausticsStrength", "Caustics");
                DrawProperty(editor, properties, "_CausticsColor", japanese ? "コースティクス色" : "Caustics Color");
            }
            DrawReflectionAndScreen(editor, properties, material, japanese);
        }

        private static void DrawIce(MaterialEditor editor, MaterialProperty[] properties, Material material, bool japanese)
        {
            Header(japanese ? "描画モード" : "Rendering Mode");
            DrawProperty(editor, properties, fShaderIceSurfaceState.TransparentProperty,
                japanese ? "透過Ice" : "Transparent Ice");
            bool transparent = fShaderIceSurfaceState.IsTransparent(material);
            EditorGUILayout.HelpBox(
                transparent
                    ? (japanese
                        ? "厚み・Frost・Crackが不透明度へ反映されます。透明面の重なり、Mirror、Overdrawに注意してください。"
                        : "Thickness, Frost, and Cracks contribute to opacity. Watch sorting, mirrors, and overdraw.")
                    : (japanese
                        ? "Opaqueは1.0.0互換の最軽量設定です。OpacityはTransparent時だけ有効です。"
                        : "Opaque is the lightweight 1.0.0-compatible default. Opacity is used only in Transparent mode."),
                transparent ? MessageType.Warning : MessageType.Info);

            Header(japanese ? "氷・吸収" : "Ice & Absorption");
            DrawProperty(editor, properties, "_IceColor", japanese ? "氷カラー" : "Ice Color");
            DrawProperty(editor, properties, "_AbsorptionColor", japanese ? "吸収カラー" : "Absorption Color");
            DrawProperty(editor, properties, "_IceThickness", japanese ? "厚み" : "Thickness");
            DrawProperty(editor, properties, "_AbsorptionStrength", japanese ? "視線角吸収" : "View-angle Absorption");

            Header("Frost");
            DrawProperty(editor, properties, "_FSIceFrost", japanese ? "2スケールFrost" : "Two-scale Frost");
            DrawTexture(editor, properties, "_FrostMap", "_FrostStrength", "Frost");
            DrawProperty(editor, properties, "_FrostColor", japanese ? "霜カラー" : "Frost Color");
            DrawProperty(editor, properties, "_FrostScaleA", "Scale A");
            DrawProperty(editor, properties, "_FrostScaleB", "Scale B");
            DrawProperty(editor, properties, "_FrostEdge", japanese ? "縁の霜" : "Edge Frost");

            Header(japanese ? "クラック・散乱" : "Cracks & Scattering");
            DrawProperty(editor, properties, "_FSIceCracks", japanese ? "簡易視差クラック" : "Parallax Cracks");
            DrawTexture(editor, properties, "_CrackMap", "_CrackDepth", "Cracks");
            DrawProperty(editor, properties, "_CrackParallax", japanese ? "視差量" : "Parallax");
            DrawProperty(editor, properties, "_CrackGlowColor", japanese ? "内部発光カラー" : "Internal Color");
            DrawProperty(editor, properties, "_CrackGlowStrength", japanese ? "内部発光風強度" : "Internal Glow");
            DrawProperty(editor, properties, "_FSIceBackLight", "Back Light");
            DrawProperty(editor, properties, "_BackLightColor", japanese ? "背面散乱カラー" : "Back Light Color");
            DrawProperty(editor, properties, "_BackLightStrength", japanese ? "背面散乱強度" : "Back Light Strength");
            DrawProperty(editor, properties, "_BackLightThickness", japanese ? "散乱厚み" : "Scatter Thickness");

            Header("Sparkle");
            DrawProperty(editor, properties, "_FSIceSparkle", "Sparkle");
            DrawProperty(editor, properties, "_SparkleStrength", japanese ? "強度" : "Strength");
            DrawProperty(editor, properties, "_SparkleDensity", japanese ? "密度" : "Density");
            DrawProperty(editor, properties, "_SparkleSize", japanese ? "サイズ" : "Size");
            DrawProperty(editor, properties, "_SparkleDistance", japanese ? "距離Fade" : "Distance Fade");
            DrawProperty(editor, properties, "_FSBoxProjection", "Box Projected Probe");
            DrawProperty(editor, properties, "_FSVertexColor", japanese ? "Vertex Color契約" : "Vertex Color Contract");

            Header(japanese ? "屈折" : "Refraction");
            DrawProperty(editor, properties, "_RefractionStrength", japanese ? "Screen歪み" : "Screen Distortion");
            using (new EditorGUI.DisabledScope(!transparent))
            {
                DrawProperty(editor, properties, "_FSScreenRefraction", japanese ? "Screen Refraction（Heavy）" : "Screen Refraction (Heavy)");
            }
            EditorGUILayout.HelpBox(
                japanese
                    ? "Screen RefractionはTransparent Ice専用の共有GrabPass機能です。通常はOFFを推奨します。"
                    : "Screen Refraction is a shared GrabPass feature for Transparent Ice. Keep it OFF for normal use.",
                IsEnabled(material, "_FSScreenRefraction") ? MessageType.Warning : MessageType.Info);
        }

        private static void DrawGlass(MaterialEditor editor, MaterialProperty[] properties, Material material, bool japanese)
        {
            Header(japanese ? "透過・吸収" : "Transmission & Absorption");
            DrawProperty(editor, properties, "_TransmissionColor", japanese ? "透過カラー" : "Transmission Color");
            DrawProperty(editor, properties, "_AbsorptionColor", japanese ? "吸収カラー" : "Absorption Color");
            DrawProperty(editor, properties, "_GlassThickness", japanese ? "Height/Vertex厚み" : "Height/Vertex Thickness");
            DrawProperty(editor, properties, "_AbsorptionStrength", japanese ? "吸収強度" : "Absorption Strength");

            Header(japanese ? "パック結露（R:水滴 G:筋 B:微細曇り）" : "Packed Condensation (R:Droplet G:Trail B:Micro Fog)");
            DrawProperty(editor, properties, "_FSGlassCondensation", japanese ? "結露" : "Condensation");
            DrawTexture(editor, properties, "_CondensationMap", "_CondensationAmount", japanese ? "結露RGB" : "Condensation RGB");
            DrawProperty(editor, properties, "_CondensationColor", japanese ? "結露カラー" : "Condensation Color");
            DrawProperty(editor, properties, "_DropletStrength", japanese ? "水滴 R" : "Droplet R");
            DrawProperty(editor, properties, "_TrailStrength", japanese ? "縦筋 G" : "Trail G");
            DrawProperty(editor, properties, "_MicroFogStrength", japanese ? "微細曇り B" : "Micro Fog B");
            DrawProperty(editor, properties, "_CondensationRoughness", japanese ? "局所ラフネス" : "Local Roughness");
            DrawProperty(editor, properties, "_CondensationOpacity", japanese ? "局所不透明度" : "Local Opacity");
            DrawProperty(editor, properties, "_CondensationFadeDistance", japanese ? "筋の距離Fade" : "Trail Distance Fade");
            DrawProperty(editor, properties, "_DropletSpeed", japanese ? "水滴/筋速度" : "Droplet/Trail Speed");
            DrawProperty(editor, properties, "_FSGlassDropletNormal", japanese ? "結露Normal" : "Condensation Normal");
            DrawTexture(editor, properties, "_CondensationNormal", "_CondensationNormalScale", japanese ? "結露Normal" : "Condensation Normal");
            DrawReflectionAndScreen(editor, properties, material, japanese);
        }

        private static void DrawStandard(MaterialEditor editor, MaterialProperty[] properties, bool japanese)
        {
            EditorGUILayout.HelpBox(
                japanese
                    ? "Standardは特殊機能を持たない不透明PBRサーフェスです。透過・波・結露・Screen Refractionはありません。色やテクスチャは「基本サーフェス」「PBR入力」で設定してください。"
                    : "Standard is an opaque PBR surface with no special features (no transparency, waves, condensation, or screen refraction). Set colors and textures in the Surface and PBR sections.",
                MessageType.Info);
            Header(japanese ? "反射" : "Reflection");
            DrawProperty(editor, properties, "_FSBoxProjection", "Box Projected Probe");
            DrawProperty(editor, properties, "_FSVertexColor", japanese ? "Vertex Color契約" : "Vertex Color Contract");
        }

        private static void DrawReflectionAndScreen(MaterialEditor editor, MaterialProperty[] properties, Material material, bool japanese)
        {
            Header(japanese ? "反射・屈折" : "Reflection & Refraction");
            DrawProperty(editor, properties, "_FresnelStrength", "Fresnel");
            DrawProperty(editor, properties, "_RefractionStrength", japanese ? "Probe/Screen歪み" : "Probe/Screen Distortion");
            DrawProperty(editor, properties, "_FSBoxProjection", "Box Projected Probe");
            DrawProperty(editor, properties, "_FSVertexColor", japanese ? "Vertex Color契約" : "Vertex Color Contract");

            bool screen = IsEnabled(material, "_FSScreenRefraction");
            EditorGUILayout.HelpBox(
                japanese
                    ? "Screen Refractionは共有GrabPassを使うHeavy機能です。通常はOFFを推奨します。"
                    : "Screen Refraction uses a shared GrabPass and is Heavy. Keep it OFF for normal use.",
                screen ? MessageType.Warning : MessageType.Info);
            DrawProperty(editor, properties, "_FSScreenRefraction", japanese ? "Screen Refraction（Heavy）" : "Screen Refraction (Heavy)");
        }

        private static void DrawCostSummary(Material material, fShaderMode mode, bool japanese)
        {
            int samples = 0;
            if (HasAssignedTexture(material, "_BaseMap")) samples++;
            if (HasAssignedTexture(material, "_ARMHMap")) samples++;
            if (HasAssignedTexture(material, "_NormalMap")) samples++;
            int waves = 0;
            bool screen = IsEnabled(material, "_FSScreenRefraction");
            bool ltcgi = IsEnabled(material, "_LTCGI");
            if (mode == fShaderMode.Water)
            {
                if (IsEnabled(material, "_FSWaterWaveNormal") && HasAssignedTexture(material, "_WaveNormalMap") && HasAssignedTexture(material, "_WaveNormalMap2")) samples += 2;
                if (IsEnabled(material, "_FSWaterFoam") && HasAssignedTexture(material, "_FoamMap")) samples += 2;
                if (IsEnabled(material, "_FSWaterCaustics") && HasAssignedTexture(material, "_CausticsMap")) samples++;
                waves = IsEnabled(material, "_FSWaterVertexWaves") ? Mathf.Clamp(Mathf.RoundToInt(GetFloat(material, "_WaveCount", 1f)), 1, 4) : 0;
            }
            else if (mode == fShaderMode.Ice)
            {
                if (IsEnabled(material, "_FSIceFrost") && HasAssignedTexture(material, "_FrostMap")) samples += 2;
                if (IsEnabled(material, "_FSIceCracks") && HasAssignedTexture(material, "_CrackMap")) samples++;
            }
            else if (mode == fShaderMode.Glass)
            {
                if (IsEnabled(material, "_FSGlassCondensation") && HasAssignedTexture(material, "_CondensationMap")) samples += 2;
                if (IsEnabled(material, "_FSGlassDropletNormal") && HasAssignedTexture(material, "_CondensationNormal")) samples++;
            }
            if (screen) samples++;
            bool combinationWarning = mode == fShaderMode.Water && ((screen && waves >= 4 && IsEnabled(material, "_FSWaterWaveNormal")) || (ltcgi && (screen || waves >= 4)));
            string rank = (screen && ltcgi) || (ltcgi && waves >= 4) ? "Heavy" : screen || ltcgi || samples >= 8 ? "Moderate" : "Light";
            EditorGUILayout.HelpBox(
                string.Format(
                    japanese
                        ? "Cost: 通常Draw 1 / GrabPass {0} / 2D Sample概算 {1} / Cubemap 1 / Vertex Wave {2} / Rank {3}"
                        : "Cost: Normal Draw 1 / GrabPass {0} / Estimated 2D Samples {1} / Cubemap 1 / Vertex Waves {2} / Rank {3}",
                    screen ? "ON" : "OFF", samples, waves, rank),
                screen ? MessageType.Warning : MessageType.Info);
            if (combinationWarning)
            {
                EditorGUILayout.HelpBox(
                    japanese
                        ? "4頂点波 + 2 Normal + Screen Refractionの同時使用はShowcase向けです。鏡や透明面が多い場所では減らしてください。"
                        : "4 vertex waves + dual normals + Screen Refraction is a showcase configuration. Reduce it around mirrors or layered transparency.",
                    MessageType.Warning);
            }
        }

        private static void ApplyPreset(MaterialEditor editor, fShaderMode mode, bool showcase)
        {
            Material[] materials = editor.targets.OfType<Material>().ToArray();
            Undo.RecordObjects(materials, "Apply fShader Plus Preset");
            foreach (Material material in materials)
            {
                SetFloat(material, "_FSBoxProjection", 1f);
                SetFloat(material, "_LTCGI", 0f);
                SetFloat(material, "_FSVertexColor", 0f);
                if (mode == fShaderMode.Water)
                {
                    CopyTextureIfEmpty(material, "_WaveNormalMap", "_WaveNormalMap2");
                    SetFloat(material, "_FSWaterWaveNormal", 1f);
                    SetFloat(material, "_FSWaterVertexWaves", showcase ? 1f : 0f);
                    SetFloat(material, "_WaveCount", showcase ? 4f : 2f);
                    SetFloat(material, "_FSWaterFoam", showcase ? 1f : 0f);
                    SetFloat(material, "_FSWaterCaustics", showcase ? 1f : 0f);
                    SetFloat(material, "_FSScreenRefraction", showcase ? 1f : 0f);
                    SetFloat(material, "_Roughness", showcase ? 0.16f : 0.1f);
                }
                else if (mode == fShaderMode.Ice)
                {
                    SetFloat(material, fShaderIceSurfaceState.TransparentProperty, showcase ? 1f : 0f);
                    SetFloat(material, fShaderPropertyNames.Opacity, showcase ? 0.34f : 1f);
                    SetFloat(material, "_FSScreenRefraction", showcase ? 1f : 0f);
                    SetFloat(material, "_FSIceFrost", showcase ? 1f : 0f);
                    SetFloat(material, "_FSIceCracks", showcase ? 1f : 0f);
                    SetFloat(material, "_FSIceBackLight", 1f);
                    SetFloat(material, "_FSIceSparkle", showcase ? 1f : 0f);
                    SetFloat(material, "_Roughness", showcase ? 0.34f : 0.16f);
                }
                else
                {
                    SetFloat(material, "_FSGlassCondensation", showcase ? 1f : 0f);
                    SetFloat(material, "_FSGlassDropletNormal", showcase ? 1f : 0f);
                    SetFloat(material, "_FSScreenRefraction", showcase ? 1f : 0f);
                    SetFloat(material, "_CondensationAmount", showcase ? 0.85f : 0f);
                    SetFloat(material, "_Roughness", showcase ? 0.12f : 0.05f);
                }
                SyncKeywords(material);
                EditorUtility.SetDirty(material);
            }
            editor.PropertiesChanged();
        }

        private static void WarnForCoarseWaterMesh(bool japanese)
        {
            MeshFilter filter = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<MeshFilter>() : null;
            if (filter != null && filter.sharedMesh != null && filter.sharedMesh.vertexCount < 128)
            {
                EditorGUILayout.HelpBox(
                    japanese
                        ? "選択メッシュの頂点数が128未満です。Plus頂点波が角張る可能性があります。"
                        : "The selected mesh has fewer than 128 vertices; Plus vertex waves may look faceted.",
                    MessageType.Warning);
            }
        }

        private static void SyncScreenShader(Material material, fShaderMode mode)
        {
            bool enabled = IsEnabled(material, "_FSScreenRefraction");
            if (mode == fShaderMode.Ice && !fShaderIceSurfaceState.IsTransparent(material))
            {
                enabled = false;
                SetFloat(material, "_FSScreenRefraction", 0f);
            }
            string standard = mode == fShaderMode.Water
                ? PlusWaterShader
                : mode == fShaderMode.Ice ? PlusIceShader : PlusGlassShader;
            string screen = mode == fShaderMode.Water
                ? ScreenWaterShader
                : mode == fShaderMode.Ice ? ScreenIceShader : ScreenGlassShader;
            string targetName = enabled ? screen : standard;
            if (material.shader != null && material.shader.name == targetName)
            {
                return;
            }
            Shader target = Shader.Find(targetName);
            if (target != null)
            {
                material.shader = target;
                material.SetFloat("_FSScreenRefraction", enabled ? 1f : 0f);
                material.SetFloat(fShaderPropertyNames.FeatureFlags, enabled ? 1f : 0f);
                EditorUtility.SetDirty(material);
            }
        }

        private static void Header(string label)
        {
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private static void DrawTexture(MaterialEditor editor, MaterialProperty[] properties, string textureName, string extraName, string label)
        {
            MaterialProperty texture = Find(properties, textureName);
            MaterialProperty extra = Find(properties, extraName);
            if (texture != null)
            {
                editor.TexturePropertySingleLine(new GUIContent(label), texture, extra);
                editor.TextureScaleOffsetProperty(texture);
            }
        }

        private static void DrawProperty(MaterialEditor editor, MaterialProperty[] properties, string propertyName, string label)
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

        private static float GetFloat(Material material, string propertyName, float fallback)
        {
            return material.HasProperty(propertyName) ? material.GetFloat(propertyName) : fallback;
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName)) material.SetFloat(propertyName, value);
        }

        private static void CopyTextureIfEmpty(Material material, string sourceName, string targetName)
        {
            if (!material.HasProperty(sourceName) || !material.HasProperty(targetName) || HasAssignedTexture(material, targetName))
            {
                return;
            }
            material.SetTexture(targetName, material.GetTexture(sourceName));
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
        }
    }
}
