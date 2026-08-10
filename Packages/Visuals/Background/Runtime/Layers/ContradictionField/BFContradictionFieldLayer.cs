using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFContradictionFieldLayer
        : IBFBackgroundLayer
    {
        private static readonly string[] LogTags =
        {
            "Background",
            "ContradictionField"
        };

        private readonly BFContradictionFieldLayerConfig config;

        private readonly List<Claim> claims =
            new List<Claim>();

        private readonly List<LineRenderer> renderers =
            new List<LineRenderer>();

        private readonly List<MemoryScar> memories =
            new List<MemoryScar>();

        private Transform root;
        private Material material;

        private float time;
        private int lastClaimCount = -1;
        private int lastResolution = -1;

        public BFContradictionFieldLayer(
            BFContradictionFieldLayerConfig config)
        {
            this.config = config;
        }

        public void Init(
            Transform parent,
            int sortingOrder)
        {
            time = 0f;

            GameObject rootObject =
                new GameObject(
                    "BFContradictionFieldLayer");

            rootObject.transform.SetParent(
                parent,
                false);

            rootObject.layer =
                BFBackgroundStackManager.BackgroundLayer;

            root = rootObject.transform;

            material = CreateMaterial();

            SyncStructure(sortingOrder);

            BFLogger.Info(
                LogTags,
                $"BFContradictionFieldLayer: initialized with {config.ClaimCount} claims.");
        }

        public void Tick(float dt)
        {
            SyncStructure();

            time += dt * config.Speed;

            UpdateMemories(dt);

            float width = Screen.width;
            float height = Screen.height;

            Vector2 center =
                new Vector2(width, height) * 0.5f;

            /*
             * First determine where the claims currently
             * disagree with one another.
             */
            float[] contradictionEnergy =
                CalculateContradictions(
                    width,
                    height);

            /*
             * Contradictions are allowed to create memory.
             */
            WriteMemories(
                contradictionEnergy,
                width,
                height,
                dt);

            for (int i = 0;
                 i < claims.Count;
                 i++)
            {
                Claim claim = claims[i];

                LineRenderer line =
                    renderers[i];

                float energy =
                    contradictionEnergy[i];

                Color color =
                    config.ColorGradient.Evaluate(
                        claim.ColorPosition);

                float brightness =
                    1f +
                    energy *
                    config.ContradictionGlow;

                color *= brightness;
                color.a = config.Opacity;

                line.startColor = color;
                line.endColor = color;

                float widthPixels =
                    Mathf.Max(
                        0.5f,
                        config.Thickness *
                        Mathf.Max(width, height) *
                        claim.WidthMultiplier);

                line.startWidth =
                    widthPixels;

                line.endWidth =
                    widthPixels;

                for (int s = 0;
                     s < config.Resolution;
                     s++)
                {
                    float normalized =
                        s /
                        (float)(config.Resolution - 1);

                    Vector2 point =
                        EvaluateClaim(
                            claim,
                            normalized,
                            center,
                            width,
                            height,
                            contradictionEnergy);

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
            if (root != null)
                DestroyObject(root.gameObject);

            root = null;

            if (material != null)
                DestroyObject(material);

            material = null;

            claims.Clear();
            renderers.Clear();
            memories.Clear();
        }

        private void SyncStructure(
            int sortingOrder = -1)
        {
            bool claimCountChanged =
                lastClaimCount != config.ClaimCount;

            bool resolutionChanged =
                lastResolution != config.Resolution;

            if (claimCountChanged)
            {
                SyncClaimCount(
                    sortingOrder);
            }

            if (resolutionChanged)
            {
                int resolution =
                    Mathf.Max(
                        2,
                        config.Resolution);

                for (int i = 0;
                     i < renderers.Count;
                     i++)
                {
                    renderers[i].positionCount =
                        resolution;
                }
            }

            lastClaimCount =
                config.ClaimCount;

            lastResolution =
                config.Resolution;
        }

        private void SyncClaimCount(
            int sortingOrder)
        {
            int target =
                Mathf.Max(
                    0,
                    config.ClaimCount);

            while (claims.Count < target)
            {
                int index =
                    claims.Count;

                Claim claim =
                    new Claim(index);

                claims.Add(claim);

                GameObject objectRoot =
                    new GameObject(
                        $"Claim_{index}");

                objectRoot.transform.SetParent(
                    root,
                    false);

                objectRoot.layer =
                    BFBackgroundStackManager.BackgroundLayer;

                LineRenderer renderer =
                    objectRoot.AddComponent<LineRenderer>();

                renderer.positionCount =
                    Mathf.Max(
                        2,
                        config.Resolution);

                renderer.useWorldSpace = true;
                renderer.material = material;

                renderer.textureMode =
                    LineTextureMode.Stretch;

                renderer.numCapVertices = 3;
                renderer.numCornerVertices = 3;

                if (sortingOrder >= 0)
                    renderer.sortingOrder =
                        sortingOrder;

                renderers.Add(renderer);
            }

            while (claims.Count > target)
            {
                int index =
                    claims.Count - 1;

                DestroyObject(
                    renderers[index].gameObject);

                renderers.RemoveAt(index);
                claims.RemoveAt(index);
            }
        }

        private float[] CalculateContradictions(
            float width,
            float height)
        {
            float[] energy =
                new float[claims.Count];

            if (claims.Count < 2)
                return energy;

            /*
             * We don't actually need to build the final
             * geometry here.
             *
             * Instead, sample a coarse mathematical
             * representation of each claim.
             */
            const int Samples = 18;

            for (int a = 0;
                 a < claims.Count;
                 a++)
            {
                for (int b = a + 1;
                     b < claims.Count;
                     b++)
                {
                    float disagreement = 0f;

                    for (int s = 0;
                         s < Samples;
                         s++)
                    {
                        float t =
                            s /
                            (float)(Samples - 1);

                        Vector2 pA =
                            EvaluateClaimNormalized(
                                claims[a],
                                t);

                        Vector2 pB =
                            EvaluateClaimNormalized(
                                claims[b],
                                t);

                        float distance =
                            Vector2.Distance(
                                pA,
                                pB);

                        float normalizedDistance =
                            distance /
                            Mathf.Max(
                                0.001f,
                                config.ContradictionRadius);

                        float local =
                            Mathf.Exp(
                                -Mathf.Pow(
                                    normalizedDistance,
                                    config.ContradictionSharpness));

                        disagreement += local;
                    }

                    disagreement /=
                        Samples;

                    /*
                     * The two claims contribute equally.
                     */
                    energy[a] += disagreement;
                    energy[b] += disagreement;
                }
            }

            float divisor =
                Mathf.Max(
                    1f,
                    claims.Count - 1);

            for (int i = 0;
                 i < energy.Length;
                 i++)
            {
                energy[i] =
                    Mathf.Clamp01(
                        energy[i] /
                        divisor);
            }

            return energy;
        }

        private Vector2 EvaluateClaim(
            Claim claim,
            float t,
            Vector2 center,
            float width,
            float height,
            float[] contradictionEnergy)
        {
            Vector2 normalized =
                EvaluateClaimNormalized(
                    claim,
                    t);

            float contradiction =
                contradictionEnergy[claim.Index];

            /*
             * Contradiction bends the claim toward
             * a direction determined by its identity.
             */
            float disagreementPhase =
                time *
                config.DisagreementSpeed +
                claim.Phase;

            float direction =
                Mathf.Sin(
                    disagreementPhase);

            float perpendicular =
                Mathf.Cos(
                    disagreementPhase *
                    1.37f);

            float bend =
                contradiction *
                config.ContradictionStrength *
                0.25f;

            normalized.x +=
                direction * bend;

            normalized.y +=
                perpendicular * bend;

            /*
             * A claim is also forbidden from perfectly
             * repeating its own old shape.
             */
            float selfConflict =
                Mathf.Sin(
                    t *
                    Mathf.PI *
                    4f +
                    time *
                    0.31f +
                    claim.Phase);

            normalized.y +=
                selfConflict *
                config.SelfContradiction *
                0.08f;

            /*
             * Old contradictions leave scars.
             */
            ApplyMemory(
                ref normalized,
                claim);

            /*
             * Global rotational bias.
             */
            float angle =
                time *
                config.RotationalBias;

            float x =
                normalized.x;

            float y =
                normalized.y;

            normalized.x =
                x * Mathf.Cos(angle) -
                y * Mathf.Sin(angle);

            normalized.y =
                x * Mathf.Sin(angle) +
                y * Mathf.Cos(angle);

            return new Vector2(
                center.x +
                normalized.x *
                width *
                0.72f,

                center.y +
                normalized.y *
                height *
                0.72f);
        }

        private Vector2 EvaluateClaimNormalized(
            Claim claim,
            float t)
        {
            float x =
                Mathf.Lerp(
                    -1.25f,
                    1.25f,
                    t);

            float phase =
                claim.Phase;

            /*
             * Every claim starts with a different
             * mathematical sentence.
             */
            float sentence =
                Mathf.Sin(
                    x *
                    claim.Frequency +
                    phase +
                    time *
                    claim.Speed);

            float clause =
                Mathf.Sin(
                    x *
                    claim.Frequency *
                    1.91f -
                    phase *
                    0.73f +
                    time *
                    claim.Speed *
                    0.47f);

            float qualification =
                Mathf.Sin(
                    x *
                    claim.Frequency *
                    3.17f +
                    phase *
                    1.4f -
                    time *
                    0.19f);

            float y =
                sentence *
                config.Curvature *
                0.32f;

            y +=
                clause *
                0.11f;

            y +=
                qualification *
                0.035f;

            /*
             * Give each claim its own logical "territory".
             */
            y +=
                claim.BaseOffset;

            return new Vector2(
                x,
                y);
        }

        private void WriteMemories(
            float[] contradictionEnergy,
            float width,
            float height,
            float dt)
        {
            for (int i = 0;
                 i < contradictionEnergy.Length;
                 i++)
            {
                float energy =
                    contradictionEnergy[i];

                if (energy < 0.65f)
                    continue;

                Claim claim =
                    claims[i];

                /*
                 * The location is deterministic but
                 * slowly changes with time.
                 */
                float x =
                    Mathf.Sin(
                        time *
                        0.17f +
                        claim.Phase);

                float y =
                    Mathf.Cos(
                        time *
                        0.13f +
                        claim.Phase *
                        1.7f);

                float strength =
                    energy *
                    config.MemoryWriteStrength;

                memories.Add(
                    new MemoryScar
                    {
                        Position =
                            new Vector2(x, y),

                        Strength =
                            strength,

                        Phase =
                            claim.Phase,

                        Age = 0f
                    });
            }

            /*
             * Prevent the memory list from growing forever.
             */
            const int MaximumMemories = 48;

            while (memories.Count >
                   MaximumMemories)
            {
                memories.RemoveAt(0);
            }
        }

        private void UpdateMemories(
            float dt)
        {
            for (int i =
                     memories.Count - 1;
                 i >= 0;
                 i--)
            {
                MemoryScar memory =
                    memories[i];

                memory.Age += dt;

                memory.Strength =
                    Mathf.MoveTowards(
                        memory.Strength,
                        0f,
                        config.MemoryDecay *
                        dt);

                memories[i] = memory;

                if (memory.Strength <=
                    0.001f)
                {
                    memories.RemoveAt(i);
                }
            }
        }

        private void ApplyMemory(
            ref Vector2 position,
            Claim claim)
        {
            if (memories.Count == 0)
                return;

            for (int i = 0;
                 i < memories.Count;
                 i++)
            {
                MemoryScar memory =
                    memories[i];

                Vector2 delta =
                    position -
                    memory.Position;

                float distance =
                    delta.magnitude;

                float influence =
                    Mathf.Exp(
                        -distance *
                        3.5f);

                float angle =
                    memory.Phase +
                    claim.Phase;

                Vector2 memoryDirection =
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle));

                position +=
                    memoryDirection *
                    influence *
                    memory.Strength *
                    config.MemoryInfluence *
                    0.04f;
            }
        }

        private static Material CreateMaterial()
        {
            Shader shader =
                Shader.Find(
                    "Legacy Shaders/Particles/Additive");

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Sprites/Default");
            }

            return new Material(shader)
            {
                name =
                    "BFContradictionField_Material"
            };
        }

        private static void DestroyObject(
            Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        private sealed class Claim
        {
            public readonly int Index;

            public readonly float Phase;
            public readonly float Frequency;
            public readonly float Speed;
            public readonly float BaseOffset;
            public readonly float WidthMultiplier;
            public readonly float ColorPosition;

            public Claim(int index)
            {
                Index = index;

                Phase =
                    Random.Range(
                        0f,
                        Mathf.PI * 2f);

                Frequency =
                    Random.Range(
                        2.2f,
                        5.5f);

                Speed =
                    Random.Range(
                        0.65f,
                        1.35f);

                BaseOffset =
                    Mathf.Lerp(
                        -0.55f,
                        0.55f,
                        Random.Range(
                            0f,
                            1f));

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

        private struct MemoryScar
        {
            public Vector2 Position;
            public float Strength;
            public float Phase;
            public float Age;
        }
    }
}