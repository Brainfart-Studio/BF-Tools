using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFGradientLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "Gradient" };
        private static readonly Color32 Color32Opaque = new Color32(255, 255, 255, 255);

        private const int RampResolution = 256;

        private readonly BFGradientLayerConfig config;

        private Transform root;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Texture2D rampTexture;

        private int lastWidth = -1;
        private int lastHeight = -1;
        private float lastAngle;

        public BFGradientLayer(BFGradientLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            GameObject obj = new GameObject("BFGradientLayer");
            obj.transform.SetParent(parent, false);
            obj.transform.position = new Vector3(0f, 0f, 10f);
            obj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = obj.transform;

            mesh = new Mesh { name = "BFGradientLayer" };
            mesh.vertices = new Vector3[4];
            mesh.colors32 = new[] { Color32Opaque, Color32Opaque, Color32Opaque, Color32Opaque };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            rampTexture = BuildRampTexture();

            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = rampTexture };
            meshRenderer.sortingOrder = sortingOrder;

            UpdateGeometry();

            BFLogger.Info(LogTags, "BFGradientLayer: initialized.");
        }

        public void Tick(float dt)
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight || !Mathf.Approximately(config.Angle, lastAngle))
                UpdateGeometry();
        }

        public void Cleanup()
        {
            if (root != null)
                Object.Destroy(root.gameObject);
            root = null;

            if (mesh != null)
                Object.Destroy(mesh);
            mesh = null;

            if (rampTexture != null)
                Object.Destroy(rampTexture);
            rampTexture = null;

            meshRenderer = null;
        }

        private Texture2D BuildRampTexture()
        {
            Texture2D texture = new Texture2D(1, RampResolution, TextureFormat.RGBA32, false)
            {
                name = "BFGradientLayer_Ramp",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Gradient gradient = config.ColorGradient;
            float midpoint = config.Midpoint;
            float spread = config.Spread;

            for (int i = 0; i < RampResolution; i++)
            {
                float t = i / (RampResolution - 1f);
                float shaped = Gain(t, 1f - spread);
                float remapped = Bias(shaped, midpoint);
                texture.SetPixel(0, i, gradient.Evaluate(remapped));
            }

            texture.Apply(false);
            return texture;
        }

        private static float Bias(float t, float b)
        {
            b = Mathf.Clamp(b, 0.0001f, 0.9999f);
            return t / ((1f / b - 2f) * (1f - t) + 1f);
        }

        private static float Gain(float t, float g)
        {
            g = Mathf.Clamp(g, 0.0001f, 0.9999f);
            return t < 0.5f
                ? Bias(t * 2f, g) * 0.5f
                : 1f - Bias(2f - t * 2f, g) * 0.5f;
        }

        private void UpdateGeometry()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            lastAngle = config.Angle;

            float width = lastWidth;
            float height = lastHeight;

            Vector3[] vertices = mesh.vertices;
            vertices[0] = new Vector3(0f, 0f, 0f);
            vertices[1] = new Vector3(width, 0f, 0f);
            vertices[2] = new Vector3(0f, height, 0f);
            vertices[3] = new Vector3(width, height, 0f);
            mesh.vertices = vertices;

            mesh.uv = BuildAxisUVs(width, height, lastAngle);

            mesh.RecalculateBounds();
        }

        private static Vector2[] BuildAxisUVs(float width, float height, float angleDegrees)
        {
            float angleRad = angleDegrees * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));

            Vector2[] corners =
            {
                new Vector2(-width * 0.5f, -height * 0.5f),
                new Vector2(width * 0.5f, -height * 0.5f),
                new Vector2(-width * 0.5f, height * 0.5f),
                new Vector2(width * 0.5f, height * 0.5f)
            };

            float[] projections = new float[4];
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                float p = corners[i].x * direction.x + corners[i].y * direction.y;
                projections[i] = p;
                min = Mathf.Min(min, p);
                max = Mathf.Max(max, p);
            }

            float range = Mathf.Max(max - min, 0.0001f);
            Vector2[] uvs = new Vector2[4];
            for (int i = 0; i < 4; i++)
                uvs[i] = new Vector2(0f, (projections[i] - min) / range);

            return uvs;
        }
    }
}