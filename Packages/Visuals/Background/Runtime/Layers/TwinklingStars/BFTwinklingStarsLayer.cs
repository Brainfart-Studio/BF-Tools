using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFTwinklingStarsLayer : IBFBackgroundLayer
    {
        private static readonly string[] LogTags = { "Background", "TwinklingStars" };

        private readonly BFTwinklingStarsLayerConfig config;
        private readonly List<BFTwinklingStar> stars = new List<BFTwinklingStar>();
        private readonly List<SpriteRenderer> starRenderers = new List<SpriteRenderer>();

        private static Sprite dotSprite;

        private Transform root;

        public BFTwinklingStarsLayer(BFTwinklingStarsLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            EnsureDotSprite();

            GameObject rootObj = new GameObject("BFTwinklingStarsLayer");
            rootObj.transform.SetParent(parent, false);
            rootObj.layer = BFBackgroundStackManager.BackgroundLayer;
            root = rootObj.transform;

            stars.Clear();
            starRenderers.Clear();
            for (int i = 0; i < config.StarCount; i++)
            {
                stars.Add(new BFTwinklingStar(config));

                GameObject go = new GameObject($"Star_{i}");
                go.transform.SetParent(root, false);
                go.layer = BFBackgroundStackManager.BackgroundLayer;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = dotSprite;
                sr.color = Color.white;
                sr.sortingOrder = sortingOrder;

                starRenderers.Add(sr);
            }

            BFLogger.Info(LogTags, $"BFTwinklingStarsLayer: initialized with {config.StarCount} star(s).");
        }

        public void Tick(float dt)
        {
            float viewWidth = Screen.width;
            float viewHeight = Screen.height;

            for (int i = 0; i < stars.Count; i++)
            {
                BFTwinklingStar star = stars[i];
                star.Tick(dt);

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
            root = null;

            stars.Clear();
            starRenderers.Clear();
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