using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background.AuroraRibbons
{
    public class BFAuroraRibbonsRenderer : IBFBackgroundRenderer
    {
        private const int Segments = 80;

        private readonly BFAuroraRibbonsEffect effect;
        private readonly BFAuroraRibbonsConfig config;

        private Transform root;
        private Mesh backgroundMesh;
        private readonly List<LineRenderer> ribbonRenderers = new List<LineRenderer>();
        private readonly List<SpriteRenderer> starRenderers = new List<SpriteRenderer>();
        private static Sprite dotSprite;

        public BFAuroraRibbonsRenderer(BFAuroraRibbonsEffect effect, BFAuroraRibbonsConfig config)
        {
            this.effect = effect;
            this.config = config;
        }

        public void Init(Transform parent)
        {
            EnsureDotSprite();

            GameObject rootObj = new GameObject("BFAuroraRibbonsRenderer");
            rootObj.transform.SetParent(parent, false);
            root = rootObj.transform;

            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(root, false);
            backgroundObj.transform.position = new Vector3(0f, 0f, 10f);

            backgroundMesh = new Mesh { name = "BFAuroraRibbonsBackground" };
            backgroundMesh.vertices = new Vector3[4];
            backgroundMesh.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            backgroundMesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };

            MeshFilter backgroundFilter = backgroundObj.AddComponent<MeshFilter>();
            backgroundFilter.mesh = backgroundMesh;

            MeshRenderer backgroundRenderer = backgroundObj.AddComponent<MeshRenderer>();
            Material backgroundMaterial = new Material(Shader.Find("Sprites/Default"));
            backgroundMaterial.mainTexture = Texture2D.whiteTexture;
            backgroundRenderer.material = backgroundMaterial;

            ribbonRenderers.Clear();
            for (int i = 0; i < effect.Ribbons.Count; i++)
            {
                GameObject go = new GameObject($"Ribbon_{i}");
                go.transform.SetParent(root, false);

                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.positionCount = Segments + 1;
                lr.useWorldSpace = true;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.textureMode = LineTextureMode.Stretch;

                ribbonRenderers.Add(lr);
            }

            starRenderers.Clear();
            for (int i = 0; i < effect.Stars.Count; i++)
            {
                GameObject go = new GameObject($"Star_{i}");
                go.transform.SetParent(root, false);

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = dotSprite;
                sr.color = Color.white;

                starRenderers.Add(sr);
            }
        }

        public void Render()
        {
            float viewWidth = Screen.width;
            float viewHeight = Screen.height;

            UpdateBackground(viewWidth, viewHeight);

            for (int i = 0; i < ribbonRenderers.Count; i++)
            {
                BFAuroraRibbon ribbon = effect.Ribbons[i];
                LineRenderer lr = ribbonRenderers[i];

                lr.startColor = ribbon.Color;
                lr.endColor = ribbon.Color;
                lr.startWidth = ribbon.CurrentThickness;
                lr.endWidth = ribbon.CurrentThickness;

                for (int s = 0; s <= Segments; s++)
                {
                    float xNorm = s / (float)Segments;
                    float x = xNorm * viewWidth;
                    float y = ribbon.SampleY(x, effect.ElapsedTime, viewHeight);
                    lr.SetPosition(s, new Vector3(x, y, 0f));
                }
            }

            for (int i = 0; i < starRenderers.Count; i++)
            {
                BFBackgroundStar star = effect.Stars[i];
                SpriteRenderer sr = starRenderers[i];

                Vector2 pos = star.Position;
                sr.transform.position = new Vector3(pos.x * viewWidth, pos.y * viewHeight, 0f);
                sr.transform.localScale = Vector3.one * star.Size;

                Color c = sr.color;
                c.a = star.Alpha;
                sr.color = c;
            }
        }

        public void Cleanup()
        {
            if (root != null)
                Object.Destroy(root.gameObject);

            if (backgroundMesh != null)
                Object.Destroy(backgroundMesh);
            backgroundMesh = null;

            ribbonRenderers.Clear();
            starRenderers.Clear();
        }

        private void UpdateBackground(float viewWidth, float viewHeight)
        {
            Vector3[] vertices = backgroundMesh.vertices;
            vertices[0] = new Vector3(0f, 0f, 0f);
            vertices[1] = new Vector3(viewWidth, 0f, 0f);
            vertices[2] = new Vector3(0f, viewHeight, 0f);
            vertices[3] = new Vector3(viewWidth, viewHeight, 0f);
            backgroundMesh.vertices = vertices;

            Color32 top = config.BaseTop;
            Color32 bottom = config.BaseBottom;
            backgroundMesh.colors32 = new[] { bottom, bottom, top, top };
            backgroundMesh.RecalculateBounds();
        }

        private static void EnsureDotSprite()
        {
            if (dotSprite != null)
                return;

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            dotSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}