using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BFTools.Core.Logger
{
    public class FileSink : IBFLoggerSink
    {
        private const long MaxFileSizeBytes = 1 * 1024 * 1024;
        private const string LogFileName = "bftools.log";
        private const string PreviousLogFileName = "bftools.log.bak";

        private readonly string logFilePath;
        private readonly string previousLogFilePath;
        private bool disabledAfterFailure;

        public FileSink()
        {
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            logFilePath = Path.Combine(logDirectory, LogFileName);
            previousLogFilePath = Path.Combine(logDirectory, PreviousLogFileName);

            try
            {
                Directory.CreateDirectory(logDirectory);
            }
            catch (Exception exception)
            {
                disabledAfterFailure = true;
                Debug.LogWarning($"[FileSink] Failed to create log directory '{logDirectory}'. File logging is disabled for this session. {exception}");
            }
        }

        public void Write(LogLevel level, string[] tags, string message, UnityEngine.Object context, bool includeStackTrace)
        {
            if (level < LogLevel.Warning)
                return;

            if (disabledAfterFailure)
                return;

            try
            {
                RotateIfNeeded();

                string tagText = tags != null && tags.Length > 0 ? string.Join(",", tags) : string.Empty;
                string line = $"{DateTime.UtcNow:O} [{level}] [{tagText}] {message}";

                if (includeStackTrace)
                    line += Environment.NewLine + Environment.StackTrace;

                File.AppendAllText(logFilePath, line + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                disabledAfterFailure = true;
                Debug.LogWarning($"[FileSink] Failed to write to log file '{logFilePath}'. File logging is disabled for this session. {exception}");
            }
        }

        private void RotateIfNeeded()
        {
            if (!File.Exists(logFilePath))
                return;

            if (new FileInfo(logFilePath).Length < MaxFileSizeBytes)
                return;

            if (File.Exists(previousLogFilePath))
                File.Delete(previousLogFilePath);

            File.Move(logFilePath, previousLogFilePath);
        }
    }
}