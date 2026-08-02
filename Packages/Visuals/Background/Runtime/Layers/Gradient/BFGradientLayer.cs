using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFGradientLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "Gradient" };
        private static readonly Color32 Color32Opaque = new Color32(255, 255, 255, 255);

        private const int RampResolution = 256;
        private const float MaxRotationOscillationHz = 0.1f;
        private const float MaxWaveFrequencyOscillationHz = 0.1f;
        private const float MaxWaveAmplitudeDriftHz = 0.1f;

        private const int WaveGridResolution = 24;
        private const int GridVertexCount = (WaveGridResolution + 1) * (WaveGridResolution + 1);

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
        private float rotationOscillationElapsedTime;
        private float waveFrequencyOscillationElapsedTime;
        private float waveAmplitudeDriftElapsedTime;

        public BFGradientLayer(BFGradientLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            elapsedTime = 0f;
            rotationElapsedTime = 0f;
            rotationOscillationElapsedTime = 0f;
            waveFrequencyOscillationElapsedTime = 0f;
            waveAmplitudeDriftElapsedTime = 0f;

            GameObject obj = new GameObject("BFGradientLayer");
            obj.transform.SetParent(parent, false);
            obj.transform.position = new Vector3(0f, 0f, 10f);
            obj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = obj.transform;

            mesh = new Mesh { name = "BFGradientLayer" };
            mesh.vertices = new Vector3[GridVertexCount];
            mesh.colors32 = BuildGridColors();
            mesh.triangles = BuildGridTriangles();

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            rampTexture = CreateRampTexture();
            RefreshRampTexture();

            material = new Material(Shader.Find("Sprites/Default")) { mainTexture = rampTexture };

            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.sortingOrder = sortingOrder;

            UpdateVertexPositions();
            UpdateUVs(config.Angle, 0f, config.WaveFrequency, 0f);

            BFLogger.Info(LogTags, "BFGradientLayer: initialized.");
        }

        public void Tick(float dt)
        {
            RefreshRampTexture();

            if (Screen.width != lastWidth || Screen.height != lastHeight)
                UpdateVertexPositions();

            rotationElapsedTime = (rotationElapsedTime + dt * config.RotationSpeed) % 360f;
            rotationOscillationElapsedTime += dt * config.RotationOscillationSpeed * MaxRotationOscillationHz;
            float oscillationOffset = Mathf.Sin(rotationOscillationElapsedTime * Mathf.PI * 2f) * config.RotationOscillationAmplitude;
            float animatedAngle = config.Angle + rotationElapsedTime + oscillationOffset;

            elapsedTime += dt * config.ShiftSpeed;
            float shiftOffset = Mathf.Sin(elapsedTime) * config.ShiftAmplitude;

            waveFrequencyOscillationElapsedTime += dt * config.WaveFrequencyOscillationSpeed * MaxWaveFrequencyOscillationHz;
            float waveFrequencyOffset = Mathf.Sin(waveFrequencyOscillationElapsedTime * Mathf.PI * 2f) * config.WaveFrequencyOscillationAmplitude;
            float animatedWaveFrequency = config.WaveFrequency + waveFrequencyOffset;

            waveAmplitudeDriftElapsedTime += dt * config.WaveAmplitudeRandomnessSpeed * MaxWaveAmplitudeDriftHz;

            UpdateUVs(animatedAngle, shiftOffset, animatedWaveFrequency, waveAmplitudeDriftElapsedTime);
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

            Vector3[] vertices = new Vector3[GridVertexCount];
            for (int row = 0; row <= WaveGridResolution; row++)
            {
                float y = height * row / WaveGridResolution;
                for (int col = 0; col <= WaveGridResolution; col++)
                {
                    float x = width * col / WaveGridResolution;
                    vertices[row * (WaveGridResolution + 1) + col] = new Vector3(x, y, 0f);
                }
            }
            mesh.vertices = vertices;

            mesh.RecalculateBounds();
        }

        private void UpdateUVs(float angleDegrees, float shiftOffset, float waveFrequency, float waveAmplitudeDriftTime)
        {
            mesh.uv = BuildGridUVs(lastWidth, lastHeight, angleDegrees, shiftOffset, config.WaveAmplitude, waveFrequency, config.WaveAmplitudeRandomness, waveAmplitudeDriftTime);
        }

        private static Vector2[] BuildGridUVs(float width, float height, float angleDegrees, float shiftOffset, float waveAmplitude, float waveFrequency, float waveAmplitudeRandomness, float waveAmplitudeDriftTime)
        {
            float angleRad = angleDegrees * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
            Vector2 perpendicular = new Vector2(direction.y, -direction.x);

            Vector2[] corners =
            {
                new Vector2(-width * 0.5f, -height * 0.5f),
                new Vector2(width * 0.5f, -height * 0.5f),
                new Vector2(-width * 0.5f, height * 0.5f),
                new Vector2(width * 0.5f, height * 0.5f)
            };

            float axisMin = float.MaxValue, axisMax = float.MinValue;
            float perpMin = float.MaxValue, perpMax = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                float axisProjection = corners[i].x * direction.x + corners[i].y * direction.y;
                float perpProjection = corners[i].x * perpendicular.x + corners[i].y * perpendicular.y;
                axisMin = Mathf.Min(axisMin, axisProjection);
                axisMax = Mathf.Max(axisMax, axisProjection);
                perpMin = Mathf.Min(perpMin, perpProjection);
                perpMax = Mathf.Max(perpMax, perpProjection);
            }

            float axisRange = Mathf.Max(axisMax - axisMin, 0.0001f);
            float perpRange = Mathf.Max(perpMax - perpMin, 0.0001f);

            Vector2[] uvs = new Vector2[GridVertexCount];
            for (int row = 0; row <= WaveGridResolution; row++)
            {
                float y = height * row / WaveGridResolution - height * 0.5f;
                for (int col = 0; col <= WaveGridResolution; col++)
                {
                    float x = width * col / WaveGridResolution - width * 0.5f;

                    float axisProjection = x * direction.x + y * direction.y;
                    float perpProjection = x * perpendicular.x + y * perpendicular.y;

                    float axisT = (axisProjection - axisMin) / axisRange;
                    float perpT = (perpProjection - perpMin) / perpRange;

                    float noise = Mathf.PerlinNoise(perpT * waveFrequency, waveAmplitudeDriftTime);
                    float amplitudeMultiplier = 1f + (noise * 2f - 1f) * waveAmplitudeRandomness;
                    float wave = Mathf.Sin(perpT * waveFrequency * Mathf.PI * 2f) * waveAmplitude * amplitudeMultiplier;

                    uvs[row * (WaveGridResolution + 1) + col] = new Vector2(0f, axisT + wave + shiftOffset);
                }
            }

            return uvs;
        }

        private static Color32[] BuildGridColors()
        {
            Color32[] colors = new Color32[GridVertexCount];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color32Opaque;
            return colors;
        }

        private static int[] BuildGridTriangles()
        {
            int[] triangles = new int[WaveGridResolution * WaveGridResolution * 6];
            int t = 0;
            for (int row = 0; row < WaveGridResolution; row++)
            {
                for (int col = 0; col < WaveGridResolution; col++)
                {
                    int bl = row * (WaveGridResolution + 1) + col;
                    int br = bl + 1;
                    int tl = bl + (WaveGridResolution + 1);
                    int tr = tl + 1;

                    triangles[t++] = bl;
                    triangles[t++] = tl;
                    triangles[t++] = br;

                    triangles[t++] = tl;
                    triangles[t++] = tr;
                    triangles[t++] = br;
                }
            }
            return triangles;
        }
    }
}