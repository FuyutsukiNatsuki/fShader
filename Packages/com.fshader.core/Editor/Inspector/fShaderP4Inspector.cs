using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor
{
    internal static class fShaderP4Inspector
    {
        private const string FoldoutKey = "fShader.Inspector.LTCGI";
        private const string MinimumVersion = "1.6.3";
        private const string MaximumExclusiveVersion = "1.7.0";
        private const string PackagePath = "Packages/at.pimaker.ltcgi/package.json";

        public static void Draw(
            MaterialEditor editor,
            MaterialProperty[] properties,
            fShaderEdition edition,
            fShaderMode mode,
            Material material,
            bool japanese)
        {
            if (edition != fShaderEdition.Plus || mode == fShaderMode.Ice || !material.HasProperty(fShaderPropertyNames.LTCGI))
            {
                return;
            }

            bool expanded = EditorPrefs.GetBool(FoldoutKey, true);
            bool next = EditorGUILayout.Foldout(
                expanded,
                japanese ? "ライティング / LTCGI" : "Lighting / LTCGI",
                true,
                EditorStyles.foldoutHeader);
            if (next != expanded) EditorPrefs.SetBool(FoldoutKey, next);
            if (!next) return;

            UnityEditor.PackageManager.PackageInfo package =
                UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);
            bool installed = package != null;
            bool supported = installed && IsSupportedVersion(package.version);
            Object controller = FindSceneController();

            if (!installed)
            {
                EditorGUILayout.HelpBox(
                    japanese
                        ? "LTCGIパッケージが見つかりません。Package Managerの解決完了後に再確認してください。"
                        : "The LTCGI package was not found. Wait for Package Manager resolution and check again.",
                    MessageType.Error);
            }
            else if (!supported)
            {
                EditorGUILayout.HelpBox(
                    string.Format(
                        japanese ? "LTCGI {0} を検出しました。対応範囲は{1}以上{2}未満です。" : "LTCGI {0} detected; supported versions are {1} or newer and below {2}.",
                        package.version,
                        MinimumVersion,
                        MaximumExclusiveVersion),
                    MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField("LTCGI Package", package.version + " / API v2", EditorStyles.miniLabel);
            }

            DrawProperty(editor, properties, fShaderPropertyNames.LTCGI, "LTCGI");
            bool enabled = IsEnabled(material, fShaderPropertyNames.LTCGI);
            using (new EditorGUI.DisabledScope(!enabled))
            {
                DrawProperty(editor, properties, fShaderPropertyNames.LTCGIDiffuseStrength,
                    japanese ? "Diffuse 強度" : "Diffuse Strength");
                DrawProperty(editor, properties, fShaderPropertyNames.LTCGISpecularStrength,
                    japanese ? "Specular 強度" : "Specular Strength");
                if (mode == fShaderMode.Glass)
                {
                    DrawProperty(editor, properties, "_LTCGICondensationDiffuse",
                        japanese ? "結露 Diffuse ブースト" : "Condensation Diffuse Boost");
                }
                DrawProperty(editor, properties, fShaderPropertyNames.LTCGIMaxBrightness,
                    japanese ? "最大輝度" : "Max Brightness");
            }

            if (!enabled)
            {
                EditorGUILayout.HelpBox(
                    japanese
                        ? "OFFバリアントではLTCGIのincludeと計算を実行しません。"
                        : "The OFF variant does not include or execute LTCGI calculations.",
                    MessageType.Info);
            }
            else
            {
                if (controller == null)
                {
                    EditorGUILayout.HelpBox(
                        japanese
                            ? "シーン内にLTCGI_Controllerが見つかりません。公式LTCGI prefabを配置し、Affected Renderersを更新してください。"
                            : "No LTCGI_Controller was found in the scene. Add the official LTCGI prefab and update Affected Renderers.",
                        MessageType.Warning);
                }
                else if (GUILayout.Button(japanese ? "LTCGI Controllerを選択" : "Select LTCGI Controller"))
                {
                    Selection.activeObject = controller;
                    EditorGUIUtility.PingObject(controller);
                }

                bool screen = IsEnabled(material, "_FSScreenRefraction");
                int waves = material.HasProperty("_WaveCount") ? Mathf.RoundToInt(material.GetFloat("_WaveCount")) : 0;
                bool fourWaves = mode == fShaderMode.Water && IsEnabled(material, "_FSWaterVertexWaves") && waves >= 4;
                if (screen || fourWaves)
                {
                    EditorGUILayout.HelpBox(
                        japanese
                            ? "LTCGIとScreen Refraction／4波を同時使用しています。透明面の重なりやMirror周辺ではHeavy構成です。"
                            : "LTCGI is combined with Screen Refraction or four waves. This is a Heavy setup around layered transparency and mirrors.",
                        MessageType.Warning);
                }
            }

            if (material.GetTag("LTCGI", false, string.Empty) != "_LTCGI")
            {
                EditorGUILayout.HelpBox("Missing shader tag: LTCGI=\"_LTCGI\".", MessageType.Error);
            }
            EditorGUILayout.Space(3f);
        }

        public static void SyncKeywords(Material material)
        {
            if (material == null || !material.HasProperty(fShaderPropertyNames.LTCGI)) return;
            bool plus = material.HasProperty(fShaderPropertyNames.Edition) &&
                        Mathf.RoundToInt(material.GetFloat(fShaderPropertyNames.Edition)) == (int)fShaderEdition.Plus;
            bool supportedMode = material.HasProperty(fShaderPropertyNames.Mode) &&
                                 Mathf.RoundToInt(material.GetFloat(fShaderPropertyNames.Mode)) != (int)fShaderMode.Ice;
            SetKeyword(material, "FSHADER_LTCGI", plus && supportedMode && IsEnabled(material, fShaderPropertyNames.LTCGI));
        }

        private static Object FindSceneController()
        {
            return Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .FirstOrDefault(component =>
                    component != null &&
                    component.GetType().Name == "LTCGI_Controller" &&
                    !EditorUtility.IsPersistent(component) &&
                    component.gameObject.scene.IsValid());
        }

        private static bool IsSupportedVersion(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string numeric = value.Split('-')[0];
            return System.Version.TryParse(numeric, out System.Version found) &&
                   System.Version.TryParse(MinimumVersion, out System.Version minimum) &&
                   System.Version.TryParse(MaximumExclusiveVersion, out System.Version maximum) &&
                   found.CompareTo(minimum) >= 0 &&
                   found.CompareTo(maximum) < 0;
        }

        private static void DrawProperty(MaterialEditor editor, MaterialProperty[] properties, string name, string label)
        {
            MaterialProperty property = properties.FirstOrDefault(candidate => candidate.name == name);
            if (property != null) editor.ShaderProperty(property, label);
        }

        private static bool IsEnabled(Material material, string propertyName)
        {
            return material.HasProperty(propertyName) && material.GetFloat(propertyName) > 0.5f;
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (material.IsKeywordEnabled(keyword) == enabled) return;
            if (enabled) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
            EditorUtility.SetDirty(material);
        }
    }
}
