using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP7Tests
    {
        [Test]
        public void CatalogExposesStandardMode()
        {
            Assert.That(fShaderShaderCatalog.ModeLabels, Does.Contain("Standard"));
            Assert.That(fShaderShaderCatalog.PublicShaderNames, Does.Contain("fShader/Lite/Standard"));
            Assert.That(fShaderShaderCatalog.PublicShaderNames, Does.Contain("fShader/Plus/Standard"));
            Assert.That((int)fShaderMode.Standard, Is.EqualTo(3));
        }

        [TestCase("fShader/Lite/Standard", fShaderEdition.Lite)]
        [TestCase("fShader/Plus/Standard", fShaderEdition.Plus)]
        public void StandardShaderNamesParseAsStandardMode(string shaderName, fShaderEdition expectedEdition)
        {
            Assert.That(
                fShaderShaderCatalog.TryParse(shaderName, out fShaderEdition edition, out fShaderMode mode),
                Is.True,
                shaderName);
            Assert.That(edition, Is.EqualTo(expectedEdition));
            Assert.That(mode, Is.EqualTo(fShaderMode.Standard));
            Assert.That(fShaderShaderCatalog.GetShaderName(expectedEdition, fShaderMode.Standard), Is.EqualTo(shaderName));
        }

        [TestCase("fShader/Lite/Standard")]
        [TestCase("fShader/Plus/Standard")]
        public void StandardShadersAreOpaquePBRWithoutModeFeatures(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            using (var material = new fShaderTestMaterial(shader))
            {
                Assert.That(material.Value.GetTag("RenderType", false), Is.EqualTo("Opaque"), shaderName);

                // Shared PBR contract is present.
                foreach (string property in new[] { "_BaseMap", "_ARMHMap", "_NormalMap", "_ReflectionStrength", "_IOR" })
                {
                    Assert.That(material.Value.HasProperty(property), Is.True, shaderName + " missing " + property);
                }

                // No transparency or mode-specific features.
                foreach (string property in new[]
                {
                    "_FSIceTransparent", "_FSScreenRefraction", "_ShallowColor",
                    "_TransmissionColor", "_IceColor", "_CondensationMap"
                })
                {
                    Assert.That(material.Value.HasProperty(property), Is.False, shaderName + " should not expose " + property);
                }
            }
        }

        [Test]
        public void LiteStandardHasNoLtcgiOrBoxProjection()
        {
            string source = File.ReadAllText("Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteStandard.shader");
            StringAssert.DoesNotContain("LTCGI", source);
            StringAssert.DoesNotContain("FSHADER_BOX_PROJECTION", source);
            StringAssert.DoesNotContain("GrabPass", source);
            StringAssert.Contains("#pragma fragment FSFragOpaque", source);
        }

        [Test]
        public void PlusStandardSupportsLtcgiAndBoxProjection()
        {
            Shader shader = Shader.Find("fShader/Plus/Standard");
            Assert.That(shader, Is.Not.Null);
            using (var material = new fShaderTestMaterial(shader))
            {
                Assert.That(material.Value.HasProperty("_LTCGI"), Is.True);
                Assert.That(material.Value.HasProperty("_LTCGIDiffuseStrength"), Is.True);
                Assert.That(material.Value.HasProperty("_FSBoxProjection"), Is.True);
                Assert.That(material.Value.GetTag("LTCGI", false, string.Empty), Is.EqualTo("_LTCGI"));
                Assert.That(material.Value.GetFloat("_LTCGI"), Is.EqualTo(0f));
            }

            string source = File.ReadAllText("Packages/com.fshader.plus/Runtime/Shaders/Plus/fShaderPlusStandard.shader");
            StringAssert.Contains("#pragma fragment FSFragOpaque", source);
            StringAssert.DoesNotContain("GrabPass", source);
            StringAssert.DoesNotContain("ForwardAdd", source);
        }

        [Test]
        public void TemplateSampleTexturesExist()
        {
            const string root = "Packages/com.fshader.core/Samples~/Gallery/Textures/";
            foreach (string fileName in new[]
            {
                "Water_Base_Calm.png", "Water_WaveNormal_Fine.png", "Water_FoamMask.png",
                "Ice_Base_Glacial.png", "Ice_Normal_Crystal.png", "Ice_FrostMask.png", "Ice_CrackMask.png",
                "Glass_Condensation_RGB.png", "Glass_CondensationNormal.png"
            })
            {
                Assert.That(File.Exists(root + fileName), Is.True, fileName);
            }
        }
    }
}
