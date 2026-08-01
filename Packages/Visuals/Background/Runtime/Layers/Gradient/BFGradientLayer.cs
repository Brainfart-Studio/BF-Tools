using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFGradientLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "Gradient" };

        private readonly BFGradientLayerConfig config;

        private Transform root;
        private Mesh mesh;
        private MeshRenderer meshRenderer;

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
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = Texture2D.whiteTexture };
            meshRenderer.sortingOrder = sortingOrder;

            UpdateMesh();

            BFLogger.Info(LogTags, "BFGradientLayer: initialized.");
        }

        public void Tick(float dt)
        {
            UpdateMesh();
        }

        public void Cleanup()
        {
            if (root != null)
                Object.Destroy(root.gameObject);
            root = null;

            if (mesh != null)
                Object.Destroy(mesh);
            mesh = null;

            meshRenderer = null;
        }

        private void UpdateMesh()
        {
            float width = Screen.width;
            float height = Screen.height;

            Vector3[] vertices = mesh.vertices;
            vertices[0] = new Vector3(0f, 0f, 0f);
            vertices[1] = new Vector3(width, 0f, 0f);
            vertices[2] = new Vector3(0f, height, 0f);
            vertices[3] = new Vector3(width, height, 0f);
            mesh.vertices = vertices;

            Color32 top = config.TopColor;
            Color32 bottom = config.BottomColor;
            mesh.colors32 = new[] { bottom, bottom, top, top };
            mesh.RecalculateBounds();
        }
    }
}