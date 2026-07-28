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

        public FileSink()
        {
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(logDirectory);
            logFilePath = Path.Combine(logDirectory, LogFileName);
            previousLogFilePath = Path.Combine(logDirectory, PreviousLogFileName);
        }

        public void Write(LogLevel level, string[] tags, string message, UnityEngine.Object context, bool includeStackTrace)
        {
            if (level < LogLevel.Warning)
                return;

            RotateIfNeeded();

            string tagText = tags != null && tags.Length > 0 ? string.Join(",", tags) : string.Empty;
            string line = $"{DateTime.UtcNow:O} [{level}] [{tagText}] {message}";

            if (includeStackTrace)
                line += Environment.NewLine + Environment.StackTrace;

            File.AppendAllText(logFilePath, line + Environment.NewLine, Encoding.UTF8);
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