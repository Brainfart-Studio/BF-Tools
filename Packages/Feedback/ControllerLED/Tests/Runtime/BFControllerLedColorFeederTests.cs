using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Core.ScreenColorSampler;
using BFTools.Feedback.ControllerLED;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Feedback.ControllerLED.Tests
{
    public class BFControllerLedColorFeederTests
    {
        private GameObject feederGo;
        private GameObject samplerGo;
        private GameObject managerGo;

        [TearDown]
        public void TearDown()
        {
            EventBus<BFControllerLedEvent>.Clear();

            if (feederGo != null)
                Object.DestroyImmediate(feederGo);
            if (samplerGo != null)
                Object.DestroyImmediate(samplerGo);
            if (managerGo != null)
                Object.DestroyImmediate(managerGo);
        }

        private class SpyControllerLed : IBFControllerLed
        {
            public bool IsSupported => true;
            public Color? LastColor;
            public bool TurnOffCalled;

            public void SetColor(Color color) => LastColor = color;
            public void TurnOff() => TurnOffCalled = true;
        }

        private BFScreenColorSampler CreateSampler()
        {
            samplerGo = new GameObject("ScreenColorSampler");
            return samplerGo.AddComponent<BFScreenColorSampler>();
        }

        private (BFControllerLedManager manager, SpyControllerLed spy) CreateManagerWithSpy()
        {
            managerGo = new GameObject("ControllerLedManager");
            BFControllerLedManager manager = managerGo.AddComponent<BFControllerLedManager>();

            SpyControllerLed spy = new SpyControllerLed();
            typeof(BFControllerLedManager)
                .GetField("activeLed", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(manager, spy);

            return (manager, spy);
        }

        private BFControllerLedColorFeeder CreateFeeder(BFScreenColorSampler sampler, BFControllerLedManager manager)
        {
            feederGo = new GameObject("ControllerLedColorFeeder");
            feederGo.SetActive(false);

            BFControllerLedColorFeeder feeder = feederGo.AddComponent<BFControllerLedColorFeeder>();
            SetField(feeder, "colorSampler", sampler);
            SetField(feeder, "ledManager", manager);

            feederGo.SetActive(true);

            return feeder;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            typeof(BFControllerLedColorFeeder)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        private static void FireColorSampled(BFScreenColorSampler sampler, Color color)
        {
            FieldInfo eventField = typeof(BFScreenColorSampler)
                .GetField("ColorSampled", BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = (System.Action<Color>)eventField.GetValue(sampler);
            handler?.Invoke(color);
        }

        [Test]
        public void OnEnable_SamplerFiresColor_ForwardsColorToManager()
        {
            BFScreenColorSampler sampler = CreateSampler();
            (BFControllerLedManager manager, SpyControllerLed spy) = CreateManagerWithSpy();
            CreateFeeder(sampler, manager);

            FireColorSampled(sampler, Color.cyan);

            Assert.AreEqual(Color.cyan, spy.LastColor);
        }

        [Test]
        public void OnEnable_MultipleSamples_ForwardsEachLatestColor()
        {
            BFScreenColorSampler sampler = CreateSampler();
            (BFControllerLedManager manager, SpyControllerLed spy) = CreateManagerWithSpy();
            CreateFeeder(sampler, manager);

            FireColorSampled(sampler, Color.red);
            Assert.AreEqual(Color.red, spy.LastColor);

            FireColorSampled(sampler, Color.blue);
            Assert.AreEqual(Color.blue, spy.LastColor);
        }

        [Test]
        public void OnDisable_UnsubscribesFromSampler_SubsequentSampleIsIgnored()
        {
            BFScreenColorSampler sampler = CreateSampler();
            (BFControllerLedManager manager, SpyControllerLed spy) = CreateManagerWithSpy();
            CreateFeeder(sampler, manager);

            feederGo.SetActive(false);
            FireColorSampled(sampler, Color.green);

            Assert.IsNull(spy.LastColor);
        }
    }
}