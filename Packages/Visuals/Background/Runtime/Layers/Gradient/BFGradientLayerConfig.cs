using UnityEngine;

namespace BFTools.Visuals.Background
{
    public class BFGradientLayerConfig : BFBackgroundLayerConfig
    {
        [SerializeField] private Gradient colorGradient = CreateDefaultGradient();

        [SerializeField, Range(0f, 1f), Tooltip("Where the gradient's midpoint sits along the axis. 0.5 is centered; move it toward 0 or 1 to give one color more of the screen.")]
        private float midpoint = 0.5f;

        [SerializeField, Range(0f, 1f), Tooltip("How gradual the blend between colors is. 0.5 matches a plain linear gradient; higher values spread the blend across more of the screen, lower values sharpen it.")]
        private float spread = 0.5f;

        [SerializeField, Range(-180f, 180f), Tooltip("Rotates the gradient axis. 0 runs bottom to top, 90 runs left to right, -90 runs right to left, 45 is a diagonal.")]
        private float angle;

        [SerializeField, Range(0f, 2f), Tooltip("How fast the gradient drifts back and forth along its axis. 0 disables the animation.")]
        private float shiftSpeed;

        [SerializeField, Range(0f, 0.5f), Tooltip("How far the gradient drifts from its configured position as it animates.")]
        private float shiftAmplitude;

        [SerializeField, Range(-180f, 180f), Tooltip("Degrees per second the gradient axis rotates. 0 disables rotation; negative values reverse the direction.")]
        private float rotationSpeed;

        internal Gradient ColorGradient => colorGradient;
        internal float Midpoint => midpoint;
        internal float Spread => spread;
        internal float Angle => angle;
        internal float ShiftSpeed => shiftSpeed;
        internal float ShiftAmplitude => shiftAmplitude;
        internal float RotationSpeed => rotationSpeed;

        public override IBFBackgroundLayer CreateLayer()
        {
            return new BFGradientLayer(this);
        }

        private static Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.039f, 0.047f, 0.118f), 0f),
                    new GradientColorKey(new Color(0.016f, 0.024f, 0.067f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
            return gradient;
        }
    }
}