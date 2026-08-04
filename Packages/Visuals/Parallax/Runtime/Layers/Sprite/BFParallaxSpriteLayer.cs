using System.Collections.Generic;
using BFTools.Core.Logger;
using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public class BFParallaxSpriteLayer : IBFParallaxLayer
    {
        private const float MinTileSize = 0.0001f;

        private static readonly string[] LogTags = { "Parallax", "Sprite" };

        private readonly BFParallaxSpriteLayerConfig config;

        private Transform root;
        private List<SpriteRenderer> tiles;
        private int tileCountX;
        private int tileCountY;
        private float tileWidth;
        private float tileHeight;

        private float elapsedTime;
        private float horizontalLockedValue;
        private float verticalLockedValue;

        public BFParallaxSpriteLayer(BFParallaxSpriteLayerConfig config)
        {
            this.config = config;
        }

        public void Init(Transform parent, int sortingOrder)
        {
            elapsedTime = 0f;
            horizontalLockedValue = 0f;
            verticalLockedValue = 0f;

            if (config.Sprite == null)
                BFLogger.Warning(LogTags, "BFParallaxSpriteLayer: no sprite assigned, layer will render nothing.");

            GameObject rootObj = new GameObject("BFParallaxSpriteLayer");
            rootObj.transform.SetParent(parent, false);
            rootObj.transform.localPosition = new Vector3(config.InitialOffset.x, config.InitialOffset.y, 0f);
            root = rootObj.transform;

            ResolveTileSize();

            tileCountX = config.HorizontalLooping ? 3 : 1;
            tileCountY = config.VerticalLooping ? 3 : 1;

            tiles = new List<SpriteRenderer>(tileCountX * tileCountY);
            for (int i = 0; i < tileCountX * tileCountY; i++)
            {
                GameObject tileObj = new GameObject("Tile");
                tileObj.transform.SetParent(root, false);

                SpriteRenderer renderer = tileObj.AddComponent<SpriteRenderer>();
                renderer.sprite = config.Sprite;
                renderer.sortingOrder = sortingOrder + config.SortingOrderOffset;

                tiles.Add(renderer);
            }

            PositionTiles(0f, 0f);

            BFLogger.Info(LogTags, "BFParallaxSpriteLayer: initialized.");
        }

        public void Tick(Vector2 cameraDisplacement, float dt)
        {
            elapsedTime += dt;

            float horizontalOffset = 0f;
            if (config.HorizontalEnabled)
            {
                horizontalOffset = cameraDisplacement.x * config.HorizontalParallaxFactor + config.HorizontalAutoScrollSpeed * elapsedTime;
                horizontalOffset = ApplyLock(horizontalOffset, config.HorizontalLock, ref horizontalLockedValue);
            }

            float verticalOffset = 0f;
            if (config.VerticalEnabled)
            {
                verticalOffset = cameraDisplacement.y * config.VerticalParallaxFactor + config.VerticalAutoScrollSpeed * elapsedTime;
                verticalOffset = ApplyLock(verticalOffset, config.VerticalLock, ref verticalLockedValue);
            }

            root.localPosition = new Vector3(config.InitialOffset.x + horizontalOffset, config.InitialOffset.y + verticalOffset, 0f);

            PositionTiles(horizontalOffset, verticalOffset);
        }

        public void Cleanup()
        {
            if (root != null)
                Object.Destroy(root.gameObject);
            root = null;

            tiles?.Clear();
        }

        private static float ApplyLock(float offset, BFParallaxAxisLock lockMode, ref float lockedValue)
        {
            switch (lockMode)
            {
                case BFParallaxAxisLock.PositiveOnly:
                    lockedValue = Mathf.Max(lockedValue, offset);
                    return lockedValue;
                case BFParallaxAxisLock.NegativeOnly:
                    lockedValue = Mathf.Min(lockedValue, offset);
                    return lockedValue;
                default:
                    return offset;
            }
        }

        private void ResolveTileSize()
        {
            Vector2 spriteSize = config.Sprite != null ? config.Sprite.bounds.size : Vector2.one;

            tileWidth = config.HorizontalTileWidthOverride > 0f ? config.HorizontalTileWidthOverride : Mathf.Max(spriteSize.x, MinTileSize);
            tileHeight = config.VerticalTileHeightOverride > 0f ? config.VerticalTileHeightOverride : Mathf.Max(spriteSize.y, MinTileSize);
        }

        // Wraps each tile's local position every tileWidth/tileHeight so a fixed 3x3
        // grid can cover an infinitely scrolling axis instead of needing one tile per
        // screen's worth of travel.
        private void PositionTiles(float horizontalOffset, float verticalOffset)
        {
            float wrappedX = config.HorizontalLooping ? Mathf.Repeat(horizontalOffset, tileWidth) : 0f;
            float wrappedY = config.VerticalLooping ? Mathf.Repeat(verticalOffset, tileHeight) : 0f;

            int index = 0;
            for (int y = 0; y < tileCountY; y++)
            {
                for (int x = 0; x < tileCountX; x++)
                {
                    float localX = (x - tileCountX / 2) * tileWidth - wrappedX;
                    float localY = (y - tileCountY / 2) * tileHeight - wrappedY;
                    tiles[index].transform.localPosition = new Vector3(localX, localY, 0f);
                    index++;
                }
            }
        }
    }
}