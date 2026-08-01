using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFAuroraRibbonsLayer : IBFBackgroundLayer
    {
        private const int Segments = 80;
        private const string GlowShaderName = "Legacy Shaders/Particles/Additive";

        private readonly BFAuroraRibbonsLayerConfig config;
        private readonly List<BFAuroraRibbon> ribbons = new List<BFAuroraRibbon>();
        private readonly List<LineRenderer> ribbonRenderers = new List<LineRenderer>();

        private Transform root;
        private float elapsedTime;

        public BFAuroraRibbonsLayer(BFAuroraRibbonsLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            elapsedTime = 0f;

            GameObject rootObj = new GameObject("BFAuroraRibbonsLayer");
            rootObj.transform.SetParent(parent, false);
            rootObj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = rootObj.transform;

            Material glowMaterial = CreateGlowMaterial();

            ribbons.Clear();
            ribbonRenderers.Clear();
            for (int i = 0; i < config.RibbonCount; i++)
            {
                ribbons.Add(new BFAuroraRibbon(i, config));

                GameObject go = new GameObject($"Ribbon_{i}");
                go.transform.SetParent(root, false);
                go.layer = BFBackgroundStackManager.BackgroundLayer;

                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.positionCount = Segments + 1;
                lr.useWorldSpace = true;
                lr.material = glowMaterial;
                lr.textureMode = LineTextureMode.Stretch;
                lr.sortingOrder = sortingOrder;

                ribbonRenderers.Add(lr);
            }
        }

        public void Tick(float dt)
        {
            elapsedTime += dt * config.WaveSpeed;

            float viewWidth = Screen.width;
            float viewHeight = Screen.height;

            for (int i = 0; i < ribbonRenderers.Count; i++)
            {
                BFAuroraRibbon ribbon = ribbons[i];
                LineRenderer lr = ribbonRenderers[i];

                Color glowColor = ApplyGlow(ribbon.Color);
                lr.startColor = glowColor;
                lr.endColor = glowColor;
                lr.startWidth = ribbon.CurrentThickness;
                lr.endWidth = ribbon.CurrentThickness;

                for (int s = 0; s <= Segments; s++)
                {
                    float xNorm = s / (float)Segments;
                    float x = xNorm * viewWidth;
                    float y = ribbon.SampleY(xNorm, elapsedTime, viewHeight);
                    lr.SetPosition(s, new Vector3(x, y, 0f));
                }
            }
        }

        public void Cleanup()
        {
            if (root != null)
                Object.Destroy(root.gameObject);
            root = null;

            ribbons.Clear();
            ribbonRenderers.Clear();
        }

        private Color ApplyGlow(Color baseColor)
        {
            float glowIntensity = Mathf.Clamp01(config.Glow / 60f);
            Color glowColor = baseColor * (1f + glowIntensity * 1.5f);
            glowColor.a = Mathf.Lerp(0.6f, 1f, glowIntensity);
            return glowColor;
        }

        private static Material CreateGlowMaterial()
        {
            Shader shader = Shader.Find(GlowShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"BFAuroraRibbonsLayer: shader '{GlowShaderName}' not found, falling back to Sprites/Default (no additive glow).");
                shader = Shader.Find("Sprites/Default");
            }

            return new Material(shader);
        }
    }
}