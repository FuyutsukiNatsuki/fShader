using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace fShader.Editor
{
    /// <summary>
    /// Removes only fShader variants that cannot produce a valid shipping result.
    /// Stereo, fog, lightmap, instancing, mode and LTCGI variants are deliberately preserved.
    /// </summary>
    public sealed class fShaderShaderVariantStripper : IPreprocessShaders
    {
        internal const string DebugKeyword = "FSHADER_DEBUG";
        internal const string CondensationKeyword = "FSHADER_GLASS_CONDENSATION";
        internal const string DropletNormalKeyword = "FSHADER_GLASS_DROPLET_NORMAL";

        public int callbackOrder => 900;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            if (shader == null || !IsFShader(shader.name) || data == null)
            {
                return;
            }

            int inputCount = data.Count;
            int removedDebug = 0;
            int removedDependency = 0;
            LocalKeyword debugKeyword = shader.keywordSpace.FindKeyword(DebugKeyword);
            LocalKeyword condensationKeyword = shader.keywordSpace.FindKeyword(CondensationKeyword);
            LocalKeyword dropletNormalKeyword = shader.keywordSpace.FindKeyword(DropletNormalKeyword);

            for (int index = data.Count - 1; index >= 0; index--)
            {
                ShaderKeywordSet keywordSet = data[index].shaderKeywordSet;
                bool debug = IsEnabled(keywordSet, debugKeyword);
                bool condensation = IsEnabled(keywordSet, condensationKeyword);
                bool dropletNormal = IsEnabled(keywordSet, dropletNormalKeyword);

                if (!EditorUserBuildSettings.development && debug)
                {
                    data.RemoveAt(index);
                    removedDebug++;
                    continue;
                }

                // Droplet normals use masks created by the condensation branch.
                // Without condensation this variant has zero visual contribution.
                if (dropletNormal && !condensation)
                {
                    data.RemoveAt(index);
                    removedDependency++;
                }
            }

            fShaderVariantBuildReport.Record(
                shader.name,
                snippet.passName,
                snippet.passType.ToString(),
                inputCount,
                data.Count,
                removedDebug,
                removedDependency);
        }

        internal static bool IsFShader(string shaderName)
        {
            return !string.IsNullOrEmpty(shaderName) &&
                   (shaderName.StartsWith("fShader/", StringComparison.Ordinal) ||
                    shaderName.StartsWith("Hidden/fShader/", StringComparison.Ordinal));
        }

        private static bool IsEnabled(ShaderKeywordSet keywordSet, LocalKeyword keyword)
        {
            return keyword.isValid && keywordSet.IsEnabled(keyword);
        }
    }

    [Serializable]
    internal sealed class fShaderVariantReportDocument
    {
        public string generatedUtc;
        public string unityVersion;
        public string buildTarget;
        public bool developmentBuild;
        public int totalInput;
        public int totalOutput;
        public int removedDebug;
        public int removedInvalidDependency;
        public string preservationPolicy;
        public List<fShaderVariantReportEntry> entries = new List<fShaderVariantReportEntry>();
    }

    [Serializable]
    internal sealed class fShaderVariantReportEntry
    {
        public string shader;
        public string pass;
        public string passType;
        public int input;
        public int output;
        public int removedDebug;
        public int removedInvalidDependency;
    }

    [InitializeOnLoad]
    internal static class fShaderVariantBuildReport
    {
        internal const string ReportPath = "Library/fShader/P5_VARIANTS.json";
        private static readonly object Gate = new object();
        private static readonly fShaderVariantReportDocument Report = new fShaderVariantReportDocument();
        private static bool writeQueued;

        static fShaderVariantBuildReport()
        {
            ResetDocument();
        }

        internal static void Record(
            string shader,
            string pass,
            string passType,
            int input,
            int output,
            int removedDebug,
            int removedInvalidDependency)
        {
            lock (Gate)
            {
                fShaderVariantReportEntry entry = Report.entries.Find(item =>
                    item.shader == shader && item.pass == pass && item.passType == passType);
                if (entry == null)
                {
                    entry = new fShaderVariantReportEntry
                    {
                        shader = shader,
                        pass = pass,
                        passType = passType
                    };
                    Report.entries.Add(entry);
                }

                entry.input += input;
                entry.output += output;
                entry.removedDebug += removedDebug;
                entry.removedInvalidDependency += removedInvalidDependency;
                Report.totalInput += input;
                Report.totalOutput += output;
                Report.removedDebug += removedDebug;
                Report.removedInvalidDependency += removedInvalidDependency;

                if (!writeQueued)
                {
                    writeQueued = true;
                    EditorApplication.delayCall += WriteReport;
                }
            }
        }

        [MenuItem("Tools/fShader/P5/Write Variant Report")]
        internal static void WriteReport()
        {
            lock (Gate)
            {
                writeQueued = false;
                Report.generatedUtc = DateTime.UtcNow.ToString("O");
                Report.unityVersion = Application.unityVersion;
                Report.buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
                Report.developmentBuild = EditorUserBuildSettings.development;
                Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
                File.WriteAllText(ReportPath, JsonUtility.ToJson(Report, true));
            }

            Debug.Log("fShader P5 variant report: " + Path.GetFullPath(ReportPath));
        }

        [MenuItem("Tools/fShader/P5/Reset Variant Counters")]
        internal static void ResetCounters()
        {
            lock (Gate)
            {
                ResetDocument();
            }
            Debug.Log("fShader P5 variant counters reset.");
        }

        private static void ResetDocument()
        {
            Report.generatedUtc = string.Empty;
            Report.unityVersion = Application.unityVersion;
            Report.buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            Report.developmentBuild = EditorUserBuildSettings.development;
            Report.totalInput = 0;
            Report.totalOutput = 0;
            Report.removedDebug = 0;
            Report.removedInvalidDependency = 0;
            Report.preservationPolicy =
                "Preserve stereo, fog, lightmap, instancing, mode, screen-refraction and LTCGI variants. " +
                "Strip shipping debug variants and glass droplet-normal variants without condensation only.";
            Report.entries.Clear();
        }
    }
}
