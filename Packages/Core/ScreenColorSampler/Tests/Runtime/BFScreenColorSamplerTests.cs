using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Core.ScreenColorSampler.Tests
{
    public class BFScreenColorSamplerTests
    {
        private GameObject go;
        private GameObject cameraGo;

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.DestroyImmediate(go);
            if (cameraGo != null)
                Object.DestroyImmediate(cameraGo);
        }

        private BFScreenColorSampler CreateSampler(float offsetX, float offsetY, float width, float height, Vector3 position, Camera cameraOverride = null)
        {
            go = new GameObject("ScreenColorSampler");
            go.transform.position = position;
            go.SetActive(false);

            BFScreenColorSampler sampler = go.AddComponent<BFScreenColorSampler>();
            SetField(sampler, "offsetX", offsetX);
            SetField(sampler, "offsetY", offsetY);
            SetField(sampler, "width", width);
            SetField(sampler, "height", height);
            if (cameraOverride != null)
                SetField(sampler, "targetCameraOverride", cameraOverride);

            go.SetActive(true);

            return sampler;
        }

        private Camera CreateOrthographicCamera(Vector3 position, float orthographicSize)
        {
            cameraGo = new GameObject("TestCamera");
            cameraGo.transform.position = position;
            cameraGo.transform.rotation = Quaternion.identity;

            Camera camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;

            return camera;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            typeof(BFScreenColorSampler)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        private static Vector3[] InvokeGetWorldCorners(BFScreenColorSampler sampler)
        {
            Vector3[] corners = new Vector3[4];
            typeof(BFScreenColorSampler)
                .GetMethod("GetWorldCorners", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(sampler, new object[] { corners });
            return corners;
        }

        private static bool InvokeTryGetViewportRect(BFScreenColorSampler sampler, Camera camera, out Rect rect)
        {
            object[] args = { camera, null };
            bool result = (bool)typeof(BFScreenColorSampler)
                .GetMethod("TryGetViewportRect", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(sampler, args);
            rect = (Rect)args[1];
            return result;
        }

        [Test]
        public void OnEnable_CreatesSampleRenderTexture()
        {
            BFScreenColorSampler sampler = CreateSampler(1f, 0f, 1f, 1f, Vector3.zero);

            RenderTexture texture = (RenderTexture)typeof(BFScreenColorSampler)
                .GetField("sampleTexture", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(sampler);

            Assert.IsNotNull(texture);
        }

        [Test]
        public void OnDisable_ReleasesSampleRenderTexture()
        {
            BFScreenColorSampler sampler = CreateSampler(1f, 0f, 1f, 1f, Vector3.zero);

            go.SetActive(false);

            RenderTexture texture = (RenderTexture)typeof(BFScreenColorSampler)
                .GetField("sampleTexture", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(sampler);

            Assert.IsNull(texture);
        }

        [Test]
        public void GetWorldCorners_CentersBoxOnTransformPositionPlusOffset()
        {
            BFScreenColorSampler sampler = CreateSampler(2f, 1f, 2f, 4f, new Vector3(5f, 5f, 5f));

            Vector3[] corners = InvokeGetWorldCorners(sampler);
            Vector3 expectedCenter = new Vector3(7f, 6f, 5f);

            Assert.AreEqual(expectedCenter + new Vector3(-1f, -2f, 0f), corners[0]);
            Assert.AreEqual(expectedCenter + new Vector3(1f, -2f, 0f), corners[1]);
            Assert.AreEqual(expectedCenter + new Vector3(1f, 2f, 0f), corners[2]);
            Assert.AreEqual(expectedCenter + new Vector3(-1f, 2f, 0f), corners[3]);
        }

        [Test]
        public void GetWorldCorners_ReflectsCurrentTransformPosition_NotCachedAtEnable()
        {
            BFScreenColorSampler sampler = CreateSampler(1f, 0f, 1f, 1f, Vector3.zero);

            go.transform.position = new Vector3(10f, 0f, 0f);
            Vector3[] corners = InvokeGetWorldCorners(sampler);
            Vector3 expectedCenter = new Vector3(11f, 0f, 0f);

            Assert.AreEqual(expectedCenter + new Vector3(-0.5f, -0.5f, 0f), corners[0]);
        }

        [Test]
        public void TryGetViewportRect_BoxDirectlyInFrontOfCamera_ReturnsRectCenteredInViewport()
        {
            Camera camera = CreateOrthographicCamera(new Vector3(0f, 0f, -10f), 5f);
            BFScreenColorSampler sampler = CreateSampler(0f, 0f, 2f, 2f, Vector3.zero, camera);

            bool found = InvokeTryGetViewportRect(sampler, camera, out Rect rect);

            Assert.IsTrue(found);
            Assert.AreEqual(0.5f, rect.center.x, 0.001f);
            Assert.AreEqual(0.5f, rect.center.y, 0.001f);
        }

        [Test]
        public void TryGetViewportRect_BoxBehindCamera_ReturnsFalse()
        {
            Camera camera = CreateOrthographicCamera(new Vector3(0f, 0f, -10f), 5f);
            BFScreenColorSampler sampler = CreateSampler(0f, 0f, 2f, 2f, new Vector3(0f, 0f, -20f), camera);

            bool found = InvokeTryGetViewportRect(sampler, camera, out Rect rect);

            Assert.IsFalse(found);
        }
    }
}