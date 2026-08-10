using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTopologyErrorLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags =
        {
            "Background",
            "TopologyError"
        };

        private const int MaxTerritories = 16;

        private readonly BFTopologyErrorLayerConfig config;

        private Transform root;
        private GameObject spriteObject;
        private SpriteRenderer spriteRenderer;
        private Texture2D texture;
        private Sprite sprite;

        private int width;
        private int height;

        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        private int lastResolutionX = -1;
        private int lastResolutionY = -1;
        private int lastTerritoryCount = -1;

        private int[] owners;
        private int[] targetOwners;
        private Vector2[] seeds;
        private Color[] territoryColors;

        private float elapsedTime;
        private float mutationTimer;
        private float mutationProgress;

        private bool mutationActive;

        private Vector2 mutationCenter;
        private float mutationRadius;

        private int sortingOrder;

        public BFTopologyErrorLayer(
            BFTopologyErrorLayerConfig config)
        {
            this.config = config;
        }

        public void Init(
            Transform parent,
            int sortingOrder)
        {
            this.sortingOrder = sortingOrder;

            elapsedTime = 0f;
            mutationTimer = 0f;
            mutationProgress = 0f;
            mutationActive = false;

            CreateRoot(parent);
            CreateTexture();
            CreateSprite();
            CreateTopology();

            Render();

            BFLogger.Info(
                LogTags,
                "BFTopologyErrorLayer: initialized.");
        }

        public void Tick(float dt)
        {
            if (texture == null ||
                spriteRenderer == null)
            {
                return;
            }

            elapsedTime += dt;

            bool screenChanged =
                Screen.width != lastScreenWidth ||
                Screen.height != lastScreenHeight;

            bool topologyChanged =
                config.ResolutionX != lastResolutionX ||
                config.ResolutionY != lastResolutionY ||
                config.TerritoryCount != lastTerritoryCount;

            if (screenChanged)
            {
                CreateTexture();
                CreateSprite();
            }

            if (topologyChanged)
            {
                CreateTopology();
            }

            UpdateMutation(dt);
            Render();
        }

        public void Cleanup()
        {
            if (root != null)
            {
                DestroyObject(root.gameObject);
            }

            root = null;
            spriteObject = null;
            spriteRenderer = null;

            if (sprite != null)
            {
                DestroyObject(sprite);
            }

            sprite = null;

            if (texture != null)
            {
                DestroyObject(texture);
            }

            texture = null;

            owners = null;
            targetOwners = null;
            seeds = null;
            territoryColors = null;
        }

        private void CreateRoot(
            Transform parent)
        {
            GameObject obj =
                new GameObject(
                    "BFTopologyErrorLayer");

            obj.transform.SetParent(
                parent,
                false);

            /*
             * The BFBackgroundStackCamera uses a pixel-sized
             * orthographic coordinate system:
             *
             *     left   = 0
             *     right  = Screen.width
             *     bottom = 0
             *     top    = Screen.height
             *
             * The stack root itself is already placed at Z = 10
             * by convention.
             */
            obj.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    0f);

            obj.transform.localRotation =
                Quaternion.identity;

            obj.transform.localScale =
                Vector3.one;

            obj.layer =
                BFBackgroundStackManager.BackgroundLayer;

            root =
                obj.transform;
        }

        private void CreateTexture()
        {
            width =
                Mathf.Clamp(
                    config.ResolutionX,
                    16,
                    256);

            height =
                Mathf.Clamp(
                    config.ResolutionY,
                    9,
                    144);

            if (texture != null)
            {
                DestroyObject(texture);
            }

            texture =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false);

            texture.name =
                "BFTopologyErrorLayer_Texture";

            texture.wrapMode =
                TextureWrapMode.Clamp;

            texture.filterMode =
                FilterMode.Bilinear;

            texture.anisoLevel = 0;

            lastScreenWidth =
                Screen.width;

            lastScreenHeight =
                Screen.height;
        }

        private void CreateSprite()
        {
            if (spriteObject == null)
            {
                spriteObject =
                    new GameObject(
                        "TopologyField");

                spriteObject.transform.SetParent(
                    root,
                    false);

                spriteObject.layer =
                    BFBackgroundStackManager.BackgroundLayer;

                spriteRenderer =
                    spriteObject.AddComponent<SpriteRenderer>();
            }

            if (sprite != null)
            {
                DestroyObject(sprite);
            }

            sprite =
                Sprite.Create(
                    texture,
                    new Rect(
                        0f,
                        0f,
                        texture.width,
                        texture.height),
                    new Vector2(
                        0.5f,
                        0.5f),
                    1f);

            sprite.name =
                "BFTopologyErrorLayer_Sprite";

            spriteRenderer.sprite =
                sprite;

            spriteRenderer.sortingOrder =
                sortingOrder;

            spriteRenderer.color =
                Color.white;

            spriteRenderer.enabled =
                true;

            FitToBackgroundCamera();
        }

        private void FitToBackgroundCamera()
        {
            if (spriteObject == null ||
                sprite == null)
            {
                return;
            }

            /*
             * Do NOT use Camera.main here.
             *
             * The background stack intentionally has its own camera.
             * Its world-space viewport is exactly:
             *
             *     0 .. Screen.width
             *     0 .. Screen.height
             *
             * Since the sprite is created at 1 pixel per world unit,
             * its native dimensions are texture.width x texture.height.
             *
             * Scale it directly to the background camera's viewport.
             */
            float scaleX =
                Screen.width /
                Mathf.Max(
                    0.0001f,
                    sprite.bounds.size.x);

            float scaleY =
                Screen.height /
                Mathf.Max(
                    0.0001f,
                    sprite.bounds.size.y);

            spriteObject.transform.localPosition =
                new Vector3(
                    Screen.width * 0.5f,
                    Screen.height * 0.5f,
                    0f);

            spriteObject.transform.localRotation =
                Quaternion.identity;

            spriteObject.transform.localScale =
                new Vector3(
                    scaleX,
                    scaleY,
                    1f);
        }

        private void CreateTopology()
        {
            int count =
                Mathf.Clamp(
                    config.TerritoryCount,
                    3,
                    MaxTerritories);

            int total =
                width * height;

            owners =
                new int[total];

            targetOwners =
                new int[total];

            seeds =
                new Vector2[count];

            territoryColors =
                new Color[count];

            for (int i = 0;
                 i < count;
                 i++)
            {
                seeds[i] =
                    new Vector2(
                        Random.Range(
                            0.05f,
                            0.95f),

                        Random.Range(
                            0.05f,
                            0.95f));

                float colorT =
                    i /
                    Mathf.Max(
                        1f,
                        count - 1f);

                territoryColors[i] =
                    config.ColorGradient.Evaluate(
                        colorT);
            }

            BuildOwnership();

            System.Array.Copy(
                owners,
                targetOwners,
                owners.Length);

            lastResolutionX =
                config.ResolutionX;

            lastResolutionY =
                config.ResolutionY;

            lastTerritoryCount =
                config.TerritoryCount;

            mutationActive = false;
            mutationTimer = 0f;
            mutationProgress = 0f;
        }

        private void BuildOwnership()
        {
            for (int y = 0;
                 y < height;
                 y++)
            {
                for (int x = 0;
                     x < width;
                     x++)
                {
                    int index =
                        y * width + x;

                    float px =
                        (x + 0.5f) /
                        width;

                    float py =
                        (y + 0.5f) /
                        height;

                    float bestDistance =
                        float.MaxValue;

                    int best =
                        0;

                    for (int i = 0;
                         i < seeds.Length;
                         i++)
                    {
                        Vector2 animatedSeed =
                            GetAnimatedSeed(i);

                        float dx =
                            px -
                            animatedSeed.x;

                        float dy =
                            py -
                            animatedSeed.y;

                        float distance =
                            dx * dx +
                            dy * dy;

                        if (distance <
                            bestDistance)
                        {
                            bestDistance =
                                distance;

                            best =
                                i;
                        }
                    }

                    owners[index] =
                        best;
                }
            }
        }

        private Vector2 GetAnimatedSeed(
            int index)
        {
            Vector2 seed =
                seeds[index];

            float time =
                elapsedTime *
                config.DriftSpeed;

            float phase =
                index *
                2.173f;

            float x =
                Mathf.Sin(
                    time * 0.73f +
                    phase);

            float y =
                Mathf.Cos(
                    time * 0.61f +
                    phase * 1.37f);

            return seed +
                   new Vector2(
                       x,
                       y) *
                   config.DriftAmount *
                   0.08f;
        }

        private void UpdateMutation(
            float dt)
        {
            if (!mutationActive)
            {
                mutationTimer += dt;

                if (mutationTimer >=
                    Mathf.Max(
                        0.05f,
                        config.MutationInterval))
                {
                    BeginMutation();
                }

                return;
            }

            mutationProgress +=
                dt /
                Mathf.Max(
                    0.05f,
                    config.MutationDuration);

            float t =
                Mathf.Clamp01(
                    mutationProgress);

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t);

            float inverseRadius =
                1f /
                Mathf.Max(
                    0.001f,
                    mutationRadius);

            for (int i = 0;
                 i < owners.Length;
                 i++)
            {
                if (owners[i] ==
                    targetOwners[i])
                {
                    continue;
                }

                GetPixelNormalizedPosition(
                    i,
                    out float px,
                    out float py);

                float dx =
                    px -
                    mutationCenter.x;

                float dy =
                    py -
                    mutationCenter.y;

                float distance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy);

                float localInfluence =
                    Mathf.Clamp01(
                        1f -
                        distance *
                        inverseRadius);

                float probability =
                    eased *
                    localInfluence *
                    config.MutationStrength;

                if (StableRandom(i) <
                    probability)
                {
                    owners[i] =
                        targetOwners[i];
                }
            }

            if (t >= 1f)
            {
                System.Array.Copy(
                    targetOwners,
                    owners,
                    owners.Length);

                mutationActive = false;
                mutationTimer = 0f;
                mutationProgress = 0f;
            }
        }

        private void BeginMutation()
        {
            System.Array.Copy(
                owners,
                targetOwners,
                owners.Length);

            mutationCenter =
                new Vector2(
                    Random.Range(
                        0.1f,
                        0.9f),

                    Random.Range(
                        0.1f,
                        0.9f));

            mutationRadius =
                Random.Range(
                    0.12f,
                    0.3f);

            float roll =
                Random.value;

            float holeEnd =
                config.HoleChance;

            float splitEnd =
                holeEnd +
                config.SplitChance;

            float swapEnd =
                splitEnd +
                config.SwapChance;

            float nestEnd =
                swapEnd +
                config.NestingChance;

            if (roll < holeEnd)
            {
                CreateHole();
            }
            else if (roll < splitEnd)
            {
                CreateSplit();
            }
            else if (roll < swapEnd)
            {
                CreateSwap();
            }
            else if (roll < nestEnd)
            {
                CreateNest();
            }
            else
            {
                CreateRewrite();
            }

            mutationActive = true;
            mutationProgress = 0f;
        }

        private void CreateHole()
        {
            int target =
                Random.Range(
                    0,
                    seeds.Length);

            float radius =
                mutationRadius;

            float radiusSquared =
                radius * radius;

            for (int i = 0;
                 i < targetOwners.Length;
                 i++)
            {
                if (targetOwners[i] !=
                    target)
                {
                    continue;
                }

                GetPixelNormalizedPosition(
                    i,
                    out float px,
                    out float py);

                float dx =
                    px -
                    mutationCenter.x;

                float dy =
                    py -
                    mutationCenter.y;

                if (dx * dx +
                    dy * dy <
                    radiusSquared)
                {
                    targetOwners[i] =
                        -1;
                }
            }
        }

        private void CreateSplit()
        {
            int source =
                Random.Range(
                    0,
                    seeds.Length);

            int destination =
                Random.Range(
                    0,
                    seeds.Length);

            if (source ==
                destination)
            {
                destination =
                    (destination + 1) %
                    seeds.Length;
            }

            Vector2 direction =
                Random.insideUnitCircle;

            if (direction.sqrMagnitude <
                0.001f)
            {
                direction =
                    Vector2.right;
            }

            direction.Normalize();

            for (int i = 0;
                 i < targetOwners.Length;
                 i++)
            {
                if (targetOwners[i] !=
                    source)
                {
                    continue;
                }

                GetPixelNormalizedPosition(
                    i,
                    out float px,
                    out float py);

                float dot =
                    (px - mutationCenter.x) *
                    direction.x +
                    (py - mutationCenter.y) *
                    direction.y;

                if (dot > 0f)
                {
                    targetOwners[i] =
                        destination;
                }
            }
        }

        private void CreateSwap()
        {
            int a =
                Random.Range(
                    0,
                    seeds.Length);

            int b =
                Random.Range(
                    0,
                    seeds.Length);

            if (a == b)
            {
                b =
                    (b + 1) %
                    seeds.Length;
            }

            for (int i = 0;
                 i < targetOwners.Length;
                 i++)
            {
                if (targetOwners[i] == a)
                {
                    targetOwners[i] = b;
                }
                else if (targetOwners[i] == b)
                {
                    targetOwners[i] = a;
                }
            }
        }

        private void CreateNest()
        {
            int inner =
                Random.Range(
                    0,
                    seeds.Length);

            int replacement =
                Random.Range(
                    0,
                    seeds.Length);

            if (inner ==
                replacement)
            {
                replacement =
                    (replacement + 1) %
                    seeds.Length;
            }

            float radius =
                mutationRadius;

            float radiusSquared =
                radius * radius;

            for (int i = 0;
                 i < targetOwners.Length;
                 i++)
            {
                GetPixelNormalizedPosition(
                    i,
                    out float px,
                    out float py);

                float dx =
                    px -
                    mutationCenter.x;

                float dy =
                    py -
                    mutationCenter.y;

                if (dx * dx +
                    dy * dy <
                    radiusSquared)
                {
                    targetOwners[i] =
                        inner;
                }
            }

            for (int i = 0;
                 i < targetOwners.Length;
                 i++)
            {
                if (targetOwners[i] ==
                    replacement)
                {
                    targetOwners[i] =
                        inner;
                }
            }
        }

        private void CreateRewrite()
        {
            int replacement =
                Random.Range(
                    0,
                    seeds.Length);

            float radius =
                mutationRadius;

            float inverseRadius =
                1f /
                Mathf.Max(
                    0.001f,
                    radius);

            float strength =
                config.MutationStrength;

            for (int i = 0;
                 i < targetOwners.Length;
                 i++)
            {
                GetPixelNormalizedPosition(
                    i,
                    out float px,
                    out float py);

                float dx =
                    px -
                    mutationCenter.x;

                float dy =
                    py -
                    mutationCenter.y;

                float distance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy);

                if (distance > radius)
                {
                    continue;
                }

                float influence =
                    1f -
                    distance *
                    inverseRadius;

                if (StableRandom(i) <
                    influence *
                    strength)
                {
                    targetOwners[i] =
                        replacement;
                }
            }
        }

        private void Render()
        {
            if (texture == null ||
                owners == null ||
                territoryColors == null ||
                territoryColors.Length == 0)
            {
                return;
            }

            float mutationPulse =
                mutationActive
                    ? Mathf.Sin(
                        mutationProgress *
                        Mathf.PI)
                    : 0f;

            float mutationInverseRadius =
                1f /
                Mathf.Max(
                    0.001f,
                    mutationRadius);

            for (int y = 0;
                 y < height;
                 y++)
            {
                for (int x = 0;
                     x < width;
                     x++)
                {
                    int index =
                        y * width + x;

                    int owner =
                        owners[index];

                    Color color;

                    if (owner < 0)
                    {
                        color =
                            Color.black *
                            0.35f;

                        color.a = 1f;
                    }
                    else
                    {
                        color =
                            territoryColors[
                                owner %
                                territoryColors.Length];

                        float localVariation =
                            Mathf.Sin(
                                elapsedTime * 0.35f +
                                x * 0.13f +
                                y * 0.17f +
                                owner * 1.7f);

                        color *=
                            1f +
                            localVariation *
                            0.12f;
                    }

                    if (IsBoundary(x, y))
                    {
                        color =
                            Color.Lerp(
                                color,
                                Color.black,
                                config.BoundaryDarkness);
                    }

                    if (mutationActive)
                    {
                        float px =
                            (x + 0.5f) /
                            width;

                        float py =
                            (y + 0.5f) /
                            height;

                        float dx =
                            px -
                            mutationCenter.x;

                        float dy =
                            py -
                            mutationCenter.y;

                        float distance =
                            Mathf.Sqrt(
                                dx * dx +
                                dy * dy);

                        float influence =
                            Mathf.Clamp01(
                                1f -
                                distance *
                                mutationInverseRadius);

                        color =
                            Color.Lerp(
                                color,
                                Color.white,
                                influence *
                                mutationPulse *
                                config.MutationGlow);
                    }

                    color.a =
                        config.Opacity;

                    texture.SetPixel(
                        x,
                        y,
                        color);
                }
            }

            texture.Apply(false);

            spriteRenderer.enabled = true;
        }

        private bool IsBoundary(
            int x,
            int y)
        {
            int owner =
                owners[
                    y * width + x];

            if (x > 0 &&
                owners[
                    y * width + x - 1] !=
                owner)
            {
                return true;
            }

            if (x < width - 1 &&
                owners[
                    y * width + x + 1] !=
                owner)
            {
                return true;
            }

            if (y > 0 &&
                owners[
                    (y - 1) * width + x] !=
                owner)
            {
                return true;
            }

            if (y < height - 1 &&
                owners[
                    (y + 1) * width + x] !=
                owner)
            {
                return true;
            }

            return false;
        }

        private void GetPixelNormalizedPosition(
            int index,
            out float x,
            out float y)
        {
            int pixelX =
                index % width;

            int pixelY =
                index / width;

            x =
                (pixelX + 0.5f) /
                width;

            y =
                (pixelY + 0.5f) /
                height;
        }

        private static float StableRandom(
            int value)
        {
            unchecked
            {
                uint x =
                    (uint)value;

                x ^= x >> 16;
                x *= 0x7feb352d;
                x ^= x >> 15;
                x *= 0x846ca68b;
                x ^= x >> 16;

                return
                    (x & 0x00ffffff) /
                    16777215f;
            }
        }

        private static void DestroyObject(
            Object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}