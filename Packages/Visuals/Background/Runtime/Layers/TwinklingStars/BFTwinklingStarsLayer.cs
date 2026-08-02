using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStarsLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "TwinklingStars" };

        private const int RampResolution = 256;

        private readonly BFTwinklingStarsLayerConfig config;
        private readonly List<BFTwinklingStar> stars = new List<BFTwinklingStar>();

        private Transform root;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Material material;
        private Texture2D rampTexture;

        private Vector3[] vertices;
        private Color32[] colors;

        private int lastWidth = -1;
        private int lastHeight = -1;

        public BFTwinklingStarsLayer(BFTwinklingStarsLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            GameObject rootObj = new GameObject("BFTwinklingStarsLayer");
            rootObj.transform.SetParent(parent, false);
            rootObj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = rootObj.transform;

            stars.Clear();

            for (int i = 0; i < config.StarCount; i++)
                stars.Add(new BFTwinklingStar(config));

            int vertexCount = stars.Count * 4;
            vertices = new Vector3[vertexCount];
            colors = new Color32[vertexCount];

            mesh = new Mesh { name = "BFTwinklingStarsLayer" };
            mesh.MarkDynamic();
            mesh.vertices = vertices;
            mesh.colors32 = colors;
            mesh.uv = BuildUVs(stars);
            mesh.triangles = BuildTriangles(stars.Count);

            MeshFilter filter = rootObj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            rampTexture = CreateRampTexture();
            RefreshRampTexture();

            material = new Material(Shader.Find("Sprites/Default")) { mainTexture = rampTexture };

            meshRenderer = rootObj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.sortingOrder = sortingOrder;

            lastWidth = -1;
            lastHeight = -1;
            UpdateVertexPositions();

            BFLogger.Info(LogTags, $"BFTwinklingStarsLayer: initialized with {stars.Count} star(s).");
        }

        public void Tick(float dt)
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight)
                UpdateVertexPositions();

            for (int i = 0; i < stars.Count; i++)
            {
                BFTwinklingStar star = stars[i];
                star.Tick(dt);

                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(star.Alpha) * 255f);
                Color32 color = new Color32(255, 255, 255, alpha);

                int baseIndex = i * 4;
                colors[baseIndex] = color;
                colors[baseIndex + 1] = color;
                colors[baseIndex + 2] = color;
                colors[baseIndex + 3] = color;
            }

            mesh.colors32 = colors;
        }

        public void Cleanup()
        {
            BFLogger.Info(LogTags, $"BFTwinklingStarsLayer: cleaning up {stars.Count} star(s).");

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
            vertices = null;
            colors = null;

            stars.Clear();
        }

        private void UpdateVertexPositions()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            for (int i = 0; i < stars.Count; i++)
            {
                BFTwinklingStar star = stars[i];
                float halfSize = star.Size * 0.5f;
                float x = star.Position.x * lastWidth;
                float y = star.Position.y * lastHeight;

                int baseIndex = i * 4;
                vertices[baseIndex] = new Vector3(x - halfSize, y - halfSize, 0f);
                vertices[baseIndex + 1] = new Vector3(x + halfSize, y - halfSize, 0f);
                vertices[baseIndex + 2] = new Vector3(x - halfSize, y + halfSize, 0f);
                vertices[baseIndex + 3] = new Vector3(x + halfSize, y + halfSize, 0f);
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
        }

        private static Vector2[] BuildUVs(List<BFTwinklingStar> stars)
        {
            Vector2[] uvs = new Vector2[stars.Count * 4];
            for (int i = 0; i < stars.Count; i++)
            {
                int baseIndex = i * 4;
                Vector2 uv = new Vector2(0f, stars[i].ColorT);
                uvs[baseIndex] = uv;
                uvs[baseIndex + 1] = uv;
                uvs[baseIndex + 2] = uv;
                uvs[baseIndex + 3] = uv;
            }
            return uvs;
        }

        private static int[] BuildTriangles(int starCount)
        {
            int[] triangles = new int[starCount * 6];
            int t = 0;
            for (int i = 0; i < starCount; i++)
            {
                int baseIndex = i * 4;
                int bl = baseIndex;
                int br = baseIndex + 1;
                int tl = baseIndex + 2;
                int tr = baseIndex + 3;

                triangles[t++] = bl;
                triangles[t++] = tl;
                triangles[t++] = br;

                triangles[t++] = tl;
                triangles[t++] = tr;
                triangles[t++] = br;
            }
            return triangles;
        }

        private static Texture2D CreateRampTexture()
        {
            return new Texture2D(1, RampResolution, TextureFormat.RGBA32, false)
            {
                name = "BFTwinklingStarsLayer_Ramp",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        private void RefreshRampTexture()
        {
            Gradient gradient = config.ColorGradient;

            for (int i = 0; i < RampResolution; i++)
            {
                float t = i / (RampResolution - 1f);
                rampTexture.SetPixel(0, i, gradient.Evaluate(t));
            }

            rampTexture.Apply(false);
        }
    }
}