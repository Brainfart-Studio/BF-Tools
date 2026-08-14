using UnityEngine;

namespace BFTools.Feedback.Vignette
{
    public static class BFVignetteTextureBaker
    {
        public static Texture2D Bake(float radius, float softness, float roundness, float aspect, float[] profileAngles, float[] profileValues, int resolution = 256)
        {
            var texture = new Texture2D(resolution, resolution, TextureFormat.Alpha8, false)
            {
                name = "BFVignetteMask",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float xScale = Mathf.Lerp(Mathf.Max(aspect, 0.0001f), 1f, Mathf.Clamp01(roundness));

            var pixels = new Color32[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                float ny = (y / (float)(resolution - 1)) * 2f - 1f;
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x / (float)(resolution - 1)) * 2f - 1f;
                    float dx = nx * xScale;
                    float dist = Mathf.Sqrt(dx * dx + ny * ny);
                    float angle = Mathf.Atan2(ny, dx);

                    float waveRadius = Mathf.Max(radius + SampleProfile(profileAngles, profileValues, angle), 0f);
                    float edge = Mathf.Max(waveRadius + softness, waveRadius + 0.0001f);
                    float mask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(waveRadius, edge, dist));
                    pixels[y * resolution + x] = new Color32(255, 255, 255, (byte)(mask * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static float SampleProfile(float[] profileAngles, float[] profileValues, float angle)
        {
            if (profileValues == null || profileValues.Length == 0)
                return 0f;

            if (profileValues.Length == 1)
                return profileValues[0];

            float twoPi = Mathf.PI * 2f;
            float wrapped = Mathf.Repeat(angle, twoPi);
            float position = wrapped / twoPi * profileValues.Length;

            int index0 = Mathf.FloorToInt(position) % profileValues.Length;
            int index1 = (index0 + 1) % profileValues.Length;
            float frac = position - Mathf.Floor(position);

            return Mathf.Lerp(profileValues[index0], profileValues[index1], frac);
        }
    }
}