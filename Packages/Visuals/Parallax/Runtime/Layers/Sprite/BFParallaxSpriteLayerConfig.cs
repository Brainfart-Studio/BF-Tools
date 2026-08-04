using UnityEngine;

namespace BFTools.Visuals.Parallax
{
    public class BFParallaxSpriteLayerConfig : BFParallaxLayerConfig
    {
        [Header("Sprite")]
        [SerializeField, Tooltip("The sprite this layer renders and moves.")]
        private Sprite sprite;

        [SerializeField, Tooltip("Local offset from the stack's origin before any parallax or scroll movement is applied.")]
        private Vector2 initialOffset;

        [SerializeField, Tooltip("Fine-tunes this layer's sorting order relative to its position in the stack, without changing its slot in the Layers list.")]
        private int sortingOrderOffset;

        [Header("Horizontal")]
        [SerializeField, Tooltip("Enables horizontal parallax and auto-scroll movement for this layer.")]
        private bool horizontalEnabled;

        [SerializeField, Range(-2f, 2f), Tooltip("How much this layer moves relative to the camera's horizontal movement. 0 stays locked to the screen, 1 moves exactly with the camera, values below 1 sit farther back, values above 1 sit closer than the camera plane. Negative values invert the direction.")]
        private float horizontalParallaxFactor = 1f;

        [SerializeField, Tooltip("Constant horizontal drift in units per second, independent of camera movement. Use for layers that move on their own, like drifting clouds.")]
        private float horizontalAutoScrollSpeed;

        [SerializeField, Tooltip("Tiles this layer horizontally so it repeats seamlessly as it scrolls, instead of scrolling off and leaving empty space.")]
        private bool horizontalLooping;

        [SerializeField, Tooltip("Overrides the tile width used for horizontal looping. 0 auto-detects the width from the sprite's bounds.")]
        private float horizontalTileWidthOverride;

        [SerializeField, Tooltip("Locks horizontal movement to one direction once it's moved that way, like a side-scroller camera that never scrolls back. None allows free movement both ways.")]
        private BFParallaxAxisLock horizontalLock = BFParallaxAxisLock.None;

        [Header("Vertical")]
        [SerializeField, Tooltip("Enables vertical parallax and auto-scroll movement for this layer.")]
        private bool verticalEnabled;

        [SerializeField, Range(-2f, 2f), Tooltip("How much this layer moves relative to the camera's vertical movement. 0 stays locked to the screen, 1 moves exactly with the camera, values below 1 sit farther back, values above 1 sit closer than the camera plane. Negative values invert the direction.")]
        private float verticalParallaxFactor = 1f;

        [SerializeField, Tooltip("Constant vertical drift in units per second, independent of camera movement.")]
        private float verticalAutoScrollSpeed;

        [SerializeField, Tooltip("Tiles this layer vertically so it repeats seamlessly as it scrolls, instead of scrolling off and leaving empty space.")]
        private bool verticalLooping;

        [SerializeField, Tooltip("Overrides the tile height used for vertical looping. 0 auto-detects the height from the sprite's bounds.")]
        private float verticalTileHeightOverride;

        [SerializeField, Tooltip("Locks vertical movement to one direction once it's moved that way. None allows free movement both ways.")]
        private BFParallaxAxisLock verticalLock = BFParallaxAxisLock.None;

        internal Sprite Sprite => sprite;
        internal Vector2 InitialOffset => initialOffset;
        internal int SortingOrderOffset => sortingOrderOffset;

        internal bool HorizontalEnabled => horizontalEnabled;
        internal float HorizontalParallaxFactor => horizontalParallaxFactor;
        internal float HorizontalAutoScrollSpeed => horizontalAutoScrollSpeed;
        internal bool HorizontalLooping => horizontalLooping;
        internal float HorizontalTileWidthOverride => horizontalTileWidthOverride;
        internal BFParallaxAxisLock HorizontalLock => horizontalLock;

        internal bool VerticalEnabled => verticalEnabled;
        internal float VerticalParallaxFactor => verticalParallaxFactor;
        internal float VerticalAutoScrollSpeed => verticalAutoScrollSpeed;
        internal bool VerticalLooping => verticalLooping;
        internal float VerticalTileHeightOverride => verticalTileHeightOverride;
        internal BFParallaxAxisLock VerticalLock => verticalLock;

        public override IBFParallaxLayer CreateLayer()
        {
            return new BFParallaxSpriteLayer(this);
        }
    }
}