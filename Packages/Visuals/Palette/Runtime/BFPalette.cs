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
        [SerializeField] private string selectedEntryName;

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

            if (!string.IsNullOrEmpty(selectedEntryName))
                Select(selectedEntryName);
        }

        private void OnDisable()
        {
            EventBus<BFPaletteEvent>.Unsubscribe(OnPaletteEvent);
            EventBus<BFPaletteConfigChangedEvent>.Unsubscribe(OnPaletteConfigChanged);
        }

        public void Select(string eventName)
        {
            currentEventName = eventName;
            Apply(eventName);
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

        public void Apply(string eventName)
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
                    GetTargetRenderer().color = entries[i].color;
                    BFLogger.Trace(LogTag, $"Applied palette entry '{eventName}' on '{name}'.", this);
                    return;
                }
            }

            BFLogger.Trace(LogTag, $"No palette entry found for name '{eventName}'.", this);
        }

        private SpriteRenderer GetTargetRenderer()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponent<SpriteRenderer>();

            return targetRenderer;
        }
    }
}