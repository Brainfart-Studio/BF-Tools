using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFAuroraRibbonsLayer : IBFBackgroundLayer
    {
        private const int Segments = 80;
        private const string GlowShaderName = "Legacy Shaders/Particles/Additive";
        private const float MaxRotationOscillationHz = 0.1f;

        private static readonly string[] LogTags = { "Background", "AuroraRibbons" };

        private readonly BFAuroraRibbonsLayerConfig config;
        private readonly List<BFAuroraRibbon> ribbons = new List<BFAuroraRibbon>();
        private readonly List<LineRenderer> ribbonRenderers = new List<LineRenderer>();

        private Transform root;
        private Material glowMaterial;
        private int sortingOrder;
        private float elapsedTime;
        private float rotationElapsedTime;
        private float rotationOscillationElapsedTime;

        public BFAuroraRibbonsLayer(BFAuroraRibbonsLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            elapsedTime = 0f;
            rotationElapsedTime = 0f;
            rotationOscillationElapsedTime = 0f;
            this.sortingOrder = sortingOrder;

            if (config.RibbonColors.Count == 0)
                BFLogger.Error(LogTags, "BFAuroraRibbonsLayer: no ribbon colors configured, ribbons will render white.");

            GameObject rootObj = new GameObject("BFAuroraRibbonsLayer");
            rootObj.transform.SetParent(parent, false);
            rootObj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = rootObj.transform;

            glowMaterial = CreateGlowMaterial();

            ribbons.Clear();
            ribbonRenderers.Clear();
            SyncRibbonCount();

            BFLogger.Info(LogTags, $"BFAuroraRibbonsLayer: initialized with {config.RibbonCount} ribbon(s).");
        }

        public void Tick(float dt)
        {
            SyncRibbonCount();

            elapsedTime += dt * config.WaveSpeed;

            float viewWidth = Screen.width;
            float viewHeight = Screen.height;
            Vector2 center = new Vector2(viewWidth, viewHeight) * 0.5f;

            // A line this long, centered on screen, reaches past the visible bounds no matter which way it's rotated.
            float spanLength = Mathf.Sqrt(viewWidth * viewWidth + viewHeight * viewHeight);

            rotationElapsedTime = (rotationElapsedTime + dt * config.RotationSpeed) % 360f;
            rotationOscillationElapsedTime += dt * config.RotationOscillationSpeed * MaxRotationOscillationHz;
            float oscillationOffset = Mathf.Sin(rotationOscillationElapsedTime * Mathf.PI * 2f) * config.RotationOscillationAmplitude;
            float animatedAngle = config.Angle + rotationElapsedTime + oscillationOffset;

            float angleRad = animatedAngle * Mathf.Deg2Rad;
            Vector2 along = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector2 perpendicular = new Vector2(-along.y, along.x);

            for (int i = 0; i < ribbonRenderers.Count; i++)
            {
                BFAuroraRibbon ribbon = ribbons[i];
                LineRenderer lr = ribbonRenderers[i];

                ribbon.Refresh();

                Color glowColor = ApplyGlow(ribbon.Color);
                lr.startColor = glowColor;
                lr.endColor = glowColor;
                lr.startWidth = ribbon.CurrentThickness;
                lr.endWidth = ribbon.CurrentThickness;

                for (int s = 0; s <= Segments; s++)
                {
                    float xNorm = s / (float)Segments;
                    float alongOffset = (xNorm - 0.5f) * spanLength;
                    float y = ribbon.SampleY(xNorm, elapsedTime, viewHeight);
                    float perpOffset = y - center.y;

                    Vector2 point = center + along * alongOffset + perpendicular * perpOffset;
                    lr.SetPosition(s, new Vector3(point.x, point.y, 0f));
                }
            }
        }

        public void Cleanup()
        {
            BFLogger.Info(LogTags, $"BFAuroraRibbonsLayer: cleaning up {ribbons.Count} ribbon(s).");

            if (root != null)
                DestroyObject(root.gameObject);
            root = null;

            ribbons.Clear();
            ribbonRenderers.Clear();
            glowMaterial = null;
        }

        private static void DestroyObject(Object obj)
        {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        // Grows/shrinks the ribbon list and its renderers to match Ribbon Count whenever it changes.
        private void SyncRibbonCount()
        {
            int targetCount = config.RibbonCount;
            if (ribbons.Count == targetCount)
                return;

            if (ribbons.Count < targetCount)
            {
                for (int i = ribbons.Count; i < targetCount; i++)
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
            else
            {
                for (int i = ribbons.Count - 1; i >= targetCount; i--)
                {
                    DestroyObject(ribbonRenderers[i].gameObject);
                    ribbonRenderers.RemoveAt(i);
                    ribbons.RemoveAt(i);
                }
            }
        }

        private Color ApplyGlow(Color baseColor)
        {
            float glowIntensity = Mathf.Clamp01(config.Glow / 60f);
            Color glowColor = baseColor * (1f + glowIntensity * 1.5f);
            glowColor.a = Mathf.Lerp(0.6f, 1f, glowIntensity) * config.OverallOpacity;
            return glowColor;
        }

        private static Material CreateGlowMaterial()
        {
            Shader shader = Shader.Find(GlowShaderName);
            if (shader == null)
            {
                BFLogger.Warning(LogTags, $"BFAuroraRibbonsLayer: shader '{GlowShaderName}' not found, falling back to Sprites/Default (no additive glow).");
                shader = Shader.Find("Sprites/Default");
            }

            return new Material(shader);
        }
    }
}