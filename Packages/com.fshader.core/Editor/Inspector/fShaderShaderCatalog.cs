using System;

namespace fShader.Editor
{
    public enum fShaderEdition
    {
        Lite = 0,
        Plus = 1
    }

    public enum fShaderMode
    {
        Water = 0,
        Ice = 1,
        Glass = 2,
        Standard = 3
    }

    public static class fShaderShaderCatalog
    {
        public const string Version = "1.2.0";

        public static readonly string[] EditionLabels = { "Lite", "Plus" };
        public static readonly string[] ModeLabels = { "Water", "Ice", "Glass", "Standard" };

        public static readonly string[] PublicShaderNames =
        {
            "fShader/Lite/Water",
            "fShader/Lite/Ice",
            "fShader/Lite/Glass",
            "fShader/Lite/Standard",
            "fShader/Plus/Water",
            "fShader/Plus/Ice",
            "fShader/Plus/Glass",
            "fShader/Plus/Standard"
        };

        public static string GetShaderName(fShaderEdition edition, fShaderMode mode)
        {
            return $"fShader/{edition}/{mode}";
        }

        public static bool TryParse(string shaderName, out fShaderEdition edition, out fShaderMode mode)
        {
            edition = fShaderEdition.Lite;
            mode = fShaderMode.Water;

            if (shaderName == "Hidden/fShader/Lite/GlassScreenRefraction")
            {
                edition = fShaderEdition.Lite;
                mode = fShaderMode.Glass;
                return true;
            }
            if (shaderName == "Hidden/fShader/Plus/WaterScreenRefraction")
            {
                edition = fShaderEdition.Plus;
                mode = fShaderMode.Water;
                return true;
            }
            if (shaderName == "Hidden/fShader/Plus/IceScreenRefraction")
            {
                edition = fShaderEdition.Plus;
                mode = fShaderMode.Ice;
                return true;
            }
            if (shaderName == "Hidden/fShader/Plus/GlassScreenRefraction")
            {
                edition = fShaderEdition.Plus;
                mode = fShaderMode.Glass;
                return true;
            }

            if (string.IsNullOrEmpty(shaderName) ||
                !shaderName.StartsWith("fShader/", StringComparison.Ordinal))
            {
                return false;
            }

            edition = shaderName.IndexOf("/Plus/", StringComparison.Ordinal) >= 0
                ? fShaderEdition.Plus
                : fShaderEdition.Lite;

            if (shaderName.EndsWith("/Ice", StringComparison.Ordinal))
            {
                mode = fShaderMode.Ice;
            }
            else if (shaderName.EndsWith("/Glass", StringComparison.Ordinal))
            {
                mode = fShaderMode.Glass;
            }
            else if (shaderName.EndsWith("/Standard", StringComparison.Ordinal))
            {
                mode = fShaderMode.Standard;
            }
            else
            {
                mode = fShaderMode.Water;
            }

            return true;
        }
    }
}
