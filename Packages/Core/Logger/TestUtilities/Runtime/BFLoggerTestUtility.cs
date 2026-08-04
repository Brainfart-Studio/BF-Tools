using System;
using System.Collections.Generic;
using System.Reflection;
using BFTools.Core.Logger;

namespace BFTools.Core.Logger.TestUtilities
{
    public static class BFLoggerTestUtility
    {
        public static void ResetState()
        {
            Type loggerType = typeof(BFLogger);
            loggerType.GetField("initialized", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, false);
            loggerType.GetField("config", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
            List<IBFLoggerSink> sinks = (List<IBFLoggerSink>)loggerType
                .GetField("sinks", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
            sinks.Clear();
        }
    }
}