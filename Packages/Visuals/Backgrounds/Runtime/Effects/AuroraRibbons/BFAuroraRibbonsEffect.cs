using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Visuals.Background.AuroraRibbons
{
    public class BFAuroraRibbonsEffect : IBFBackgroundEffect
    {
        private readonly BFAuroraRibbonsConfig config;
        private readonly List<BFAuroraRibbon> ribbons = new List<BFAuroraRibbon>();
        private readonly List<BFBackgroundStar> stars = new List<BFBackgroundStar>();
        private float elapsedTime;

        internal IReadOnlyList<BFAuroraRibbon> Ribbons => ribbons;
        internal IReadOnlyList<BFBackgroundStar> Stars => stars;
        internal float ElapsedTime => elapsedTime;

        public BFAuroraRibbonsEffect(BFAuroraRibbonsConfig config)
        {
            this.config = config;
        }

        public void Init()
        {
            elapsedTime = 0f;

            ribbons.Clear();
            for (int i = 0; i < config.RibbonCount; i++)
                ribbons.Add(new BFAuroraRibbon(i, config));

            stars.Clear();
            for (int i = 0; i < config.StarCount; i++)
                stars.Add(new BFBackgroundStar(config));
        }

        public void Tick(float dt)
        {
            elapsedTime += dt * config.WaveSpeed;

            for (int i = 0; i < stars.Count; i++)
                stars[i].Tick(dt);

            for (int i = 0; i < ribbons.Count; i++)
                ribbons[i].Tick(elapsedTime);
        }

        public void Cleanup()
        {
            ribbons.Clear();
            stars.Clear();
        }
    }
}