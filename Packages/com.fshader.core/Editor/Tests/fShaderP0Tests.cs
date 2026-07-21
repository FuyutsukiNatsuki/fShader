using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP0Tests
    {
        [Test]
        public void ProjectUsesLinearColorSpace()
        {
            Assert.AreEqual(ColorSpace.Linear, PlayerSettings.colorSpace);
        }

        [Test]
        public void EmbeddedPackageFoldersExist()
        {
            Assert.IsTrue(AssetDatabase.IsValidFolder("Packages/com.fshader.core"));
            Assert.IsTrue(AssetDatabase.IsValidFolder("Packages/com.fshader.plus"));
        }

        [TestCase("fShader/Lite/Water", 0, 0, 1)]
        [TestCase("fShader/Lite/Ice", 0, 1, 3)]
        [TestCase("fShader/Lite/Glass", 0, 2, 1)]
        [TestCase("fShader/Lite/Standard", 0, 3, 3)]
        [TestCase("fShader/Plus/Water", 1, 0, 1)]
        [TestCase("fShader/Plus/Ice", 1, 1, 1)]
        [TestCase("fShader/Plus/Glass", 1, 2, 1)]
        [TestCase("fShader/Plus/Standard", 1, 3, 3)]
        public void PublicShaderImportsWithStableP0Contract(
            string shaderName,
            int expectedEdition,
            int expectedMode,
            int expectedPassCount)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.IsNotNull(shader, shaderName);
            Assert.IsTrue(shader.isSupported, shaderName);
            Assert.AreEqual(expectedPassCount, shader.passCount, shaderName);

            using (Material material = new Material(shader))
            {
                Assert.IsTrue(material.HasProperty(fShaderPropertyNames.BaseColor));
                Assert.IsTrue(material.HasProperty(fShaderPropertyNames.Version));
                Assert.IsTrue(material.HasProperty(fShaderPropertyNames.Edition));
                Assert.IsTrue(material.HasProperty(fShaderPropertyNames.Mode));
                Assert.IsTrue(material.HasProperty(fShaderPropertyNames.FeatureFlags));
                Assert.AreEqual(expectedEdition, material.GetFloat(fShaderPropertyNames.Edition));
                Assert.AreEqual(expectedMode, material.GetFloat(fShaderPropertyNames.Mode));
            }
        }
    }
}
