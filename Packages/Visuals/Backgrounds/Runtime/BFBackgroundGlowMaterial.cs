using UnityEngine;

namespace BFTools.Visuals.Background
{
    internal static class BFBackgroundGlowMaterial
    {
        private const string ShaderName = "Legacy Shaders/Particles/Additive";

        public static Material Create()
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"BFBackgroundGlowMaterial: shader '{ShaderName}' not found, falling back to Sprites/Default (no additive glow).");
                shader = Shader.Find("Sprites/Default");
            }

            return new Material(shader);
        }
    }
}