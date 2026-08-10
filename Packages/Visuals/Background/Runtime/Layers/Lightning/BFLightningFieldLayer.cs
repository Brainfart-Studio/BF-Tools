using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFLightningFieldLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "LightningField" };
        private static readonly Color32 Color32Opaque = new Color32(255, 255, 255, 255);

        private const int MaxBolts = 4;
        private const int SegmentsPerBolt = 12;
        private const int VerticesPerSegment = 4;
        private const int VerticesPerBolt = (SegmentsPerBolt + 1) * VerticesPerSegment;
        private const int TotalVertices = MaxBolts * VerticesPerBolt;
        private const int IndicesPerBolt = SegmentsPerBolt * 6;
        private const int TotalIndices = MaxBolts * IndicesPerBolt;

        private readonly BFLightningFieldLayerConfig config;

        private Transform root;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Material material;

        private int lastWidth = -1;
        private int lastHeight = -1;
        private float nextFlashTime;
        private float currentFlashTimer;
        private bool isFlashing;
        private float activeBoltSeed;
        private float[] boltXPositions = new float[MaxBolts];

        public BFLightningFieldLayer(BFLightningFieldLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            activeBoltSeed = Random.Range(0f, 1000f);
            for (int i = 0; i < MaxBolts; i++)
            {
                boltXPositions[i] = Random.Range(0.15f, 0.85f);
            }
            ScheduleNextFlash();

            GameObject obj = new GameObject("BFLightningFieldLayer");
            obj.transform.SetParent(parent, false);
            obj.transform.position = new Vector3(0f, 0f, 10f);
            obj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = obj.transform;

            mesh = new Mesh { name = "BFLightningFieldLayer" };
            mesh.vertices = new Vector3[TotalVertices];
            mesh.uv = new Vector2[TotalVertices];
            mesh.colors32 = BuildColors();
            mesh.triangles = BuildTriangles();

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            material = new Material(Shader.Find("Sprites/Default"));
            if (config.BoltTexture != null)
            {
                material.mainTexture = config.BoltTexture;
            }

            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.sortingOrder = sortingOrder;

            UpdateGeometry();
            UpdateMaterialProperties(0f);

            BFLogger.Info(LogTags, "BFLightningFieldLayer: initialized.");
        }

        public void Tick(float dt)
        {
            if (!isFlashing)
            {
                nextFlashTime -= dt;
                if (nextFlashTime <= 0f)
                {
                    TriggerFlash();
                }
            }
            else
            {
                currentFlashTimer -= dt;
                if (currentFlashTimer <= 0f)
                {
                    isFlashing = false;
                    ScheduleNextFlash();
                }
            }

            float flashIntensity = isFlashing ? Mathf.Clamp01(currentFlashTimer / Mathf.Max(0.001f, config.FlashDuration)) : 0f;
            UpdateMaterialProperties(flashIntensity);

            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                UpdateGeometry();
            }
        }

        public void Cleanup()
        {
            if (root != null)
                DestroyObject(root.gameObject);
            root = null;

            if (mesh != null)
                DestroyObject(mesh);
            mesh = null;

            meshRenderer = null;
            material = null;
        }

        private static void DestroyObject(Object obj)
        {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        private void ScheduleNextFlash()
        {
            float variance = Random.Range(-config.FrequencyVariance, config.FrequencyVariance);
            float rate = Mathf.Max(0.1f, config.FlashFrequency + variance);
            nextFlashTime = 1f / rate;
        }

        private void TriggerFlash()
        {
            isFlashing = true;
            currentFlashTimer = Mathf.Max(0.001f, config.FlashDuration);
            activeBoltSeed = Random.Range(0f, 1000f);
            for (int i = 0; i < MaxBolts; i++)
            {
                boltXPositions[i] = Random.Range(0.1f, 0.9f);
            }
            UpdateGeometry();
        }

        private void UpdateGeometry()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            float worldWidth = lastWidth;
            float worldHeight = lastHeight;

            root.position = new Vector3(worldWidth * 0.5f, worldHeight * 0.5f, 10f);

            Vector3[] vertices = new Vector3[TotalVertices];
            Vector2[] uvs = new Vector2[TotalVertices];

            float boltThickness = Mathf.Max(8f, config.BoltScale * 35f);

            for (int b = 0; b < MaxBolts; b++)
            {
                int vertexBase = b * VerticesPerBolt;
                float startX = worldWidth * boltXPositions[b];
                float seedOffset = activeBoltSeed + (b * 133.7f);

                float prevX = startX;
                float prevY = worldHeight * 0.5f;

                for (int i = 0; i <= SegmentsPerBolt; i++)
                {
                    float t = (float)i / SegmentsPerBolt;
                    float currentY = Mathf.Lerp(worldHeight * 0.5f, -worldHeight * 0.5f, t);

                    float currentX = startX;
                    if (i > 0 && i < SegmentsPerBolt)
                    {
                        float noise = Mathf.PerlinNoise(t * 6f + seedOffset, seedOffset);
                        float jaggedOffset = (noise - 0.5f) * worldWidth * 0.35f * config.BranchingIntensity;
                        currentX += jaggedOffset;
                    }

                    Vector2 currentPos = new Vector2(currentX, currentY);
                    Vector2 dir = (i == 0) ? Vector2.down : (currentPos - new Vector2(prevX, prevY)).normalized;
                    Vector2 normal = new Vector2(-dir.y, dir.x) * (boltThickness * 0.5f);

                    int vIndex = vertexBase + (i * VerticesPerSegment);
                    vertices[vIndex + 0] = new Vector3(currentPos.x - normal.x, currentPos.y - normal.y, 0f);
                    vertices[vIndex + 1] = new Vector3(currentPos.x + normal.x, currentPos.y + normal.y, 0f);

                    // Add jagged side forks/branches
                    if (config.BranchingIntensity > 0.1f && i > 2 && i < SegmentsPerBolt - 2 && i % 3 == 0)
                    {
                        float branchDir = (Mathf.Sin(i + seedOffset) > 0f) ? 1f : -1f;
                        Vector2 branchTip = currentPos + new Vector2(branchDir * boltThickness * 5f, -boltThickness * 5f);
                        vertices[vIndex + 2] = new Vector3(currentPos.x, currentPos.y, 0f);
                        vertices[vIndex + 3] = new Vector3(branchTip.x, branchTip.y, 0f);
                    }
                    else
                    {
                        vertices[vIndex + 2] = vertices[vIndex + 0];
                        vertices[vIndex + 3] = vertices[vIndex + 1];
                    }

                    uvs[vIndex + 0] = new Vector2(0f, t);
                    uvs[vIndex + 1] = new Vector2(1f, t);
                    uvs[vIndex + 2] = new Vector2(0f, t);
                    uvs[vIndex + 3] = new Vector2(1f, t);

                    prevX = currentX;
                    prevY = currentY;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
        }

        private void UpdateMaterialProperties(float flashIntensity)
        {
            if (material == null) return;

            Color finalColor = config.BoltColor * flashIntensity;
            material.color = finalColor;

            if (config.BoltTexture != null && material.mainTexture != config.BoltTexture)
            {
                material.mainTexture = config.BoltTexture;
            }
        }

        private static Color32[] BuildColors()
        {
            Color32[] colors = new Color32[TotalVertices];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color32Opaque;
            return colors;
        }

        private static int[] BuildTriangles()
        {
            int[] triangles = new int[TotalIndices];
            int t = 0;
            for (int b = 0; b < MaxBolts; b++)
            {
                int vertexBase = b * VerticesPerBolt;
                int indexBase = b * IndicesPerBolt;

                for (int i = 0; i < SegmentsPerBolt; i++)
                {
                    int baseIndex = vertexBase + (i * VerticesPerSegment);
                    int nextIndex = vertexBase + ((i + 1) * VerticesPerSegment);

                    triangles[t++] = baseIndex + 0;
                    triangles[t++] = nextIndex + 0;
                    triangles[t++] = baseIndex + 1;

                    triangles[t++] = baseIndex + 1;
                    triangles[t++] = nextIndex + 0;
                    triangles[t++] = nextIndex + 1;
                }
            }
            return triangles;
        }
    }
}