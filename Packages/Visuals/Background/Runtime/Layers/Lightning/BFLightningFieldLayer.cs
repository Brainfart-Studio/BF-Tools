using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFLightningFieldLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "LightningField" };
        private static readonly Color32 Color32Opaque = new Color32(255, 255, 255, 255);

        private const int BoltSegments = 16;
        private const int VerticesPerSegment = 4;
        private const int TotalVertices = (BoltSegments + 1) * VerticesPerSegment;
        private const int TotalIndices = BoltSegments * 6;

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
        private float boltXPosition;

        public BFLightningFieldLayer(BFLightningFieldLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            activeBoltSeed = Random.Range(0f, 1000f);
            boltXPosition = Random.Range(0.2f, 0.8f);
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
            boltXPosition = Random.Range(0.2f, 0.8f);
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

            float boltThickness = Mathf.Max(10f, config.BoltScale * 25f);
            float startX = worldWidth * boltXPosition;

            float prevX = startX;
            float prevY = worldHeight * 0.5f;

            for (int i = 0; i <= BoltSegments; i++)
            {
                float t = (float)i / BoltSegments;
                float currentY = Mathf.Lerp(worldHeight * 0.5f, -worldHeight * 0.5f, t);

                float currentX = startX;
                if (i > 0 && i < BoltSegments)
                {
                    float noise = Mathf.PerlinNoise(t * 5f + activeBoltSeed, activeBoltSeed);
                    float jaggedOffset = (noise - 0.5f) * worldWidth * 0.4f * config.BranchingIntensity;
                    currentX += jaggedOffset;
                }

                Vector2 currentPos = new Vector2(currentX, currentY);
                Vector2 dir = (i == 0) ? Vector2.down : (currentPos - new Vector2(prevX, prevY)).normalized;
                Vector2 normal = new Vector2(-dir.y, dir.x) * (boltThickness * 0.5f);

                int vIndex = i * VerticesPerSegment;
                vertices[vIndex + 0] = new Vector3(currentPos.x - normal.x, currentPos.y - normal.y, 0f);
                vertices[vIndex + 1] = new Vector3(currentPos.x + normal.x, currentPos.y + normal.y, 0f);

                // Optional branching stub
                if (config.BranchingIntensity > 0.2f && i > 3 && i < BoltSegments - 3 && i % 4 == 0)
                {
                    float branchDir = (Mathf.Sin(i + activeBoltSeed) > 0f) ? 1f : -1f;
                    Vector2 branchPos = currentPos + new Vector2(branchDir * boltThickness * 4f, -boltThickness * 4f);
                    vertices[vIndex + 2] = new Vector3(currentPos.x, currentPos.y, 0f);
                    vertices[vIndex + 3] = new Vector3(branchPos.x, branchPos.y, 0f);
                }
                else
                {
                    vertices[vIndex + 2] = vertices[vIndex + 0];
                    vertices[vIndex + 3] = vertices[vIndex + 1];
                }

                float vCoord = t;
                uvs[vIndex + 0] = new Vector2(0f, vCoord);
                uvs[vIndex + 1] = new Vector2(1f, vCoord);
                uvs[vIndex + 2] = new Vector2(0f, vCoord);
                uvs[vIndex + 3] = new Vector2(1f, vCoord);

                prevX = currentX;
                prevY = currentY;
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
            for (int i = 0; i < BoltSegments; i++)
            {
                int baseIndex = i * VerticesPerSegment;
                int nextIndex = (i + 1) * VerticesPerSegment;

                // Main bolt quad
                triangles[t++] = baseIndex + 0;
                triangles[t++] = nextIndex + 0;
                triangles[t++] = baseIndex + 1;

                triangles[t++] = baseIndex + 1;
                triangles[t++] = nextIndex + 0;
                triangles[t++] = nextIndex + 1;
            }
            return triangles;
        }
    }
}