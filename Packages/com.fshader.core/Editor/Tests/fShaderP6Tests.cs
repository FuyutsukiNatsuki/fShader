using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP6Tests
    {
        [Test]
        public void ReleasePackagesUseVersionOne()
        {
            string core = File.ReadAllText("Packages/com.fshader.core/package.json");
            string plus = File.ReadAllText("Packages/com.fshader.plus/package.json");
            StringAssert.Contains("\"version\": \"1.2.2\"", core);
            StringAssert.Contains("\"version\": \"1.2.2\"", plus);
            StringAssert.Contains("\"com.fshader.core\": \"1.2.2\"", plus);
        }

        [Test]
        public void ReleasePackagesContainRequiredMetadata()
        {
            foreach (string package in new[] { "com.fshader.core", "com.fshader.plus" })
            {
                string source = File.ReadAllText("Packages/" + package + "/package.json");
                StringAssert.Contains("\"email\"", source, package);
                StringAssert.Contains("\"license\"", source, package);
                StringAssert.Contains("\"documentationUrl\"", source, package);
                string licensePath = "Packages/" + package + "/fSHaderLicense.md";
                Assert.That(File.Exists(licensePath), Is.True, package);
                string license = File.ReadAllText(licensePath);
                StringAssert.Contains("fShader License 1.0", license, package);
                StringAssert.Contains("fSHaderLicense.md", license, package);
                StringAssert.Contains("modified from or based on fShader", license, package);
            }
        }

        [Test]
        public void UserManualAndPropertyReferenceExist()
        {
            Assert.That(File.Exists("Packages/com.fshader.core/Documentation~/USER_MANUAL_JA.md"), Is.True);
            Assert.That(File.Exists("Packages/com.fshader.core/Documentation~/PROPERTY_REFERENCE.md"), Is.True);
            Assert.That(File.Exists("Packages/com.fshader.plus/Documentation~/PLUS_GUIDE_JA.md"), Is.True);
        }

        [Test]
        public void SampleScenesAndMaterialsExist()
        {
            Assert.That(File.Exists("Packages/com.fshader.core/Samples~/Gallery/Scenes/fShader Lite Gallery.unity"), Is.True);
            Assert.That(File.Exists("Packages/com.fshader.core/Samples~/Gallery/Materials/Lite Water Ocean.mat"), Is.True);
            Assert.That(File.Exists("Packages/com.fshader.core/Samples~/Gallery/Materials/Lite Ice Frosted.mat"), Is.True);
            Assert.That(File.Exists("Packages/com.fshader.core/Samples~/Gallery/Materials/Lite Glass Condensed.mat"), Is.True);
            Assert.That(File.Exists("Packages/com.fshader.plus/Samples~/Plus Gallery/Scenes/fShader Plus Gallery.unity"), Is.True);
        }

        [Test]
        public void StarterMaterialsKeepHeavyFeaturesOff()
        {
            foreach (string shaderName in new[] { "fShader/Plus/Water", "fShader/Plus/Glass" })
            {
                Shader shader = Shader.Find(shaderName);
                Assert.That(shader, Is.Not.Null, shaderName);
                using (var material = new Material(shader))
                {
                    Assert.That(material.GetFloat("_FSScreenRefraction"), Is.EqualTo(0f));
                    Assert.That(material.GetFloat("_LTCGI"), Is.EqualTo(0f));
                }
            }
        }

        [Test]
        public void PlusRetainsSupportedLtcgiRange()
        {
            string plus = File.ReadAllText("Packages/com.fshader.plus/package.json");
            StringAssert.Contains("\"at.pimaker.ltcgi\": \">=1.6.3 <1.7.0\"", plus);
        }
    }
}
