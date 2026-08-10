using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFRealityFoldLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags =
        {
            "Background",
            "RealityFold"
        };

        private const int PaletteSize = 64;
        private const int MaximumFolds = 5;

        private readonly BFRealityFoldLayerConfig config;

        private Transform root;
        private SpriteRenderer spriteRenderer;
        private Texture2D texture;
        private Sprite sprite;

        private Color32[] pixels;
        private Color32[] palette;

        private Vector2[] foldPositions;
        private float[] foldPhases;

        private int textureWidth;
        private int textureHeight;

        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        private float elapsedTime;
        private float updateTimer;

        public BFRealityFoldLayer(
            BFRealityFoldLayerConfig config)
        {
            this.config = config;
        }

        public void Init(
            Transform parent,
            int sortingOrder)
        {
            elapsedTime = 0f;
            updateTimer = 999f;

            GameObject obj =
                new GameObject(
                    "BFRealityFoldLayer");

            obj.transform.SetParent(
                parent,
                false);

            obj.transform.position =
                new Vector3(
                    Screen.width * 0.5f,
                    Screen.height * 0.5f,
                    10f);

            obj.layer =
                BFBackgroundStackManager.BackgroundLayer;

            root = obj.transform;

            spriteRenderer =
                obj.AddComponent<SpriteRenderer>();

            spriteRenderer.sortingOrder =
                sortingOrder;

            spriteRenderer.color =
                Color.white;

            BuildPalette();
            BuildFolds();
            BuildTexture();

            SyncScreenTransform();
            Render();

            BFLogger.Info(
                LogTags,
                $"BFRealityFoldLayer: initialized at {textureWidth}x{textureHeight}.");
        }

        public void Tick(float dt)
        {
            if (root == null ||
                texture == null)
            {
                return;
            }

            elapsedTime += dt;
            updateTimer += dt;

            if (Screen.width != lastScreenWidth ||
                Screen.height != lastScreenHeight)
            {
                SyncScreenTransform();
            }

            float interval =
                Mathf.Max(
                    0.02f,
                    config.UpdateInterval);

            if (updateTimer >= interval)
            {
                updateTimer = 0f;
                Render();
            }
        }

        public void Cleanup()
        {
            if (sprite != null)
            {
                DestroyObject(sprite);
                sprite = null;
            }

            if (texture != null)
            {
                DestroyObject(texture);
                texture = null;
            }

            if (root != null)
            {
                DestroyObject(
                    root.gameObject);

                root = null;
            }

            spriteRenderer = null;

            pixels = null;
            palette = null;
            foldPositions = null;
            foldPhases = null;
        }

        private void BuildPalette()
        {
            palette =
                new Color32[PaletteSize];

            Gradient gradient =
                config.ColorGradient;

            for (int i = 0;
                i < PaletteSize;
                i++)
            {
                float t =
                    i /
                    (PaletteSize - 1f);

                Color color =
                    gradient != null
                        ? gradient.Evaluate(t)
                        : Color.white;

                palette[i] =
                    color;
            }
        }

        private void BuildFolds()
        {
            int count =
                Mathf.Clamp(
                    config.FoldCount,
                    1,
                    MaximumFolds);

            foldPositions =
                new Vector2[count];

            foldPhases =
                new float[count];

            for (int i = 0;
                i < count;
                i++)
            {
                float angle =
                    Mathf.PI * 2f *
                    i /
                    count;

                /*
                 * Fixed deterministic positions.
                 *
                 * We deliberately don't use Random here.
                 * The configuration therefore produces the same
                 * visual every time.
                 */
                foldPositions[i] =
                    new Vector2(
                        0.5f +
                        Mathf.Cos(angle) *
                        0.22f,

                        0.5f +
                        Mathf.Sin(angle) *
                        0.22f);

                foldPhases[i] =
                    angle +
                    i * 1.371f;
            }
        }

        private void BuildTexture()
        {
            int width =
                Mathf.Clamp(
                    config.TextureWidth,
                    16,
                    64);

            int height =
                Mathf.Clamp(
                    config.TextureHeight,
                    16,
                    64);

            if (texture != null &&
                textureWidth == width &&
                textureHeight == height)
            {
                return;
            }

            if (sprite != null)
            {
                DestroyObject(sprite);
                sprite = null;
            }

            if (texture != null)
            {
                DestroyObject(texture);
                texture = null;
            }

            textureWidth = width;
            textureHeight = height;

            pixels =
                new Color32[
                    width * height];

            texture =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false,
                    false);

            texture.name =
                "BFRealityFold_Texture";

            texture.wrapMode =
                TextureWrapMode.Clamp;

            texture.filterMode =
                FilterMode.Bilinear;

            texture.anisoLevel = 0;

            sprite =
                Sprite.Create(
                    texture,
                    new Rect(
                        0f,
                        0f,
                        width,
                        height),
                    new Vector2(
                        0.5f,
                        0.5f),
                    1f);

            sprite.name =
                "BFRealityFold_Sprite";

            spriteRenderer.sprite =
                sprite;
        }

        private void SyncScreenTransform()
        {
            lastScreenWidth =
                Screen.width;

            lastScreenHeight =
                Screen.height;

            root.position =
                new Vector3(
                    Screen.width * 0.5f,
                    Screen.height * 0.5f,
                    10f);

            root.localScale =
                new Vector3(
                    Screen.width /
                    Mathf.Max(
                        1f,
                        textureWidth),

                    Screen.height /
                    Mathf.Max(
                        1f,
                        textureHeight),

                    1f);
        }

        private void Render()
        {
            int width =
                textureWidth;

            int height =
                textureHeight;

            float time =
                elapsedTime;

            float movement =
                config.MovementAmount;

            float speed =
                config.MovementSpeed;

            float frequency =
                config.FoldFrequency;

            float foldStrength =
                config.FoldStrength;

            float foldSize =
                Mathf.Max(
                    0.01f,
                    config.FoldSize);

            float chaos =
                config.Chaos;

            float collapse =
                config.Collapse;

            float edgeGlow =
                config.EdgeGlow;

            float colorTime =
                time *
                config.ColorSpeed;

            int foldCount =
                foldPositions.Length;

            for (int y = 0;
                y < height;
                y++)
            {
                float v =
                    y /
                    (height - 1f);

                float py =
                    v - 0.5f;

                int row =
                    y * width;

                for (int x = 0;
                    x < width;
                    x++)
                {
                    float u =
                        x /
                        (width - 1f);

                    float px =
                        u - 0.5f;

                    float field = 0f;
                    float edge = 0f;

                    for (int i = 0;
                        i < foldCount;
                        i++)
                    {
                        Vector2 basePosition =
                            foldPositions[i];

                        float phase =
                            foldPhases[i];

                        float orbit =
                            time *
                            speed;

                        float fx =
                            basePosition.x -
                            0.5f;

                        float fy =
                            basePosition.y -
                            0.5f;

                        float ox =
                            Mathf.Cos(
                                orbit +
                                phase) *
                            movement;

                        float oy =
                            Mathf.Sin(
                                orbit * 0.83f +
                                phase) *
                            movement;

                        /*
                         * Collapse all folds toward the centre.
                         */
                        fx *=
                            1f -
                            collapse;

                        fy *=
                            1f -
                            collapse;

                        float dx =
                            px -
                            fx -
                            ox;

                        float dy =
                            py -
                            fy -
                            oy;

                        /*
                         * Squared distance only.
                         *
                         * No sqrt.
                         */
                        float distanceSquared =
                            dx * dx +
                            dy * dy;

                        float radiusSquared =
                            foldSize *
                            foldSize;

                        /*
                         * Cheap rational falloff.
                         *
                         * This replaces several expensive operations
                         * from the previous implementation.
                         */
                        float influence =
                            radiusSquared /
                            (
                                radiusSquared +
                                distanceSquared * 4f
                            );

                        float wave =
                            Mathf.Sin(
                                distanceSquared *
                                32f *
                                frequency +
                                phase +
                                colorTime);

                        field +=
                            wave *
                            influence *
                            foldStrength;

                        /*
                         * Detect the bright boundary of each fold.
                         */
                        float boundary =
                            Mathf.Abs(
                                wave);

                        edge +=
                            influence *
                            (
                                1f -
                                boundary
                            );
                    }

                    /*
                     * A cheap secondary distortion gives the image
                     * much more visual complexity than the actual
                     * computation involved.
                     */
                    float secondary =
                        Mathf.Sin(
                            px * 17f +
                            field * 1.7f +
                            colorTime);

                    secondary *=
                        Mathf.Cos(
                            py * 13f -
                            field);

                    field +=
                        secondary *
                        chaos *
                        0.45f;

                    /*
                     * Turn the scalar field into a looping position
                     * in the palette.
                     */
                    float colorPosition =
                        0.5f +
                        0.5f *
                        Mathf.Sin(
                            field +
                            colorTime);

                    colorPosition =
                        Mathf.Repeat(
                            colorPosition,
                            1f);

                    int paletteIndex =
                        (int)(
                            colorPosition *
                            (PaletteSize - 1));

                    if (paletteIndex < 0)
                        paletteIndex = 0;

                    if (paletteIndex >= PaletteSize)
                        paletteIndex =
                            PaletteSize - 1;

                    Color32 color =
                        palette[paletteIndex];

                    /*
                     * Edge lighting.
                     */
                    float glow =
                        Mathf.Clamp01(
                            edge /
                            Mathf.Max(
                                1f,
                                foldCount * 0.8f));

                    glow *=
                        edgeGlow;

                    float brightness =
                        config.Brightness *
                        (0.8f + glow);

                    color.r =
                        MultiplyByte(
                            color.r,
                            brightness);

                    color.g =
                        MultiplyByte(
                            color.g,
                            brightness);

                    color.b =
                        MultiplyByte(
                            color.b,
                            brightness);

                    color.a =
                        MultiplyByte(
                            color.a,
                            config.Opacity);

                    pixels[row + x] =
                        color;
                }
            }

            texture.SetPixels32(
                pixels);

            texture.Apply(
                false,
                false);
        }

        private static byte MultiplyByte(
            byte value,
            float multiplier)
        {
            int result =
                (int)(
                    value *
                    multiplier);

            if (result < 0)
                result = 0;

            if (result > 255)
                result = 255;

            return (byte)result;
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
    }
}