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

    public struct BFPaletteConfigChangedEvent
    {
        public BFPaletteConfig config;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class BFPalette : MonoBehaviour
    {
        private const string LogTag = "Palette";

        [SerializeField] private BFPaletteConfig config;

        private SpriteRenderer targetRenderer;
        private string currentEventName;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            EventBus<BFPaletteEvent>.Subscribe(OnPaletteEvent);
            EventBus<BFPaletteConfigChangedEvent>.Subscribe(OnPaletteConfigChanged);
        }

        private void OnDisable()
        {
            EventBus<BFPaletteEvent>.Unsubscribe(OnPaletteEvent);
            EventBus<BFPaletteConfigChangedEvent>.Unsubscribe(OnPaletteConfigChanged);
        }

        private void OnPaletteEvent(BFPaletteEvent evt)
        {
            currentEventName = evt.eventName;
            Apply(evt.eventName);
        }

        private void OnPaletteConfigChanged(BFPaletteConfigChangedEvent evt)
        {
            if (evt.config != config || currentEventName == null)
                return;

            Apply(currentEventName);
        }

        private void Apply(string eventName)
        {
            if (config == null)
            {
                BFLogger.Warning(LogTag, $"No config assigned on '{name}'.", this);
                return;
            }

            IReadOnlyList<BFPaletteEntry> entries = config.Entries;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].name == eventName)
                {
                    targetRenderer.color = entries[i].color;
                    BFLogger.Trace(LogTag, $"Applied palette entry '{eventName}' on '{name}'.", this);
                    return;
                }
            }

            BFLogger.Trace(LogTag, $"No palette entry found for name '{eventName}'.", this);
        }
    }
}