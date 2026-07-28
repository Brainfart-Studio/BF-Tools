using UnityEngine;

namespace BFTools.Core.Logger
{
    public interface IBFLoggerSink
    {
        void Write(LogLevel level, string[] tags, string message, Object context, bool includeStackTrace);
    }
}