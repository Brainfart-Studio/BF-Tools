using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BFTools.Core.Logger;

namespace BFTools.Core.Logger.Tests
{
    public class UnityConsoleSinkTests
    {
        private UnityConsoleSink sink;

        [SetUp]
        public void SetUp()
        {
            sink = new UnityConsoleSink();
        }

        [Test]
        public void Write_IncludesTagAndMessageInFormattedOutput()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[Gameplay\].*hello"));

            sink.Write(LogLevel.Info, new[] { "Gameplay" }, "hello", null, false);
        }

        [TestCase(LogLevel.Trace, LogType.Log)]
        [TestCase(LogLevel.Debug, LogType.Log)]
        [TestCase(LogLevel.Info, LogType.Log)]
        [TestCase(LogLevel.Warning, LogType.Warning)]
        [TestCase(LogLevel.Error, LogType.Error)]
        [TestCase(LogLevel.Critical, LogType.Error)]
        public void Write_MapsLogLevelToExpectedUnityLogType(LogLevel level, LogType expectedLogType)
        {
            LogAssert.Expect(expectedLogType, new Regex(".*"));

            sink.Write(level, new[] { "Tag" }, "message", null, false);
        }
    }
}