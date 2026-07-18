using System.IO;
using NUnit.Framework;
using UnityEngine;

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
                "_SparkleStrength", "_SparkleDistance", "_FSIceScatter");
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
        public void OptionalGlassScreenShaderAndColdMistImport()
        {
            Shader screenGlass = Shader.Find("Hidden/fShader/Lite/GlassScreenRefraction");
            Shader coldMist = Shader.Find("fShader/Effects/ColdMist");
            Assert.That(screenGlass, Is.Not.Null);
            Assert.That(coldMist, Is.Not.Null);
            Assert.That(coldMist.passCount, Is.EqualTo(1));
        }

        [TestCase("Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteWater.shader")]
        [TestCase("Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteIce.shader")]
        [TestCase("Packages/com.fshader.core/Runtime/Shaders/Lite/fShaderLiteGlass.shader")]
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
