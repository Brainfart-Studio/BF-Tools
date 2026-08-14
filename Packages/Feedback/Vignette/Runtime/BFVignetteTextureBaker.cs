using UnityEngine;

namespace BFTools.Feedback.Vignette
{
    public static class BFVignetteTextureBaker
    {
        public static Texture2D Bake(float radius, float softness, float roundness, float aspect, int resolution = 256)
        {
            var texture = new Texture2D(resolution, resolution, TextureFormat.Alpha8, false)
            {
                name = "BFVignetteMask",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float xScale = Mathf.Lerp(Mathf.Max(aspect, 0.0001f), 1f, Mathf.Clamp01(roundness));
            float edge = Mathf.Max(radius + softness, radius + 0.0001f);

            var pixels = new Color32[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                float ny = (y / (float)(resolution - 1)) * 2f - 1f;
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x / (float)(resolution - 1)) * 2f - 1f;
                    float dx = nx * xScale;
                    float dist = Mathf.Sqrt(dx * dx + ny * ny);
                    float mask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(radius, edge, dist));
                    pixels[y * resolution + x] = new Color32(255, 255, 255, (byte)(mask * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}