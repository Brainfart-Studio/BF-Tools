using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFAtmosphericFieldLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "AtmosphericField" };
        private static readonly Color32 Color32Opaque = new Color32(255, 255, 255, 255);

        private const int GridResolution = 24;
        private const int GridVertexCount = (GridResolution + 1) * (GridResolution + 1);

        private readonly BFAtmosphericFieldLayerConfig config;

        private Transform root;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Material material;

        private int lastWidth = -1;
        private int lastHeight = -1;
        private float elapsedTime;

        public BFAtmosphericFieldLayer(BFAtmosphericFieldLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            elapsedTime = 0f;

            GameObject obj = new GameObject("BFAttosphericFieldLayer");
            obj.transform.SetParent(parent, false);
            obj.transform.position = new Vector3(0f, 0f, 10f);
            obj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = obj.transform;

            mesh = new Mesh { name = "BFAttosphericFieldLayer" };
            mesh.vertices = new Vector3[GridVertexCount];
            mesh.colors32 = BuildGridColors();
            mesh.triangles = BuildGridTriangles();

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            material = new Material(Shader.Find("Sprites/Default"));
            if (config.FieldTexture != null)
            {
                material.mainTexture = config.FieldTexture;
            }

            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.sortingOrder = sortingOrder;

            UpdateVertexPositions();
            UpdateMaterialProperties();

            BFLogger.Info(LogTags, "BFAttosphericFieldLayer: initialized.");
        }

        public void Tick(float dt)
        {
            elapsedTime += dt * config.ScrollSpeed;
            UpdateMaterialProperties();

            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                UpdateVertexPositions();
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

        private void UpdateVertexPositions()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            float worldWidth = lastWidth;
            float worldHeight = lastHeight;

            root.position = new Vector3(worldWidth * 0.5f, worldHeight * 0.5f, 10f);

            Vector3[] vertices = new Vector3[GridVertexCount];
            for (int row = 0; row <= GridResolution; row++)
            {
                float y = worldHeight * row / GridResolution - worldHeight * 0.5f;
                for (int col = 0; col <= GridResolution; col++)
                {
                    float x = worldWidth * col / GridResolution - worldWidth * 0.5f;
                    vertices[row * (GridResolution + 1) + col] = new Vector3(x, y, 0f);
                }
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();

            BuildGridUVs();
        }

        private void BuildGridUVs()
        {
            float angleRad = config.Angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
            Vector2 offset = direction * elapsedTime * 0.1f;

            Vector2[] uvs = new Vector2[GridVertexCount];
            for (int row = 0; row <= GridResolution; row++)
            {
                float v = (float)row / GridResolution;
                for (int col = 0; col <= GridResolution; col++)
                {
                    float u = (float)col / GridResolution;

                    Vector2 baseUV = new Vector2(u, v) * config.NoiseScale + offset;

                    if (config.EnableDistortion)
                    {
                        float noiseX = Mathf.PerlinNoise(baseUV.x + elapsedTime * config.DistortionSpeed, baseUV.y);
                        float noiseY = Mathf.PerlinNoise(baseUV.x, baseUV.y + elapsedTime * config.DistortionSpeed);
                        baseUV += new Vector2(noiseX, noiseY) * config.DistortionAmount;
                    }

                    uvs[row * (GridResolution + 1) + col] = baseUV;
                }
            }
            mesh.uv = uvs;
        }

        private void UpdateMaterialProperties()
        {
            if (material == null) return;

            material.color = config.Color;

            if (config.FieldTexture != null && material.mainTexture != config.FieldTexture)
            {
                material.mainTexture = config.FieldTexture;
            }

            BuildGridUVs();
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
            int[] triangles = new int[GridResolution * GridResolution * 6];
            int t = 0;
            for (int row = 0; row < GridResolution; row++)
            {
                for (int col = 0; col < GridResolution; col++)
                {
                    int bl = row * (GridResolution + 1) + col;
                    int br = bl + 1;
                    int tl = bl + (GridResolution + 1);
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