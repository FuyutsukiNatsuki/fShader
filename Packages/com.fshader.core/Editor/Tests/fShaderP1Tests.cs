using NUnit.Framework;
using UnityEngine;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP1Tests
    {
        private static readonly string[] RequiredP1Properties =
        {
            "_BaseMap",
            "_BaseColor",
            "_ARMHMap",
            "_AOStrength",
            "_Roughness",
            "_Metallic",
            "_HeightScale",
            "_NormalMap",
            "_NormalScale",
            "_Opacity",
            "_ReflectionStrength",
            "_IOR",
            "_FSDebugView"
        };

        [TestCaseSource(typeof(fShaderShaderCatalog), nameof(fShaderShaderCatalog.PublicShaderNames))]
        public void PublicShadersExposeSharedPBRContract(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);

            using (var material = new fShaderTestMaterial(shader))
            {
                foreach (string property in RequiredP1Properties)
                {
                    Assert.That(
                        material.Value.HasProperty(property),
                        Is.True,
                        shaderName + " is missing " + property);
                }
            }
        }

        [TestCaseSource(typeof(fShaderShaderCatalog), nameof(fShaderShaderCatalog.PublicShaderNames))]
        public void PublicShadersKeepExpectedPassStructure(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            int expectedPasses =
                shaderName == "fShader/Lite/Ice" || shaderName.EndsWith("/Standard") ? 3 : 1;
            Assert.That(shader.passCount, Is.EqualTo(expectedPasses), shaderName);
        }

        [TestCaseSource(typeof(fShaderShaderCatalog), nameof(fShaderShaderCatalog.PublicShaderNames))]
        public void DebugViewAndPBRScalarsCanBeAssigned(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);

            using (var material = new fShaderTestMaterial(shader))
            {
                material.Value.SetFloat("_Roughness", 0.42f);
                material.Value.SetFloat("_Metallic", 0.17f);
                material.Value.SetFloat("_FSDebugView", 3f);
                Assert.That(material.Value.GetFloat("_Roughness"), Is.EqualTo(0.42f).Within(0.001f));
                Assert.That(material.Value.GetFloat("_Metallic"), Is.EqualTo(0.17f).Within(0.001f));
                Assert.That(material.Value.GetFloat("_FSDebugView"), Is.EqualTo(3f).Within(0.001f));
            }
        }
    }
}
