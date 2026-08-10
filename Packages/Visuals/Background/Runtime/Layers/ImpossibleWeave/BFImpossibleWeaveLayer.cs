using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFImpossibleWeaveLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags =
        {
            "Background",
            "ImpossibleWeave"
        };

        private readonly BFImpossibleWeaveLayerConfig config;

        private readonly List<LineRenderer> renderers =
            new List<LineRenderer>();

        private readonly List<Weave> weaves =
            new List<Weave>();

        private Transform root;
        private Material material;

        private float time;
        private float rotation;

        // Cached structural configuration.
        private int lastSeamCount = -1;
        private int lastResolution = -1;

        public BFImpossibleWeaveLayer(
            BFImpossibleWeaveLayerConfig config)
        {
            this.config = config;
        }

        public void Init(
            Transform parent,
            int sortingOrder)
        {
            time = 0f;
            rotation = 0f;

            lastSeamCount = -1;
            lastResolution = -1;

            GameObject rootObject =
                new GameObject("BFImpossibleWeaveLayer");

            rootObject.transform.SetParent(parent, false);
            rootObject.layer =
                BFBackgroundStackManager.BackgroundLayer;

            root = rootObject.transform;

            material = CreateMaterial();

            SyncConfiguration(sortingOrder);

            BFLogger.Info(
                LogTags,
                $"BFImpossibleWeaveLayer: initialized with {config.SeamCount} seams.");
        }

        public void Tick(float dt)
        {
            /*
             * Structural values such as SeamCount and Resolution
             * can be changed while the layer is alive.
             *
             * SyncConfiguration() only rebuilds the affected
             * objects when those values actually change.
             */
            SyncConfiguration();

            time += dt * config.FoldSpeed;
            rotation += dt * config.RotationSpeed;

            float width = Screen.width;
            float height = Screen.height;

            float aspect =
                width / Mathf.Max(1f, height);

            Vector2 center =
                new Vector2(width, height) * 0.5f;

            float breathing =
                1f +
                Mathf.Sin(
                    time *
                    Mathf.PI *
                    2f *
                    config.BreathingSpeed) *
                config.BreathingAmount;

            for (int i = 0;
                 i < weaves.Count;
                 i++)
            {
                Weave weave = weaves[i];
                LineRenderer line = renderers[i];

                Color color =
                    config.ColorGradient.Evaluate(
                        weave.ColorPosition);

                float intersection =
                    CalculateIntersectionEnergy(
                        weave,
                        time);

                float brightness =
                    1f +
                    intersection *
                    config.IntersectionGlow;

                color *= brightness;
                color.a = config.Opacity;

                line.startColor = color;
                line.endColor = color;

                float widthPixels =
                    Mathf.Max(
                        0.5f,
                        config.Thickness *
                        Mathf.Max(width, height) *
                        weave.WidthMultiplier);

                line.startWidth = widthPixels;
                line.endWidth = widthPixels;

                for (int s = 0;
                     s < config.Resolution;
                     s++)
                {
                    float t =
                        s /
                        (float)(config.Resolution - 1);

                    Vector2 point =
                        EvaluateWeave(
                            weave,
                            t,
                            center,
                            width,
                            height,
                            aspect,
                            breathing);

                    line.SetPosition(
                        s,
                        new Vector3(
                            point.x,
                            point.y,
                            0f));
                }
            }
        }

        public void Cleanup()
        {
            BFLogger.Info(
                LogTags,
                "BFImpossibleWeaveLayer: cleaning up.");

            if (root != null)
                DestroyObject(root.gameObject);

            root = null;

            if (material != null)
                DestroyObject(material);

            material = null;

            renderers.Clear();
            weaves.Clear();

            lastSeamCount = -1;
            lastResolution = -1;
        }

        private void SyncConfiguration(
            int sortingOrder = -1)
        {
            bool seamCountChanged =
                lastSeamCount != config.SeamCount;

            bool resolutionChanged =
                lastResolution != config.Resolution;

            if (!seamCountChanged &&
                !resolutionChanged)
            {
                return;
            }

            /*
             * Seam count changes require the actual
             * LineRenderer / Weave objects to change.
             */
            if (seamCountChanged)
            {
                SyncSeamCount(sortingOrder);
            }

            /*
             * Resolution only requires changing the
             * LineRenderer point counts.
             */
            if (resolutionChanged)
            {
                SyncResolution();
            }

            lastSeamCount = config.SeamCount;
            lastResolution = config.Resolution;
        }

        private void SyncSeamCount(int sortingOrder)
        {
            int targetCount =
                Mathf.Max(0, config.SeamCount);

            /*
             * Grow.
             */
            while (weaves.Count < targetCount)
            {
                int index = weaves.Count;

                Weave weave =
                    new Weave(
                        index,
                        targetCount);

                weaves.Add(weave);

                GameObject objectRoot =
                    new GameObject(
                        $"Weave_{index}");

                objectRoot.transform.SetParent(
                    root,
                    false);

                objectRoot.layer =
                    BFBackgroundStackManager.BackgroundLayer;

                LineRenderer renderer =
                    objectRoot.AddComponent<LineRenderer>();

                renderer.positionCount =
                    config.Resolution;

                renderer.useWorldSpace = true;
                renderer.material = material;

                renderer.textureMode =
                    LineTextureMode.Stretch;

                renderer.numCapVertices = 4;
                renderer.numCornerVertices = 4;

                if (sortingOrder >= 0)
                    renderer.sortingOrder =
                        sortingOrder;

                renderers.Add(renderer);
            }

            /*
             * Shrink.
             */
            while (weaves.Count > targetCount)
            {
                int index =
                    weaves.Count - 1;

                LineRenderer renderer =
                    renderers[index];

                if (renderer != null)
                {
                    DestroyObject(
                        renderer.gameObject);
                }

                renderers.RemoveAt(index);
                weaves.RemoveAt(index);
            }
        }

        private void SyncResolution()
        {
            int resolution =
                Mathf.Max(2, config.Resolution);

            for (int i = 0;
                 i < renderers.Count;
                 i++)
            {
                renderers[i].positionCount =
                    resolution;
            }
        }

        private Vector2 EvaluateWeave(
            Weave weave,
            float t,
            Vector2 center,
            float width,
            float height,
            float aspect,
            float breathing)
        {
            float x =
                Mathf.Lerp(-1.4f, 1.4f, t);

            float seed =
                weave.Seed;

            float phase =
                weave.Phase;

            float foldTime =
                time * config.FoldSpeed +
                seed * Mathf.PI * 2f;

            float deformationTime =
                time * config.DeformationSpeed +
                seed * 11.7f;

            float primaryFold =
                Mathf.Sin(
                    x *
                    config.FoldFrequency *
                    Mathf.PI *
                    2f +
                    foldTime);

            float secondaryFold =
                Mathf.Sin(
                    x *
                    config.FoldFrequency *
                    2.37f *
                    Mathf.PI *
                    2f +
                    deformationTime +
                    phase);

            float tertiaryFold =
                Mathf.Sin(
                    x *
                    5.17f +
                    deformationTime *
                    0.73f +
                    phase * 2f);

            float displacement =
                primaryFold *
                config.Curvature;

            displacement +=
                secondaryFold *
                config.SecondaryDeformation *
                0.35f;

            displacement +=
                tertiaryFold *
                0.08f;

            displacement +=
                weave.VerticalOffset *
                0.65f;

            float foldField =
                Mathf.Cos(
                    x *
                    config.FoldFrequency *
                    Mathf.PI *
                    2f +
                    foldTime);

            float compression =
                1f -
                Mathf.Abs(foldField) *
                config.PinchStrength *
                0.35f;

            displacement *= compression;

            float localFold =
                Mathf.Sin(
                    x *
                    Mathf.PI *
                    2f *
                    1.71f +
                    foldTime *
                    0.63f +
                    phase);

            float attraction =
                Mathf.Sign(localFold) *
                Mathf.Pow(
                    Mathf.Abs(localFold),
                    3f);

            displacement +=
                attraction *
                config.FoldStrength *
                0.18f;

            float rotatedX =
                x * Mathf.Cos(rotation) -
                displacement * Mathf.Sin(rotation);

            float rotatedY =
                x * Mathf.Sin(rotation) +
                displacement * Mathf.Cos(rotation);

            rotatedX *= breathing;
            rotatedY *= breathing;

            float px =
                center.x +
                rotatedX *
                width *
                0.62f;

            float py =
                center.y +
                rotatedY *
                height *
                0.42f;

            px =
                center.x +
                (px - center.x) /
                Mathf.Max(0.001f, aspect);

            return new Vector2(px, py);
        }

        private float CalculateIntersectionEnergy(
            Weave weave,
            float currentTime)
        {
            float phase =
                currentTime *
                config.IntersectionMotion +
                weave.Phase;

            float a =
                Mathf.Sin(
                    phase +
                    weave.Seed * 7.3f);

            float b =
                Mathf.Sin(
                    phase * 1.73f +
                    weave.Seed * 3.1f);

            float energy =
                Mathf.Abs(a * b);

            return Mathf.SmoothStep(
                0f,
                1f,
                energy);
        }

        private static Material CreateMaterial()
        {
            Shader shader =
                Shader.Find(
                    "Legacy Shaders/Particles/Additive");

            if (shader == null)
            {
                shader =
                    Shader.Find("Sprites/Default");
            }

            return new Material(shader)
            {
                name =
                    "BFImpossibleWeave_Material"
            };
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

        private sealed class Weave
        {
            public readonly float Seed;
            public readonly float Phase;
            public readonly float VerticalOffset;
            public readonly float WidthMultiplier;
            public readonly float ColorPosition;

            public Weave(
                int index,
                int count)
            {
                Seed =
                    Random.Range(0f, 1f);

                Phase =
                    Random.Range(
                        0f,
                        Mathf.PI * 2f);

                float normalized =
                    count <= 1
                        ? 0.5f
                        : index /
                          (float)(count - 1);

                VerticalOffset =
                    Mathf.Lerp(
                        -1f,
                        1f,
                        normalized);

                WidthMultiplier =
                    Random.Range(
                        0.65f,
                        1.35f);

                ColorPosition =
                    Random.Range(
                        0.05f,
                        0.95f);
            }
        }
    }
}