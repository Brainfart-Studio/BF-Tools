using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace BFTools.Core.Logger
{
    public static class BFLogger
    {
        private static readonly List<IBFLoggerSink> sinks = new List<IBFLoggerSink>();
        private static BFLoggerConfig config;

        public static void Initialize(BFLoggerConfig loggerConfig, params IBFLoggerSink[] activeSinks)
        {
            config = loggerConfig;
            sinks.Clear();
            if (activeSinks != null)
                sinks.AddRange(activeSinks);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Trace(string tag, string message, Object context = null) => Log(LogLevel.Trace, new[] { tag }, message, context);

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Trace(string[] tags, string message, Object context = null) => Log(LogLevel.Trace, tags, message, context);

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string tag, string message, Object context = null) => Log(LogLevel.Debug, new[] { tag }, message, context);

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string[] tags, string message, Object context = null) => Log(LogLevel.Debug, tags, message, context);

        public static void Info(string tag, string message, Object context = null) => Log(LogLevel.Info, new[] { tag }, message, context);
        public static void Info(string[] tags, string message, Object context = null) => Log(LogLevel.Info, tags, message, context);

        public static void Warning(string tag, string message, Object context = null) => Log(LogLevel.Warning, new[] { tag }, message, context);
        public static void Warning(string[] tags, string message, Object context = null) => Log(LogLevel.Warning, tags, message, context);

        public static void Error(string tag, string message, Object context = null) => Log(LogLevel.Error, new[] { tag }, message, context);
        public static void Error(string[] tags, string message, Object context = null) => Log(LogLevel.Error, tags, message, context);

        public static void Critical(string tag, string message, Object context = null) => Log(LogLevel.Critical, new[] { tag }, message, context);
        public static void Critical(string[] tags, string message, Object context = null) => Log(LogLevel.Critical, tags, message, context);

        private static void Log(LogLevel level, string[] tags, string message, Object context)
        {
            if (config == null || !ShouldLog(level, tags))
                return;

            bool includeStackTrace = level >= config.StackTraceMinimumLevel;

            for (int i = 0; i < sinks.Count; i++)
                sinks[i].Write(level, tags, message, context, includeStackTrace);
        }

        private static bool ShouldLog(LogLevel level, string[] tags)
        {
            LogLevel minimumLevel = ResolveMinimumLevel(tags);
            if (level < minimumLevel)
                return false;

            if (config.UseTagAllowlist && !IsTagAllowed(tags))
                return false;

            return true;
        }

        private static LogLevel ResolveMinimumLevel(string[] tags)
        {
            LogLevel minimumLevel = config.GlobalMinimumLevel;

            if (tags == null)
                return minimumLevel;

            for (int i = 0; i < tags.Length; i++)
            {
                for (int j = 0; j < config.TagLevelOverrides.Count; j++)
                {
                    BFLoggerConfig.TagLevelOverride overrideEntry = config.TagLevelOverrides[j];
                    if (overrideEntry.tag == tags[i] && overrideEntry.minimumLevel < minimumLevel)
                        minimumLevel = overrideEntry.minimumLevel;
                }
            }

            return minimumLevel;
        }

        private static bool IsTagAllowed(string[] tags)
        {
            if (tags == null)
                return false;

            for (int i = 0; i < tags.Length; i++)
            {
                for (int j = 0; j < config.TagAllowlist.Count; j++)
                {
                    if (config.TagAllowlist[j] == tags[i])
                        return true;
                }
            }

            return false;
        }
    }
}