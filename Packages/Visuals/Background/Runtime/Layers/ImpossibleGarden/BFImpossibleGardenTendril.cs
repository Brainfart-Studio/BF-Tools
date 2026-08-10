using UnityEngine;

namespace BFTools.Visuals.Background
{
    internal sealed class BFImpossibleGardenTendril
    {
        public readonly int BloomIndex;
        public readonly int Index;

        public readonly float Phase;
        public readonly float CurvatureSeed;
        public readonly float LengthSeed;
        public readonly float WidthSeed;
        public readonly float FailureSeed;

        public int TargetIndex { get; private set; }
        public bool HasTarget { get; private set; }

        public BFImpossibleGardenTendril(
            int bloomIndex,
            int index,
            System.Random random,
            int bloomCount)
        {
            BloomIndex = bloomIndex;
            Index = index;

            Phase = (float)random.NextDouble() * Mathf.PI * 2f;
            CurvatureSeed = (float)random.NextDouble();
            LengthSeed = (float)random.NextDouble();
            WidthSeed = (float)random.NextDouble();
            FailureSeed = (float)random.NextDouble();

            ChooseTarget(random, bloomCount);
        }

        public void ChooseTarget(System.Random random, int bloomCount)
        {
            if (bloomCount <= 1)
            {
                HasTarget = false;
                TargetIndex = -1;
                return;
            }

            TargetIndex = random.Next(bloomCount - 1);

            if (TargetIndex >= BloomIndex)
                TargetIndex++;

            HasTarget = true;
        }

        public bool ShouldFail(float failureChance)
        {
            return FailureSeed < failureChance;
        }
    }
}