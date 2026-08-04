using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using BFTools.Core.Logger;

namespace BFTools.Systems.SceneManager
{
    public static class BFSceneLoader
    {
        private const string LogTag = "SceneManager";

        private static readonly Dictionary<string, AsyncOperation> operations = new Dictionary<string, AsyncOperation>();

        public static Task LoadAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (operations.ContainsKey(sceneName))
            {
                BFLogger.Debug(LogTag, $"'{sceneName}' is already loading or loaded. Ignoring duplicate load request.");
                return Task.CompletedTask;
            }

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            operations[sceneName] = operation;
            BFLogger.Debug(LogTag, $"Loading '{sceneName}' ({mode}).");

            return AwaitCompletion(sceneName, operation);
        }

        public static void Preload(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (operations.ContainsKey(sceneName))
            {
                BFLogger.Debug(LogTag, $"'{sceneName}' is already loading or loaded. Ignoring duplicate preload request.");
                return;
            }

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            operation.allowSceneActivation = false;
            operations[sceneName] = operation;
            BFLogger.Debug(LogTag, $"Preloading '{sceneName}' ({mode}).");
        }

        public static Task ActivateAsync(string sceneName)
        {
            if (!operations.TryGetValue(sceneName, out AsyncOperation operation))
            {
                BFLogger.Error(LogTag, $"Activate requested for '{sceneName}' but it is not preloaded.");
                return Task.CompletedTask;
            }

            BFLogger.Debug(LogTag, $"Activating '{sceneName}'.");
            operation.allowSceneActivation = true;
            return AwaitCompletion(sceneName, operation);
        }

        public static bool IsTracked(string sceneName) => operations.ContainsKey(sceneName);

        public static float GetProgress(string sceneName)
        {
            if (!operations.TryGetValue(sceneName, out AsyncOperation operation))
                return 1f;

            return operation.progress;
        }

        public static Task UnloadAsync(string sceneName)
        {
            operations.Remove(sceneName);

            if (!UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                BFLogger.Debug(LogTag, $"'{sceneName}' is not loaded. Skipping unload.");
                return Task.CompletedTask;
            }

            BFLogger.Debug(LogTag, $"Unloading '{sceneName}'.");

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            return AwaitCompletion(operation);
        }

        private static Task AwaitCompletion(string sceneName, AsyncOperation operation)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            operation.completed += _ =>
            {
                operations.Remove(sceneName);
                tcs.SetResult(true);
            };
            return tcs.Task;
        }

        private static Task AwaitCompletion(AsyncOperation operation)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            operation.completed += _ => tcs.SetResult(true);
            return tcs.Task;
        }
    }
}