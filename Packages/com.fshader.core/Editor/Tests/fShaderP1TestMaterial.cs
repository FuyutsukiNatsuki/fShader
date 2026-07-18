using System;
using UnityEngine;

namespace fShader.Editor.Tests
{
    internal sealed class fShaderTestMaterial : IDisposable
    {
        public UnityEngine.Material Value { get; }

        public fShaderTestMaterial(Shader shader)
        {
            Value = new UnityEngine.Material(shader);
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(Value);
        }
    }
}
