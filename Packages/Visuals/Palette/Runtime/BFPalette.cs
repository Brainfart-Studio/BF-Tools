using System.Collections.Generic;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.Logger;

namespace BFTools.Visuals.Palette
{
    public struct BFPaletteEvent
    {
        public string eventName;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class BFPalette : MonoBehaviour
    {
        private const string LogTag = "Palette";

        [SerializeField] private BFPaletteConfig config;

        private SpriteRenderer targetRenderer;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            EventBus<BFPaletteEvent>.Subscribe(OnPaletteEvent);
        }

        private void OnDisable()
        {
            EventBus<BFPaletteEvent>.Unsubscribe(OnPaletteEvent);
        }

        private void OnPaletteEvent(BFPaletteEvent evt)
        {
            if (config == null)
            {
                BFLogger.Warning(LogTag, $"No config assigned on '{name}'.", this);
                return;
            }

            IReadOnlyList<BFPaletteEntry> entries = config.Entries;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].name == evt.eventName)
                {
                    targetRenderer.color = entries[i].color;
                    BFLogger.Trace(LogTag, $"Applied palette entry '{evt.eventName}' on '{name}'.", this);
                    return;
                }
            }

            BFLogger.Trace(LogTag, $"No palette entry found for name '{evt.eventName}'.", this);
        }
    }
}