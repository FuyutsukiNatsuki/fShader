using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP2Tests
    {
        [Test]
        public void LiteWaterExposesModeContract()
        {
            AssertProperties(
                "fShader/Lite/Water",
                "_ShallowColor", "_DeepColor", "_WaveNormalMap", "_WaveNormalScale",
                "_WaveSpeedA", "_WaveSpeedB", "_WaveAmplitude", "_WaveLength",
                "_WaveDirection", "_FresnelStrength", "_FoamMap", "_FoamStrength",
                "_RefractionStrength", "_FSWaterVertexWaves");
        }

        [Test]
        public void LiteIceExposesModeContract()
        {
            AssertProperties(
                "fShader/Lite/Ice",
                "_IceColor", "_IceThickness", "_FrostMap", "_FrostStrength",
                "_CrackMap", "_CrackDepth", "_ScatterColor", "_ScatterStrength",
                "_SparkleStrength", "_SparkleDistance", "_FSIceScatter", "_FSIceTransparent",
                "_FSSrcBlend", "_FSDstBlend", "_FSZWrite");
        }

        [Test]
        public void LiteGlassExposesModeContract()
        {
            AssertProperties(
                "fShader/Lite/Glass",
                "_TransmissionColor", "_GlassThickness", "_RefractionStrength",
                "_CondensationMap", "_CondensationNormal", "_CondensationAmount",
                "_DropletSpeed", "_FSScreenRefraction");
        }

        [Test]
        public void OptionalGlassScreenShaderImports()
        {
            Shader screenGlass = Shader.Find("Hidden/fShader/Lite/GlassScreenRefraction");
            Assert.That(screenGlass, Is.Not.Null);
        }

        [TestCase("fShader/Lite/Ice", true)]
        [TestCase("fShader/Plus/Ice", false)]
        public void IceTransparentModeSynchronizesRenderState(string shaderName, bool lite)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            using (var material = new fShaderTestMaterial(shader))
            {
                material.Value.SetFloat("_FSIceTransparent", 1f);
                fShaderIceSurfaceState.Sync(material.Value);

                Assert.That(material.Value.IsKeywordEnabled("FSHADER_ICE_TRANSPARENT"), Is.True);
                Assert.That(material.Value.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
                Assert.That(material.Value.GetFloat("_FSDstBlend"), Is.EqualTo((float)BlendMode.OneMinusSrcAlpha));
                Assert.That(material.Value.GetFloat("_FSZWrite"), Is.EqualTo(0f));
                Assert.That(material.Value.GetTag("RenderType", false), Is.EqualTo("Transparent"));

                material.Value.SetFloat("_FSIceTransparent", 0f);
                fShaderIceSurfaceState.Sync(material.Value);
                Assert.That(material.Value.IsKeywordEnabled("FSHADER_ICE_TRANSPARENT"), Is.False);
                Assert.That(material.Value.renderQueue, Is.EqualTo((int)RenderQueue.Geometry));
                Assert.That(material.Value.GetFloat("_FSDstBlend"), Is.EqualTo((float)BlendMode.Zero));
                Assert.That(material.Value.GetFloat("_FSZWrite"), Is.EqualTo(1f));

                material.Value.shader = Shader.Find(lite ? "fShader/Lite/Water" : "fShader/Plus/Water");
                material.Value.SetFloat(fShaderPropertyNames.Mode, (float)fShaderMode.Water);
                fShaderIceSurfaceState.Sync(material.Value);
                Assert.That(material.Value.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
                Assert.That(material.Value.GetTag("RenderType", false), Is.EqualTo("Transparent"));
            }
        }

        [TestCase("Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteWater.shader")]
        [TestCase("Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteIce.shader")]
        [TestCase("Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteGlass.shader")]
        [TestCase("Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteStandard.shader")]
        public void StandardLiteShadersAvoidHeavyDependencies(string path)
        {
            string source = File.ReadAllText(path);
            StringAssert.DoesNotContain("GrabPass", source);
            StringAssert.DoesNotContain("LTCGI", source);
            StringAssert.DoesNotContain("CameraDepth", source);
            StringAssert.DoesNotContain("ForwardAdd", source);
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
