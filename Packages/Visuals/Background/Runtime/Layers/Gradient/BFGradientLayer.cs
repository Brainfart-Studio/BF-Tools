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
        private Material material;
        private Texture2D rampTexture;

        private int lastWidth = -1;
        private int lastHeight = -1;
        private float elapsedTime;
        private float rotationElapsedTime;

        public BFGradientLayer(BFGradientLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            elapsedTime = 0f;
            rotationElapsedTime = 0f;

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

            rampTexture = CreateRampTexture();
            RefreshRampTexture();

            material = new Material(Shader.Find("Sprites/Default")) { mainTexture = rampTexture };

            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.sortingOrder = sortingOrder;

            UpdateVertexPositions();
            UpdateUVs(config.Angle, 0f);

            BFLogger.Info(LogTags, "BFGradientLayer: initialized.");
        }

        public void Tick(float dt)
        {
            RefreshRampTexture();

            if (Screen.width != lastWidth || Screen.height != lastHeight)
                UpdateVertexPositions();

            rotationElapsedTime = (rotationElapsedTime + dt * config.RotationSpeed) % 360f;
            float animatedAngle = config.Angle + rotationElapsedTime;

            elapsedTime += dt * config.ShiftSpeed;
            float shiftOffset = Mathf.Sin(elapsedTime) * config.ShiftAmplitude;

            UpdateUVs(animatedAngle, shiftOffset);
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
            material = null;
        }

        private static Texture2D CreateRampTexture()
        {
            return new Texture2D(1, RampResolution, TextureFormat.RGBA32, false)
            {
                name = "BFGradientLayer_Ramp",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        private void RefreshRampTexture()
        {
            Gradient gradient = config.ColorGradient;
            float midpoint = config.Midpoint;
            float spread = config.Spread;

            for (int i = 0; i < RampResolution; i++)
            {
                float t = i / (RampResolution - 1f);
                float shaped = Gain(t, 1f - spread);
                float remapped = Bias(shaped, midpoint);
                rampTexture.SetPixel(0, i, gradient.Evaluate(remapped));
            }

            rampTexture.Apply(false);
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

        private void UpdateVertexPositions()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            float width = lastWidth;
            float height = lastHeight;

            Vector3[] vertices = mesh.vertices;
            vertices[0] = new Vector3(0f, 0f, 0f);
            vertices[1] = new Vector3(width, 0f, 0f);
            vertices[2] = new Vector3(0f, height, 0f);
            vertices[3] = new Vector3(width, height, 0f);
            mesh.vertices = vertices;

            mesh.RecalculateBounds();
        }

        private void UpdateUVs(float angleDegrees, float shiftOffset)
        {
            mesh.uv = BuildAxisUVs(lastWidth, lastHeight, angleDegrees, shiftOffset);
        }

        private static Vector2[] BuildAxisUVs(float width, float height, float angleDegrees, float shiftOffset)
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
                uvs[i] = new Vector2(0f, (projections[i] - min) / range + shiftOffset);

            return uvs;
        }
    }
}