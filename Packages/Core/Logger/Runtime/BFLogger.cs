using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace BFTools.Core.Logger
{
    public static class BFLogger
    {
        private const string ResourcesConfigPath = "BFTools/BFLoggerConfig";

        private static readonly List<IBFLoggerSink> sinks = new List<IBFLoggerSink>();
        private static BFLoggerConfig config;
        private static bool initialized;

        public static void Initialize(BFLoggerConfig loggerConfig, params IBFLoggerSink[] activeSinks)
        {
            config = loggerConfig;
            sinks.Clear();
            if (activeSinks != null)
                sinks.AddRange(activeSinks);
            initialized = true;
        }

        private static void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;

            config = Resources.Load<BFLoggerConfig>(ResourcesConfigPath);
            if (config == null)
            {
                UnityEngine.Debug.LogWarning($"[BFLogger] No BFLoggerConfig found at Resources/{ResourcesConfigPath}. Using default config (Info level, console sink).");
                config = ScriptableObject.CreateInstance<BFLoggerConfig>();
            }

            if (sinks.Count == 0)
                sinks.Add(new UnityConsoleSink());
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Trace(string tag, string message, Object context = null) => Log(LogLevel.Trace, tag, message, context);

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Trace(string[] tags, string message, Object context = null) => Log(LogLevel.Trace, tags, message, context);

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string tag, string message, Object context = null) => Log(LogLevel.Debug, tag, message, context);

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string[] tags, string message, Object context = null) => Log(LogLevel.Debug, tags, message, context);

        public static void Info(string tag, string message, Object context = null) => Log(LogLevel.Info, tag, message, context);
        public static void Info(string[] tags, string message, Object context = null) => Log(LogLevel.Info, tags, message, context);

        public static void Warning(string tag, string message, Object context = null) => Log(LogLevel.Warning, tag, message, context);
        public static void Warning(string[] tags, string message, Object context = null) => Log(LogLevel.Warning, tags, message, context);

        public static void Error(string tag, string message, Object context = null) => Log(LogLevel.Error, tag, message, context);
        public static void Error(string[] tags, string message, Object context = null) => Log(LogLevel.Error, tags, message, context);

        public static void Critical(string tag, string message, Object context = null) => Log(LogLevel.Critical, tag, message, context);
        public static void Critical(string[] tags, string message, Object context = null) => Log(LogLevel.Critical, tags, message, context);

        private static void Log(LogLevel level, string tag, string message, Object context)
        {
            EnsureInitialized();

            if (!ShouldLog(level, tag))
                return;

            string[] tags = { tag };
            bool includeStackTrace = level >= config.StackTraceMinimumLevel;

            for (int i = 0; i < sinks.Count; i++)
                sinks[i].Write(level, tags, message, context, includeStackTrace);
        }

        private static void Log(LogLevel level, string[] tags, string message, Object context)
        {
            EnsureInitialized();

            if (!ShouldLog(level, tags))
                return;

            bool includeStackTrace = level >= config.StackTraceMinimumLevel;

            for (int i = 0; i < sinks.Count; i++)
                sinks[i].Write(level, tags, message, context, includeStackTrace);
        }

        private static bool ShouldLog(LogLevel level, string tag)
        {
            LogLevel minimumLevel = ResolveMinimumLevel(tag);
            if (level < minimumLevel)
                return false;

            if (config.UseTagAllowlist && !IsTagAllowed(tag))
                return false;

            return true;
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

        private static LogLevel ResolveMinimumLevel(string tag)
        {
            LogLevel overrideLevel = LogLevel.Critical;
            bool hasOverride = false;

            for (int j = 0; j < config.TagLevelOverrides.Count; j++)
            {
                BFLoggerConfig.TagLevelOverride overrideEntry = config.TagLevelOverrides[j];
                if (overrideEntry.tag != tag)
                    continue;

                hasOverride = true;
                if (overrideEntry.minimumLevel < overrideLevel)
                    overrideLevel = overrideEntry.minimumLevel;
            }

            return hasOverride ? overrideLevel : config.GlobalMinimumLevel;
        }

        private static LogLevel ResolveMinimumLevel(string[] tags)
        {
            if (tags == null)
                return config.GlobalMinimumLevel;

            bool hasOverride = false;
            LogLevel overrideLevel = LogLevel.Critical;

            for (int i = 0; i < tags.Length; i++)
            {
                for (int j = 0; j < config.TagLevelOverrides.Count; j++)
                {
                    BFLoggerConfig.TagLevelOverride overrideEntry = config.TagLevelOverrides[j];
                    if (overrideEntry.tag != tags[i])
                        continue;

                    hasOverride = true;
                    if (overrideEntry.minimumLevel < overrideLevel)
                        overrideLevel = overrideEntry.minimumLevel;
                }
            }

            return hasOverride ? overrideLevel : config.GlobalMinimumLevel;
        }

        private static bool IsTagAllowed(string tag)
        {
            for (int j = 0; j < config.TagAllowlist.Count; j++)
            {
                if (config.TagAllowlist[j] == tag)
                    return true;
            }

            return false;
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