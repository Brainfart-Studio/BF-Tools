using UnityEngine;

namespace BFTools.Core.Logger
{
    public class UnityConsoleSink : IBFLoggerSink
    {
        public void Write(LogLevel level, string[] tags, string message, Object context, bool includeStackTrace)
        {
            string formatted = Format(level, tags, message);
            LogType logType = ToLogType(level);
            LogOption logOption = includeStackTrace ? LogOption.None : LogOption.NoStacktrace;

            Debug.LogFormat(logType, logOption, context, "{0}", formatted);
        }

        private static string Format(LogLevel level, string[] tags, string message)
        {
            string tagText = tags != null && tags.Length > 0 ? string.Join(",", tags) : string.Empty;
            return $"<color={ColorFor(level)}>[{level}]</color> [{tagText}] {message}";
        }

        private static string ColorFor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return "grey";
                case LogLevel.Warning:
                    return "yellow";
                case LogLevel.Error:
                case LogLevel.Critical:
                    return "red";
                default:
                    return "white";
            }
        }

        private static LogType ToLogType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    return LogType.Warning;
                case LogLevel.Error:
                case LogLevel.Critical:
                    return LogType.Error;
                default:
                    return LogType.Log;
            }
        }
    }
}