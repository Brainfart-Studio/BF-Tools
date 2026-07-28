using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFTools.Core.Logger
{
    public class BFLoggerConfig : ScriptableObject
    {
        [Serializable]
        public struct TagLevelOverride
        {
            public string tag;
            public LogLevel minimumLevel;
        }

        [SerializeField] private LogLevel globalMinimumLevel = LogLevel.Info;
        [SerializeField] private List<TagLevelOverride> tagLevelOverrides = new List<TagLevelOverride>();
        [SerializeField] private bool useTagAllowlist = false;
        [SerializeField] private List<string> tagAllowlist = new List<string>();
        [SerializeField] private LogLevel stackTraceMinimumLevel = LogLevel.Error;

        internal LogLevel GlobalMinimumLevel => globalMinimumLevel;
        internal IReadOnlyList<TagLevelOverride> TagLevelOverrides => tagLevelOverrides;
        internal bool UseTagAllowlist => useTagAllowlist;
        internal IReadOnlyList<string> TagAllowlist => tagAllowlist;
        internal LogLevel StackTraceMinimumLevel => stackTraceMinimumLevel;
    }
}