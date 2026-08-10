using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public sealed class BFImpossibleGardenLayer : IBFBackgroundLayer
    {
        private const string LogTag = "ImpossibleGarden";

        private const int Segments = 48;
        private const int PetalSegments = 8;

        private readonly BFImpossibleGardenLayerConfig config;
        private readonly List<BFImpossibleGardenBloom> blooms =
            new List<BFImpossibleGardenBloom>();

        private readonly List<LineRenderer> renderers =
            new List<LineRenderer>();

        private Transform root;
        private Material material;

        private float elapsedTime;
        private int lastGardenCount = -1;
        private int lastPetalCount = -1;
        private int sortingOrder;

        public BFImpossibleGardenLayer(
            BFImpossibleGardenLayerConfig config)
        {
            this.config = config;
        }

        public void Init(
            Transform parent,
            int sortingOrder)
        {
            elapsedTime = 0f;
            this.sortingOrder = sortingOrder;

            GameObject rootObject =
                new GameObject("BFImpossibleGardenLayer");

            rootObject.transform.SetParent(parent, false);
            rootObject.layer =
                BFBackgroundStackManager.BackgroundLayer;

            root = rootObject.transform;

            material = CreateMaterial();

            RebuildGardens();

            BFLogger.Info(
                LogTag,
                $"BFImpossibleGardenLayer: initialized with {blooms.Count} bloom(s).");
        }

        public void Tick(float dt)
        {
            if (root == null)
                return;

            elapsedTime += dt;

            int gardenCount =
                Mathf.Max(1, config.GardenCount);

            int petalCount =
                Mathf.Max(3, config.PetalsPerGarden);

            if (gardenCount != lastGardenCount ||
                petalCount != lastPetalCount)
            {
                RebuildGardens();
            }

            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);

            for (int i = 0; i < blooms.Count; i++)
            {
                BFImpossibleGardenBloom bloom = blooms[i];

                bloom.Refresh(
                    elapsedTime,
                    width,
                    height);

                LineRenderer renderer = renderers[i];

                UpdateBloomRenderer(
                    renderer,
                    bloom,
                    width,
                    height,
                    i);
            }
        }

        public void Cleanup()
        {
            if (root != null)
                DestroyObject(root.gameObject);

            root = null;

            if (material != null)
                DestroyObject(material);

            material = null;

            blooms.Clear();
            renderers.Clear();
        }

        private void RebuildGardens()
        {
            ClearGardens();

            int count =
                Mathf.Max(1, config.GardenCount);

            int petalCount =
                Mathf.Max(3, config.PetalsPerGarden);

            lastGardenCount = count;
            lastPetalCount = petalCount;

            for (int i = 0; i < count; i++)
            {
                Vector2 position =
                    GenerateGardenPosition(
                        i,
                        count);

                BFImpossibleGardenBloom bloom =
                    new BFImpossibleGardenBloom(
                        i,
                        config,
                        position);

                blooms.Add(bloom);

                GameObject objectRoot =
                    new GameObject(
                        $"Bloom_{i}");

                objectRoot.transform.SetParent(
                    root,
                    false);

                objectRoot.layer =
                    BFBackgroundStackManager.BackgroundLayer;

                LineRenderer renderer =
                    objectRoot.AddComponent<LineRenderer>();

                renderer.useWorldSpace = true;
                renderer.loop = true;
                renderer.positionCount =
                    Segments;

                renderer.material = material;

                renderer.textureMode =
                    LineTextureMode.Stretch;

                renderer.alignment =
                    LineAlignment.View;

                renderer.numCapVertices = 2;
                renderer.numCornerVertices = 2;

                renderer.sortingOrder =
                    sortingOrder;

                renderers.Add(renderer);
            }
        }

        private void ClearGardens()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                    DestroyObject(
                        renderers[i].gameObject);
            }

            renderers.Clear();
            blooms.Clear();
        }

        private void UpdateBloomRenderer(
            LineRenderer renderer,
            BFImpossibleGardenBloom bloom,
            float width,
            float height,
            int index)
        {
            Vector2 center =
                new Vector2(
                    bloom.Position.x * width,
                    bloom.Position.y * height);

            float radius =
                bloom.Radius *
                Mathf.Min(width, height);

            float breathing =
                1f +
                Mathf.Sin(
                    elapsedTime *
                    config.BreathingSpeed +
                    index * 1.37f) *
                config.BreathingAmount;

            radius *= breathing;

            Color color = bloom.Color;
            color.a = bloom.Alpha;

            renderer.startColor = color;
            renderer.endColor = color;

            float widthMultiplier =
                Mathf.Max(
                    0.5f,
                    config.PetalWidth);

            renderer.startWidth =
                widthMultiplier;

            renderer.endWidth =
                widthMultiplier;

            float rotation =
                elapsedTime *
                config.RotationSpeed +
                index * 47.123f;

            float distortion =
                config.MembraneDistortion;

            for (int i = 0; i < Segments; i++)
            {
                float t =
                    i / (float)Segments;

                float angle =
                    t * Mathf.PI * 2f +
                    rotation * Mathf.Deg2Rad;

                float petalWave =
                    Mathf.Sin(
                        angle *
                        config.PetalsPerGarden);

                float secondary =
                    Mathf.Sin(
                        angle *
                        (config.PetalsPerGarden * 2f) +
                        elapsedTime *
                        config.BreathingSpeed);

                float petalShape =
                    1f +
                    petalWave *
                    config.PetalCurl *
                    0.35f;

                float membrane =
                    secondary *
                    distortion *
                    0.15f;

                float finalRadius =
                    radius *
                    petalShape *
                    (1f + membrane);

                Vector3 position =
                    new Vector3(
                        center.x +
                        Mathf.Cos(angle) *
                        finalRadius,
                        center.y +
                        Mathf.Sin(angle) *
                        finalRadius,
                        0f);

                renderer.SetPosition(
                    i,
                    position);
            }
        }

        private Vector2 GenerateGardenPosition(
            int index,
            int count)
        {
            float goldenAngle =
                Mathf.PI *
                (3f - Mathf.Sqrt(5f));

            float normalized =
                (index + 0.5f) /
                Mathf.Max(1, count);

            float radius =
                Mathf.Sqrt(normalized) *
                config.GardenRadius;

            float angle =
                index *
                goldenAngle;

            float spread =
                config.GardenSpread;

            float x =
                0.5f +
                Mathf.Cos(angle) *
                radius +
                HashSigned(index * 13 + 7) *
                spread *
                0.25f;

            float y =
                0.5f +
                Mathf.Sin(angle) *
                radius +
                HashSigned(index * 29 + 3) *
                spread *
                0.25f;

            return new Vector2(
                Mathf.Clamp01(x),
                Mathf.Clamp01(y));
        }

        private static float HashSigned(int value)
        {
            unchecked
            {
                uint x = (uint)value;

                x ^= x >> 16;
                x *= 0x7feb352d;
                x ^= x >> 15;
                x *= 0x846ca68b;
                x ^= x >> 16;

                return
                    (x / (float)uint.MaxValue) *
                    2f -
                    1f;
            }
        }

        private static Material CreateMaterial()
        {
            Shader shader =
                Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            if (shader == null)
                return new Material(Shader.Find("Standard"));

            Material result =
                new Material(shader);

            result.name =
                "BFImpossibleGarden_Runtime";

            return result;
        }

        private static void DestroyObject(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }
}