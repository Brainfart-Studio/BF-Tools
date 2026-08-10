using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFStainedGlassLayer : IBFBackgroundLayer
    {
        private const string LogTag = "Background";

        private readonly BFStainedGlassLayerConfig config;

        private Transform root;
        private GameObject glassObject;
        private SpriteRenderer renderer;

        private Texture2D texture;
        private Sprite sprite;

        private Color32[] pixels;

        private int[] paneMap;
        private bool[] leadMap;

        private Color[] paneColors;
        private float[] panePhases;

        private int width;
        private int height;
        private int paneCount;

        private int lastResolutionX = -1;
        private int lastResolutionY = -1;
        private int lastPaneCount = -1;
        private float time;

        public BFStainedGlassLayer(
            BFStainedGlassLayerConfig config)
        {
            this.config = config;
        }

        public void Init(
            Transform parent,
            int sortingOrder)
        {
            CreateRoot(parent);
            CreateTexture();
            CreateSprite(sortingOrder);
            BuildGlass();

            Render();

            BFLogger.Info(
                LogTag,
                "BFStainedGlassLayer: initialized.");
        }

        public void Tick(float dt)
        {
            time += dt;

            bool resolutionChanged =
                config.ResolutionX != lastResolutionX ||
                config.ResolutionY != lastResolutionY;

            bool paneCountChanged =
                config.PaneCount != lastPaneCount;

            bool screenChanged =
                Screen.width != lastScreenWidth ||
                Screen.height != lastScreenHeight;

            if (resolutionChanged)
            {
                CreateTexture();
                CreateSprite(renderer.sortingOrder);
                BuildGlass();
                Render();
                return;
            }

            if (paneCountChanged)
            {
                BuildGlass();
                Render();
                return;
            }

            if (screenChanged)
            {
                FitToBackgroundCamera();
            }

            Render();
        }

        public void Cleanup()
        {
            if (renderer != null)
            {
                renderer.sprite = null;
            }

            DestroyObject(sprite);
            DestroyObject(texture);

            if (root != null)
            {
                DestroyObject(root.gameObject);
            }

            root = null;
            glassObject = null;
            renderer = null;

            texture = null;
            sprite = null;

            pixels = null;
            paneMap = null;
            leadMap = null;
            paneColors = null;
            panePhases = null;
        }

        private void CreateRoot(
            Transform parent)
        {
            GameObject obj =
                new GameObject(
                    "BFStainedGlassLayer");

            obj.transform.SetParent(
                parent,
                false);

            obj.transform.localPosition =
                Vector3.zero;

            obj.transform.localRotation =
                Quaternion.identity;

            obj.transform.localScale =
                Vector3.one;

            obj.layer =
                BFBackgroundStackManager.BackgroundLayer;

            root = obj.transform;
        }

        private void CreateTexture()
        {
            width =
                Mathf.Clamp(
                    config.ResolutionX,
                    24,
                    160);

            height =
                Mathf.Clamp(
                    config.ResolutionY,
                    14,
                    90);

            DestroyObject(texture);

            texture =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false,
                    false);

            texture.name =
                "BFStainedGlassLayer_Texture";

            texture.wrapMode =
                TextureWrapMode.Clamp;

            texture.filterMode =
                FilterMode.Bilinear;

            texture.anisoLevel = 0;

            pixels =
                new Color32[
                    width * height];

            lastResolutionX =
                config.ResolutionX;

            lastResolutionY =
                config.ResolutionY;
        }

        private void CreateSprite(
            int sortingOrder)
        {
            if (glassObject == null)
            {
                glassObject =
                    new GameObject(
                        "StainedGlass");

                glassObject.transform.SetParent(
                    root,
                    false);

                glassObject.layer =
                    BFBackgroundStackManager.BackgroundLayer;

                renderer =
                    glassObject.AddComponent<SpriteRenderer>();
            }

            DestroyObject(sprite);

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
                "BFStainedGlassLayer_Sprite";

            renderer.sprite =
                sprite;

            renderer.sortingOrder =
                sortingOrder;

            renderer.color =
                Color.white;

            renderer.enabled =
                true;

            FitToBackgroundCamera();
        }

        private void FitToBackgroundCamera()
        {
            if (glassObject == null ||
                sprite == null)
            {
                return;
            }

            /*
             * BFBackgroundStackCamera deliberately uses:
             *
             *     orthographicSize = Screen.height / 2
             *     position.x = Screen.width / 2
             *     position.y = Screen.height / 2
             *
             * Therefore its visible world rectangle is exactly:
             *
             *     X = 0 .. Screen.width
             *     Y = 0 .. Screen.height
             *
             * The sprite is created at 1 pixel = 1 world unit.
             * Scale it directly to those dimensions.
             */

            float screenWidth =
                Mathf.Max(
                    1,
                    Screen.width);

            float screenHeight =
                Mathf.Max(
                    1,
                    Screen.height);

            glassObject.transform.localPosition =
                new Vector3(
                    screenWidth * 0.5f,
                    screenHeight * 0.5f,
                    0f);

            glassObject.transform.localScale =
                new Vector3(
                    screenWidth / width,
                    screenHeight / height,
                    1f);

            lastScreenWidth =
                Screen.width;

            lastScreenHeight =
                Screen.height;
        }

        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        private void BuildGlass()
        {
            paneCount =
                Mathf.Clamp(
                    config.PaneCount,
                    4,
                    96);

            paneMap =
                new int[
                    width *
                    height];

            leadMap =
                new bool[
                    width *
                    height];

            paneColors =
                new Color[paneCount];

            panePhases =
                new float[paneCount];

            GeneratePanes();
            GeneratePaneMap();
            GenerateLeadMap();

            lastPaneCount =
                config.PaneCount;
        }

        private void GeneratePanes()
        {
            Random.State oldState =
                Random.state;

            Random.InitState(
                width * 92821 +
                height * 68917 +
                paneCount * 39113);

            int columns =
                Mathf.CeilToInt(
                    Mathf.Sqrt(
                        paneCount));

            int rows =
                Mathf.CeilToInt(
                    paneCount /
                    (float)columns);

            float irregularity =
                Mathf.Clamp01(
                    config.Irregularity);

            for (int i = 0;
                 i < paneCount;
                 i++)
            {
                int column =
                    i % columns;

                int row =
                    i / columns;

                float regularX =
                    (column + 0.5f) /
                    columns;

                float regularY =
                    (row + 0.5f) /
                    rows;

                float randomX =
                    Random.value;

                float randomY =
                    Random.value;

                float x =
                    Mathf.Lerp(
                        regularX,
                        randomX,
                        irregularity);

                float y =
                    Mathf.Lerp(
                        regularY,
                        randomY,
                        irregularity);

                /*
                 * Store the seed directly in panePhase.
                 * No seed array is necessary because the topology
                 * is generated only once.
                 */

                panePhases[i] =
                    Random.Range(
                        0f,
                        Mathf.PI * 2f);

                float gradientPosition =
                    paneCount <= 1
                        ? 0f
                        : i /
                          (float)(paneCount - 1);

                Color color =
                    config.ColorGradient.Evaluate(
                        gradientPosition);

                /*
                 * A small deterministic variation keeps adjacent
                 * panes from looking like cloned rectangles.
                 */
                float variation =
                    Random.Range(
                        0.88f,
                        1.12f);

                color *= variation;

                paneColors[i] =
                    color;

                /*
                 * Reuse the phase storage to encode the seed.
                 * The lower precision is completely irrelevant at
                 * this resolution and saves another Vector2 array.
                 */
                panePhases[i] =
                    panePhases[i] +
                    x * 17.31f +
                    y * 31.77f;
            }

            Random.state = oldState;

            /*
             * GeneratePaneMap needs the same deterministic seed
             * positions, so regenerate them there rather than
             * keeping a permanent seed array.
             */
        }

        private Vector2 GetPanePosition(
            int index)
        {
            int columns =
                Mathf.CeilToInt(
                    Mathf.Sqrt(
                        paneCount));

            int rows =
                Mathf.CeilToInt(
                    paneCount /
                    (float)columns);

            Random.State oldState =
                Random.state;

            Random.InitState(
                width * 92821 +
                height * 68917 +
                paneCount * 39113);

            Vector2 result =
                Vector2.zero;

            float irregularity =
                Mathf.Clamp01(
                    config.Irregularity);

            for (int i = 0;
                 i <= index;
                 i++)
            {
                int column =
                    i % columns;

                int row =
                    i / columns;

                float regularX =
                    (column + 0.5f) /
                    columns;

                float regularY =
                    (row + 0.5f) /
                    rows;

                float randomX =
                    Random.value;

                float randomY =
                    Random.value;

                if (i == index)
                {
                    result =
                        new Vector2(
                            Mathf.Lerp(
                                regularX,
                                randomX,
                                irregularity),

                            Mathf.Lerp(
                                regularY,
                                randomY,
                                irregularity));
                }

                Random.Range(
                    0f,
                    Mathf.PI * 2f);
            }

            Random.state = oldState;

            return result;
        }

        private void GeneratePaneMap()
        {
            Vector2[] positions =
                new Vector2[paneCount];

            for (int i = 0;
                 i < paneCount;
                 i++)
            {
                positions[i] =
                    GetPanePosition(i);
            }

            for (int y = 0;
                 y < height;
                 y++)
            {
                float ny =
                    (y + 0.5f) /
                    height;

                for (int x = 0;
                     x < width;
                     x++)
                {
                    float nx =
                        (x + 0.5f) /
                        width;

                    int best =
                        0;

                    float bestDistance =
                        float.MaxValue;

                    for (int p = 0;
                         p < paneCount;
                         p++)
                    {
                        float dx =
                            nx -
                            positions[p].x;

                        float dy =
                            ny -
                            positions[p].y;

                        float distance =
                            dx * dx +
                            dy * dy;

                        if (distance <
                            bestDistance)
                        {
                            bestDistance =
                                distance;

                            best =
                                p;
                        }
                    }

                    paneMap[
                        y * width + x] =
                        best;
                }
            }
        }

        private void GenerateLeadMap()
        {
            int radius =
                Mathf.Max(
                    0,
                    Mathf.RoundToInt(
                        config.LeadThickness *
                        Mathf.Min(
                            width,
                            height)));

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
                        paneMap[index];

                    bool boundary =
                        false;

                    if (x > 0 &&
                        paneMap[index - 1] != owner)
                    {
                        boundary = true;
                    }
                    else if (x < width - 1 &&
                             paneMap[index + 1] != owner)
                    {
                        boundary = true;
                    }
                    else if (y > 0 &&
                             paneMap[index - width] != owner)
                    {
                        boundary = true;
                    }
                    else if (y < height - 1 &&
                             paneMap[index + width] != owner)
                    {
                        boundary = true;
                    }

                    if (boundary)
                    {
                        leadMap[index] = true;
                    }
                    else if (radius > 0 &&
                             NearBoundary(
                                 x,
                                 y,
                                 owner,
                                 radius))
                    {
                        leadMap[index] = true;
                    }
                }
            }
        }

        private bool NearBoundary(
            int x,
            int y,
            int owner,
            int radius)
        {
            int minX =
                Mathf.Max(
                    0,
                    x - radius);

            int maxX =
                Mathf.Min(
                    width - 1,
                    x + radius);

            int minY =
                Mathf.Max(
                    0,
                    y - radius);

            int maxY =
                Mathf.Min(
                    height - 1,
                    y + radius);

            for (int yy = minY;
                 yy <= maxY;
                 yy++)
            {
                int row =
                    yy * width;

                for (int xx = minX;
                     xx <= maxX;
                     xx++)
                {
                    if (paneMap[row + xx] != owner)
                        return true;
                }
            }

            return false;
        }

        private void Render()
        {
            if (texture == null ||
                pixels == null ||
                paneMap == null)
            {
                return;
            }

            float animation =
                time *
                config.AnimationSpeed;

            float opacity =
                Mathf.Clamp01(
                    config.Opacity);

            float movement =
                config.LightMovement;

            Color lead =
                Color.Lerp(
                    Color.white,
                    Color.black,
                    config.LeadDarkness);

            lead.a =
                opacity;

            /*
             * One animation value per pane.
             *
             * This is important: we do not perform a Sin() for
             * every pixel. A 96x54 image is ~5,000 pixels, but
             * the expensive animation math happens only once
             * per pane.
             */
            float[] paneLight =
                new float[paneCount];

            for (int i = 0;
                 i < paneCount;
                 i++)
            {
                float wave =
                    Mathf.Sin(
                        animation +
                        panePhases[i]);

                paneLight[i] =
                    1f +
                    wave *
                    movement;
            }

            for (int i = 0;
                 i < pixels.Length;
                 i++)
            {
                if (leadMap[i])
                {
                    pixels[i] =
                        lead;

                    continue;
                }

                int pane =
                    paneMap[i];

                Color color =
                    paneColors[pane];

                color *=
                    paneLight[pane];

                color.a =
                    opacity;

                pixels[i] =
                    color;
            }

            texture.SetPixels32(
                pixels);

            texture.Apply(
                false,
                false);
        }

        private static void DestroyObject(
            Object obj)
        {
            if (obj == null)
                return;

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