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
        private static readonly Dictionary<string, Task> pendingLoadTasks = new Dictionary<string, Task>();

        public static Task LoadAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                BFLogger.Error(LogTag, "Load requested with a null or empty scene name. Ignoring.");
                return Task.CompletedTask;
            }

            if (operations.ContainsKey(sceneName))
            {
                if (pendingLoadTasks.TryGetValue(sceneName, out Task pendingTask))
                {
                    BFLogger.Debug(LogTag, $"'{sceneName}' is already loading. Returning the in-flight load's task instead of starting a new one.");
                    return pendingTask;
                }

                BFLogger.Debug(LogTag, $"'{sceneName}' is already loading or loaded. Ignoring duplicate load request.");
                return Task.CompletedTask;
            }

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            operations[sceneName] = operation;
            BFLogger.Debug(LogTag, $"Loading '{sceneName}' ({mode}).");

            Task loadTask = AwaitCompletion(operation, sceneName);
            pendingLoadTasks[sceneName] = loadTask;
            return loadTask;
        }

        public static void Preload(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                BFLogger.Error(LogTag, "Preload requested with a null or empty scene name. Ignoring.");
                return;
            }

            if (IsAlreadyTracked(sceneName, "preload"))
                return;

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            operation.allowSceneActivation = false;
            operations[sceneName] = operation;
            BFLogger.Debug(LogTag, $"Preloading '{sceneName}' ({mode}).");
        }

        private static bool IsAlreadyTracked(string sceneName, string actionLabel)
        {
            if (!operations.ContainsKey(sceneName))
                return false;

            BFLogger.Debug(LogTag, $"'{sceneName}' is already loading or loaded. Ignoring duplicate {actionLabel} request.");
            return true;
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
            return AwaitCompletion(operation, sceneName);
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

        private static Task AwaitCompletion(AsyncOperation operation, string sceneNameToUntrack = null)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            operation.completed += _ =>
            {
                if (sceneNameToUntrack != null)
                {
                    operations.Remove(sceneNameToUntrack);
                    pendingLoadTasks.Remove(sceneNameToUntrack);
                }

                tcs.SetResult(true);
            };
            return tcs.Task;
        }
    }
}