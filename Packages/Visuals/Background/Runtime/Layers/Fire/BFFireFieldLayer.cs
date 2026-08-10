using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFFireFieldLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "FireField" };
        private static readonly Color32 Color32Opaque = new Color32(255, 255, 255, 255);

        private const int FlameResolution = 24;
        private const int VerticesPerFlame = (FlameResolution + 1) * 2;

        private readonly BFFireFieldLayerConfig config;

        private Transform root;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Material material;

        private int lastWidth = -1;
        private int lastHeight = -1;
        private float elapsedTime;
        private float seed;

        public BFFireFieldLayer(BFFireFieldLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            elapsedTime = 0f;
            seed = Random.Range(0f, 1000f);

            GameObject obj = new GameObject("BFFireFieldLayer");
            obj.transform.SetParent(parent, false);
            obj.transform.position = new Vector3(0f, 0f, 10f);
            obj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = obj.transform;

            mesh = new Mesh { name = "BFFireFieldLayer" };

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            material = new Material(Shader.Find("Sprites/Default"));

            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.sortingOrder = sortingOrder;

            RebuildMeshStructure();
            UpdateFlameGeometry();

            BFLogger.Info(LogTags, "BFFireFieldLayer: initialized.");
        }

        public void Tick(float dt)
        {
            elapsedTime += dt;
            UpdateFlameGeometry();

            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                RebuildMeshStructure();
                UpdateFlameGeometry();
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

        private void RebuildMeshStructure()
        {
            int columns = Mathf.Max(2, config.FlameColumns);
            int totalVerts = columns * VerticesPerFlame;
            int totalTriangles = columns * FlameResolution * 6;

            Vector3[] vertices = new Vector3[totalVerts];
            Vector2[] uvs = new Vector2[totalVerts];
            Color32[] colors = new Color32[totalVerts];
            int[] triangles = new int[totalTriangles];

            int tIdx = 0;
            for (int c = 0; c < columns; c++)
            {
                int baseV = c * VerticesPerFlame;
                for (int r = 0; r <= FlameResolution; r++)
                {
                    int v0 = baseV + (r * 2);
                    int v1 = v0 + 1;

                    float vNorm = (float)r / FlameResolution;
                    uvs[v0] = new Vector2(0f, vNorm);
                    uvs[v1] = new Vector2(1f, vNorm);

                    float alpha = Mathf.Sin(vNorm * Mathf.PI);
                    colors[v0] = new Color32(255, 255, 255, (byte)(alpha * 255));
                    colors[v1] = new Color32(255, 255, 255, (byte)(alpha * 255));

                    if (r < FlameResolution)
                    {
                        int nextV0 = v0 + 2;
                        int nextV1 = v1 + 2;

                        triangles[tIdx++] = v0;
                        triangles[tIdx++] = nextV0;
                        triangles[tIdx++] = v1;

                        triangles[tIdx++] = v1;
                        triangles[tIdx++] = nextV0;
                        triangles[tIdx++] = nextV1;
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors32 = colors;
            mesh.triangles = triangles;
        }

        private void UpdateFlameGeometry()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            float worldWidth = lastWidth;
            float worldHeight = lastHeight;

            root.position = new Vector3(worldWidth * 0.5f, worldHeight * 0.5f, 10f);

            int columns = Mathf.Max(2, config.FlameColumns);
            Vector3[] vertices = mesh.vertices;
            float columnWidth = worldWidth / columns;

            for (int c = 0; c < columns; c++)
            {
                int baseV = c * VerticesPerFlame;
                float centerX = (c + 0.5f) * columnWidth - (worldWidth * 0.5f);

                for (int r = 0; r <= FlameResolution; r++)
                {
                    int v0 = baseV + (r * 2);
                    int v1 = v0 + 1;

                    float vNorm = (float)r / FlameResolution; // 0 at bottom, 1 at tip
                    float yPos = (vNorm * worldHeight * 0.8f * config.FlameHeight) - (worldHeight * 0.4f);

                    float timeOffset = elapsedTime * config.RiseSpeed + seed + (c * 1.7f);
                    float swayNoise = Mathf.PerlinNoise(timeOffset * 0.5f, vNorm * 2f + c);
                    float sway = (swayNoise - 0.5f) * worldWidth * 0.15f * config.Turbulence;

                    float flickerNoise = Mathf.PerlinNoise(vNorm * 3f - timeOffset * config.TurbulenceSpeed, c * 3f);
                    float widthMultiplier = Mathf.Sin(vNorm * Mathf.PI) * (0.5f + flickerNoise * 0.5f);
                    float halfWidth = (columnWidth * 0.45f) * Mathf.Max(0.1f, widthMultiplier);

                    float xCenter = centerX + (sway * vNorm);

                    vertices[v0] = new Vector3(xCenter - halfWidth, yPos, 0f);
                    vertices[v1] = new Vector3(xCenter + halfWidth, yPos, 0f);
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();

            if (material != null)
            {
                material.color = config.FireColor;
            }
        }
    }
}