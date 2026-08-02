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
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
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
            if (Screen.width != lastWidth || Screen.height != lastHeight)
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
            for (int i = 0; i < RampResolution; i++)
            {
                float t = i / (RampResolution - 1f);
                texture.SetPixel(0, i, gradient.Evaluate(t));
            }

            texture.Apply(false);
            return texture;
        }

        private void UpdateGeometry()
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
    }
}