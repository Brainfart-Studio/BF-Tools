using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStarsLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "TwinklingStars" };

        private static Texture2D dotTexture;

        private readonly BFTwinklingStarsLayerConfig config;
        private readonly List<BFTwinklingStar> stars = new List<BFTwinklingStar>();

        private Transform root;
        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Material material;

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
            EnsureDotTexture();

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
            mesh.uv = BuildUVs(stars.Count);
            mesh.triangles = BuildTriangles(stars.Count);

            MeshFilter filter = rootObj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            material = new Material(Shader.Find("Sprites/Default")) { mainTexture = dotTexture };

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

        private static Vector2[] BuildUVs(int starCount)
        {
            Vector2[] uvs = new Vector2[starCount * 4];
            for (int i = 0; i < starCount; i++)
            {
                int baseIndex = i * 4;
                uvs[baseIndex] = new Vector2(0f, 0f);
                uvs[baseIndex + 1] = new Vector2(1f, 0f);
                uvs[baseIndex + 2] = new Vector2(0f, 1f);
                uvs[baseIndex + 3] = new Vector2(1f, 1f);
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

        private static void EnsureDotTexture()
        {
            if (dotTexture != null)
                return;

            dotTexture = new Texture2D(1, 1) { name = "BFTwinklingStarsLayer_Dot" };
            dotTexture.SetPixel(0, 0, Color.white);
            dotTexture.Apply();
        }
    }
}