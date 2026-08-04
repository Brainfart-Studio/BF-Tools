using System.Collections.Generic;
using UnityEngine;
using BFTools.Core.Logger;

namespace BFTools.Feedback.Haptics.Tests
{
    internal class SpyLoggerSink : IBFLoggerSink
    {
        public struct Entry
        {
            public LogLevel Level;
            public string[] Tags;
            public string Message;
        }

        public readonly List<Entry> Entries = new List<Entry>();

        public void Write(LogLevel level, string[] tags, string message, Object context, bool includeStackTrace)
        {
            Entries.Add(new Entry { Level = level, Tags = tags, Message = message });
        }
    }
}