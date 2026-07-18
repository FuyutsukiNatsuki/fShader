using System;
using UnityEngine;

namespace fShader.Editor.Tests
{
    internal sealed class Material : IDisposable
    {
        private readonly UnityEngine.Material material;

        public Material(Shader shader)
        {
            material = new UnityEngine.Material(shader);
        }

        public bool HasProperty(string propertyName)
        {
            return material.HasProperty(propertyName);
        }

        public float GetFloat(string propertyName)
        {
            return material.GetFloat(propertyName);
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
    }
}
