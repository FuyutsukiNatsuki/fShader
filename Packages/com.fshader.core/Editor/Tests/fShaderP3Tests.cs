using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP3Tests
    {
        [Test]
        public void PlusWaterExposesP3Contract()
        {
            AssertProperties(
                "fShader/Plus/Water",
                "_ShallowColor", "_DeepColor", "_AbsorptionColor", "_AbsorptionStrength",
                "_WaterThickness", "_DepthStrength", "_WaveNormalMap", "_WaveNormalMap2",
                "_WaveScaleA", "_WaveScaleB", "_WaveCount", "_WaveTimeScale",
                "_FoamMap", "_FoamDetailScale", "_FoamCrestStrength", "_CausticsMap",
                "_FSBoxProjection", "_FSScreenRefraction");
        }

        [Test]
        public void PlusIceExposesP3Contract()
        {
            AssertProperties(
                "fShader/Plus/Ice",
                "_AbsorptionColor", "_AbsorptionStrength", "_FrostMap", "_FrostScaleA",
                "_FrostScaleB", "_FrostEdge", "_CrackMap", "_CrackParallax",
                "_CrackGlowColor", "_CrackGlowStrength", "_BackLightColor",
                "_BackLightStrength", "_SparkleDensity", "_SparkleSize", "_SparkleDistance",
                "_FSIceTransparent", "_FSScreenRefraction", "_RefractionStrength");
        }

        [Test]
        public void PlusGlassExposesP3Contract()
        {
            AssertProperties(
                "fShader/Plus/Glass",
                "_TransmissionColor", "_AbsorptionColor", "_GlassThickness",
                "_CondensationMap", "_CondensationNormal", "_DropletStrength",
                "_TrailStrength", "_MicroFogStrength", "_CondensationRoughness",
                "_CondensationOpacity", "_CondensationFadeDistance",
                "_FSBoxProjection", "_FSScreenRefraction");
        }

        [TestCase("fShader/Plus/Water")]
        [TestCase("fShader/Plus/Ice")]
        [TestCase("fShader/Plus/Glass")]
        public void StandardPlusShadersKeepOneNormalPass(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            Assert.That(shader.passCount, Is.EqualTo(1), shaderName);
            using (var material = new fShaderTestMaterial(shader))
            {
                if (material.Value.HasProperty("_FSScreenRefraction"))
                {
                    Assert.That(material.Value.GetFloat("_FSScreenRefraction"), Is.EqualTo(0f));
                }
            }
        }

        [TestCase("Packages/com.fshader.plus/Runtime/Shaders/Plus/fShaderPlusWater.shader")]
        [TestCase("Packages/com.fshader.plus/Runtime/Shaders/Plus/fShaderPlusIce.shader")]
        [TestCase("Packages/com.fshader.plus/Runtime/Shaders/Plus/fShaderPlusGlass.shader")]
        [TestCase("Packages/com.fshader.plus/Runtime/Shaders/Plus/fShaderPlusStandard.shader")]
        public void StandardPlusShadersAvoidOptionalHeavyDependencies(string path)
        {
            string source = File.ReadAllText(path);
            StringAssert.DoesNotContain("GrabPass", source);
            StringAssert.DoesNotContain("CameraDepth", source);
            StringAssert.DoesNotContain("ForwardAdd", source);
        }

        [Test]
        public void OptionalScreenShadersUseSharedGrab()
        {
            Assert.That(Shader.Find("Hidden/fShader/Plus/WaterScreenRefraction"), Is.Not.Null);
            Assert.That(Shader.Find("Hidden/fShader/Plus/IceScreenRefraction"), Is.Not.Null);
            Assert.That(Shader.Find("Hidden/fShader/Plus/GlassScreenRefraction"), Is.Not.Null);
            string water = File.ReadAllText("Packages/com.fshader.plus/Runtime/Shaders/Plus/Hidden/fShaderPlusWaterScreenRefraction.shader");
            string ice = File.ReadAllText("Packages/com.fshader.plus/Runtime/Shaders/Plus/Hidden/fShaderPlusIceScreenRefraction.shader");
            string glass = File.ReadAllText("Packages/com.fshader.plus/Runtime/Shaders/Plus/Hidden/fShaderPlusGlassScreenRefraction.shader");
            StringAssert.Contains("GrabPass { \"_fShaderSharedGrab\" }", water);
            StringAssert.Contains("GrabPass { \"_fShaderSharedGrab\" }", ice);
            StringAssert.Contains("GrabPass { \"_fShaderSharedGrab\" }", glass);
        }

        [Test]
        public void CatalogParsesOptionalPlusShaders()
        {
            Assert.That(fShaderShaderCatalog.TryParse(
                "Hidden/fShader/Plus/WaterScreenRefraction",
                out fShaderEdition waterEdition,
                out fShaderMode waterMode), Is.True);
            Assert.That(waterEdition, Is.EqualTo(fShaderEdition.Plus));
            Assert.That(waterMode, Is.EqualTo(fShaderMode.Water));

            Assert.That(fShaderShaderCatalog.TryParse(
                "Hidden/fShader/Plus/IceScreenRefraction",
                out fShaderEdition iceEdition,
                out fShaderMode iceMode), Is.True);
            Assert.That(iceEdition, Is.EqualTo(fShaderEdition.Plus));
            Assert.That(iceMode, Is.EqualTo(fShaderMode.Ice));

            Assert.That(fShaderShaderCatalog.TryParse(
                "Hidden/fShader/Plus/GlassScreenRefraction",
                out fShaderEdition glassEdition,
                out fShaderMode glassMode), Is.True);
            Assert.That(glassEdition, Is.EqualTo(fShaderEdition.Plus));
            Assert.That(glassMode, Is.EqualTo(fShaderMode.Glass));
        }

        [Test]
        public void LiteToPlusWaterKeepsSharedMaterialValues()
        {
            Shader lite = Shader.Find("fShader/Lite/Water");
            Shader plus = Shader.Find("fShader/Plus/Water");
            Assert.That(lite, Is.Not.Null);
            Assert.That(plus, Is.Not.Null);
            using (var material = new fShaderTestMaterial(lite))
            {
                material.Value.SetColor("_BaseColor", new Color(0.17f, 0.42f, 0.73f, 0.61f));
                material.Value.SetFloat("_Roughness", 0.27f);
                material.Value.SetColor("_ShallowColor", new Color(0.2f, 0.8f, 0.9f, 1f));
                material.Value.shader = plus;
                Assert.That(material.Value.GetColor("_BaseColor").r, Is.EqualTo(0.17f).Within(0.001f));
                Assert.That(material.Value.GetFloat("_Roughness"), Is.EqualTo(0.27f).Within(0.001f));
                Assert.That(material.Value.GetColor("_ShallowColor").g, Is.EqualTo(0.8f).Within(0.001f));
            }
        }

        private static void AssertProperties(string shaderName, params string[] properties)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            using (var material = new fShaderTestMaterial(shader))
            {
                foreach (string property in properties)
                {
                    Assert.That(material.Value.HasProperty(property), Is.True, shaderName + " missing " + property);
                }
            }
        }
    }
}
