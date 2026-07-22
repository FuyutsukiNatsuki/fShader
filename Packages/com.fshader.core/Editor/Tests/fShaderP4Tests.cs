using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP4Tests
    {
        [TestCase("fShader/Plus/Water")]
        [TestCase("fShader/Plus/Glass")]
        public void PlusTransparentModesExposeLTCGIContract(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            Assert.That(shader.passCount, Is.EqualTo(1), shaderName);
            using (var material = new fShaderTestMaterial(shader))
            {
                Assert.That(material.Value.GetTag("LTCGI", false, string.Empty), Is.EqualTo("_LTCGI"));
                Assert.That(material.Value.HasProperty("_LTCGI"), Is.True);
                Assert.That(material.Value.HasProperty("_LTCGIDiffuseStrength"), Is.True);
                Assert.That(material.Value.HasProperty("_LTCGISpecularStrength"), Is.True);
                Assert.That(material.Value.HasProperty("_LTCGIMaxBrightness"), Is.True);
                Assert.That(material.Value.GetFloat("_LTCGI"), Is.EqualTo(0f));
            }
        }

        [Test]
        public void GlassExposesCondensationDiffuseBoost()
        {
            using (var material = new fShaderTestMaterial(Shader.Find("fShader/Plus/Glass")))
            {
                Assert.That(material.Value.HasProperty("_LTCGICondensationDiffuse"), Is.True);
            }
        }

        [TestCase("fShader/Lite/Water")]
        [TestCase("fShader/Lite/Ice")]
        [TestCase("fShader/Lite/Glass")]
        [TestCase("fShader/Plus/Ice")]
        public void UnsupportedVariantsExcludeLTCGI(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            using (var material = new fShaderTestMaterial(shader))
            {
                Assert.That(material.Value.HasProperty("_LTCGI"), Is.False, shaderName);
            }
        }

        [Test]
        public void AdapterUsesApiV2AndBrightnessClamp()
        {
            string adapter = File.ReadAllText("Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderLTCGI.cginc");
            StringAssert.Contains("LTCGI_structs.cginc", adapter);
            StringAssert.Contains("LTCGI_V2_DIFFUSE_CALLBACK", adapter);
            StringAssert.Contains("LTCGI_V2_SPECULAR_CALLBACK", adapter);
            StringAssert.Contains("surface.roughness", adapter);
            StringAssert.Contains("_LTCGIMaxBrightness", adapter);

            string common = File.ReadAllText("Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc");
            StringAssert.Contains("#if defined(FSHADER_LTCGI)", common);
            StringAssert.Contains("FSEvaluateLTCGI", common);
        }

        [Test]
        public void PackagePinsSupportedLTCGIVersion()
        {
            string plusPackage = File.ReadAllText("Packages/com.fshader.plus/package.json");
            StringAssert.Contains("\"at.pimaker.ltcgi\": \">=1.6.3 <1.7.0\"", plusPackage);
            Assert.That(File.Exists("Packages/com.fshader.plus/THIRD_PARTY_NOTICES.md"), Is.True);
        }
    }
}
