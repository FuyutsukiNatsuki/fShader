using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP8Tests
    {
        [TestCaseSource(typeof(fShaderShaderCatalog), nameof(fShaderShaderCatalog.PublicShaderNames))]
        public void PublicShadersExposeCullDefaultingToBack(string shaderName)
        {
            using (var material = new fShaderTestMaterial(Shader.Find(shaderName)))
            {
                Assert.That(material.Value.HasProperty("_Cull"), Is.True, shaderName);
                Assert.That(material.Value.GetFloat("_Cull"), Is.EqualTo((float)CullMode.Back), shaderName);
            }
        }

        [Test]
        public void ForwardPassesBindCullToProperty()
        {
            foreach (string path in new[]
            {
                "Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteWater.shader",
                "Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteStandard.shader",
                "Packages/com.fshader.plus/Runtime/Shaders/Plus/fShaderPlusGlass.shader",
                "Packages/com.fshader.plus/Runtime/Shaders/Plus/Hidden/fShaderPlusWaterScreenRefraction.shader"
            })
            {
                string source = File.ReadAllText(path);
                StringAssert.Contains("Cull [_Cull]", source, path);
                StringAssert.DoesNotContain("Cull Back", source, path);
            }
        }

        [Test]
        public void DoubleSidedFlipsShadingNormalOnBackFaces()
        {
            string common = File.ReadAllText("Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc");
            StringAssert.Contains("SV_IsFrontFace", common);
            StringAssert.Contains("FSApplyDoubleSided", common);
        }

        [TestCaseSource(typeof(fShaderShaderCatalog), nameof(fShaderShaderCatalog.PublicShaderNames))]
        public void PublicShadersExposeQueueOverride(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            using (var material = new fShaderTestMaterial(shader))
            {
                Assert.That(material.Value.HasProperty("_FSQueueOverride"), Is.True, shaderName);
            }
        }

        [TestCase("fShader/Lite/Water")]
        [TestCase("fShader/Lite/Glass")]
        [TestCase("fShader/Plus/Water")]
        [TestCase("fShader/Plus/Glass")]
        public void TransparentShadersExposeTransparentZWrite(string shaderName)
        {
            using (var material = new fShaderTestMaterial(Shader.Find(shaderName)))
            {
                Assert.That(material.Value.HasProperty("_FSTransparentZWrite"), Is.True, shaderName);
            }
        }

        [TestCase("fShader/Lite/Standard")]
        [TestCase("fShader/Plus/Standard")]
        public void OpaqueStandardShadersOmitTransparentZWrite(string shaderName)
        {
            using (var material = new fShaderTestMaterial(Shader.Find(shaderName)))
            {
                Assert.That(material.Value.HasProperty("_FSTransparentZWrite"), Is.False, shaderName);
            }
        }

        [Test]
        public void CustomRenderQueueSurvivesSyncButAutoResets()
        {
            using (var material = new fShaderTestMaterial(Shader.Find("fShader/Plus/Water")))
            {
                material.Value.SetFloat(fShaderPropertyNames.Mode, (float)fShaderMode.Water);

                material.Value.SetFloat("_FSQueueOverride", 1f);
                material.Value.renderQueue = 2345;
                fShaderInspector.SyncMaterial(material.Value);
                Assert.That(material.Value.renderQueue, Is.EqualTo(2345), "custom queue must survive sync");

                material.Value.SetFloat("_FSQueueOverride", 0f);
                fShaderInspector.SyncMaterial(material.Value);
                Assert.That(material.Value.renderQueue, Is.EqualTo((int)RenderQueue.Transparent), "auto must restore transparent queue");
            }
        }

        [Test]
        public void TransparentZWriteToggleDrivesShaderZWrite()
        {
            // The Water/Glass FORWARD pass binds ZWrite to _FSTransparentZWrite, so the
            // toggle value directly controls depth writes without a keyword.
            string water = System.IO.File.ReadAllText(
                "Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteWater.shader");
            StringAssert.Contains("ZWrite [_FSTransparentZWrite]", water);
        }

        [Test]
        public void UserTemplateExportImportRoundTrip()
        {
            string exported = null;
            try
            {
                using (var source = new fShaderTestMaterial(Shader.Find("fShader/Plus/Glass")))
                {
                    source.Value.SetFloat("_Roughness", 0.271f);
                    exported = fShaderTemplateIO.ExportMaterial(source.Value, "fShaderUnitTestTemplate");
                }
                Assert.That(exported, Is.Not.Null.And.Not.Empty);
                Assert.That(fShaderTemplateIO.ListUserTemplates().Any(t => t.AssetPath == exported), Is.True,
                    "exported template should be listed");

                using (var target = new fShaderTestMaterial(Shader.Find("fShader/Lite/Water")))
                {
                    bool applied = fShaderTemplateIO.ApplyToMaterials(new[] { target.Value }, exported);
                    Assert.That(applied, Is.True);
                    Assert.That(target.Value.shader.name, Is.EqualTo("fShader/Plus/Glass"), "template should switch shader");
                    Assert.That(target.Value.GetFloat("_Roughness"), Is.EqualTo(0.271f).Within(0.001f));
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(exported))
                {
                    AssetDatabase.DeleteAsset(exported);
                }
            }
        }
    }
}
