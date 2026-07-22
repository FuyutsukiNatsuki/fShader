using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace fShader.Editor.Tests
{
    public sealed class fShaderP5Tests
    {
        [Test]
        public void ScreenRefractionUsesStereoSafeScreenTextureSampling()
        {
            string common = File.ReadAllText("Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc");
            StringAssert.Contains("UNITY_DECLARE_SCREENSPACE_TEXTURE(_fShaderSharedGrab)", common);
            StringAssert.Contains("UNITY_SAMPLE_SCREENSPACE_TEXTURE(_fShaderSharedGrab, screenUV)", common);
            StringAssert.Contains("distortion *= unity_StereoScaleOffset[unity_StereoEyeIndex].xy", common);
            StringAssert.Contains("UnityStereoClamp(screenUV, unity_StereoScaleOffset[unity_StereoEyeIndex])", common);
            StringAssert.Contains("UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX", common);
        }

        [Test]
        public void CameraDependentEffectsUseMirrorAwarePosition()
        {
            string common = File.ReadAllText("Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderCommon.cginc");
            StringAssert.Contains("float _VRChatMirrorMode", common);
            StringAssert.Contains("float3 _VRChatMirrorCameraPos", common);
            StringAssert.Contains("FSWorldSpaceCameraPosition()", common);

            string[] modeIncludes =
            {
                "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderWater.cginc",
                "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderIce.cginc",
                "Packages/com.fshader.core/Runtime/Shaders/Includes/fShaderGlass.cginc",
                "Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderPlusWater.cginc",
                "Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderPlusIce.cginc",
                "Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderPlusGlass.cginc",
                "Packages/com.fshader.plus/Runtime/Shaders/Includes/fShaderLTCGI.cginc"
            };
            foreach (string path in modeIncludes)
            {
                string source = File.ReadAllText(path);
                StringAssert.DoesNotContain("UnityWorldSpaceViewDir", source, path);
                StringAssert.DoesNotContain("distance(_WorldSpaceCameraPos", source, path);
            }
        }

        [Test]
        public void VariantStripperUsesConservativeRulesAndWritesReport()
        {
            string source = File.ReadAllText("Packages/com.fshader.core/Editor/Build/fShaderShaderVariantStripper.cs");
            StringAssert.Contains("FSHADER_DEBUG", source);
            StringAssert.Contains("FSHADER_GLASS_CONDENSATION", source);
            StringAssert.Contains("FSHADER_GLASS_DROPLET_NORMAL", source);
            StringAssert.Contains("Library/fShader/P5_VARIANTS.json", source);
            StringAssert.DoesNotContain("STEREO_INSTANCING_ON\")", source);
            StringAssert.DoesNotContain("LIGHTMAP_ON\")", source);
            StringAssert.DoesNotContain("FSHADER_LTCGI\")", source);
        }

        [TestCase("fShader/Lite/Water", 1)]
        [TestCase("fShader/Lite/Ice", 3)]
        [TestCase("fShader/Lite/Glass", 1)]
        [TestCase("fShader/Lite/Standard", 3)]
        [TestCase("fShader/Plus/Water", 1)]
        [TestCase("fShader/Plus/Ice", 1)]
        [TestCase("fShader/Plus/Glass", 1)]
        [TestCase("fShader/Plus/Standard", 3)]
        public void PublicShadersKeepExpectedPassBudget(string shaderName, int expectedPasses)
        {
            Shader shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);
            Assert.That(shader.passCount, Is.EqualTo(expectedPasses), shaderName);
        }

        [Test]
        public void QASceneGeneratorIncludesRefractionABDiagnostics()
        {
            string source = File.ReadAllText("Packages/com.fshader.core/Editor/Tools/fShaderP5QATools.cs");
            StringAssert.Contains("CreateRefractionDiagnosticStage", source);
            StringAssert.Contains("P5_Refraction_AB.png", source);
            StringAssert.Contains("DisableGameObjectsWithComponentType", source);
            StringAssert.Contains("DisableGameObjectsWithComponentType(scene, \"MirrorReflection\")", source);
            StringAssert.Contains("Plus_Water_Refraction_OFF_Test", source);
            StringAssert.Contains("Plus_Water_Refraction_ON_Test", source);
            StringAssert.Contains("Plus_Glass_Refraction_OFF_Test", source);
            StringAssert.Contains("Plus_Glass_Refraction_ON_Test", source);
            StringAssert.Contains("GetOrCreateDiagnosticNormal", source);
            StringAssert.Contains("_RefractionStrength\", 0.50f", source);
            StringAssert.Contains("MEASURE VISUAL A/B", source);
            StringAssert.Contains("MEASURE WATER OFF", source);
            StringAssert.Contains("MEASURE GLASS ON", source);
            StringAssert.Contains("RSHIFT + ~ + 1", source);
        }

        [Test]
        public void CatalogMarksReleaseVersion()
        {
            Assert.That(fShaderShaderCatalog.Version, Is.EqualTo("1.2.2"));
        }
    }
}
