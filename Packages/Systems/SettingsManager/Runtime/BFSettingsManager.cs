using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BFTools.Core.Logger;
using BFTools.Systems.SaveSystem;

namespace BFTools.Systems.SettingsManager
{
    public static class BFSettingsManager
    {
        private const string LogTag = "Settings";
        private const string FileName = "settings.json";

        private static readonly List<ISettingsProvider> providers = new List<ISettingsProvider>();
        private static readonly HashSet<Type> registeredStateTypes = new HashSet<Type>();

        public static void Register(ISettingsProvider provider)
        {
            if (!providers.Contains(provider))
            {
                providers.Add(provider);

                if (registeredStateTypes.Add(provider.StateType))
                    BFSaveSerializer.AllowType(provider.StateType);

                BFLogger.Trace(LogTag, $"Registered {provider.GetType().Name}");
            }
        }

        public static void Unregister(ISettingsProvider provider)
        {
            if (providers.Remove(provider))
                BFLogger.Trace(LogTag, $"Unregistered {provider.GetType().Name}");
        }

        public static Task SaveAsync()
        {
            return SaveAsync(BFSavePath.DefaultDirectory);
        }

        public static Task<bool> LoadAsync()
        {
            return LoadAsync(BFSavePath.DefaultDirectory);
        }

        public static async Task SaveAsync(string directoryPath)
        {
            BFLogger.Trace(LogTag, "SaveAsync started");

            Dictionary<string, object> providerStates = new Dictionary<string, object>();
            for (int i = 0; i < providers.Count; i++)
            {
                ISettingsProvider provider = providers[i];
                providerStates[provider.GetType().Name] = provider.CaptureState();
            }

            BFLogger.Debug(LogTag, $"Captured state from {providers.Count} provider(s)");

            string json = BFSaveSerializer.Serialize(providerStates);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            string filePath = Path.Combine(directoryPath, FileName);
            await BFSaveFileIO.WriteAsync(filePath, bytes);

            BFLogger.Trace(LogTag, $"SaveAsync completed ({bytes.Length} byte(s) written to '{filePath}')");
        }

        public static async Task<bool> LoadAsync(string directoryPath)
        {
            BFLogger.Trace(LogTag, "LoadAsync started");

            string filePath = Path.Combine(directoryPath, FileName);
            byte[] bytes = await BFSaveFileIO.ReadAsync(filePath);

            if (bytes == null)
            {
                BFLogger.Trace(LogTag, "No settings file found, using defaults");
                return false;
            }

            string json = Encoding.UTF8.GetString(bytes);
            Dictionary<string, object> providerStates;

            try
            {
                providerStates = BFSaveSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception exception)
            {
                BFLogger.Warning(LogTag, $"Failed to parse settings file: {exception.Message}. Using defaults.");
                return false;
            }

            if (providerStates == null)
            {
                BFLogger.Warning(LogTag, "Settings file was empty or invalid. Using defaults.");
                return false;
            }

            int restoredCount = 0;
            for (int i = 0; i < providers.Count; i++)
            {
                ISettingsProvider provider = providers[i];
                string key = provider.GetType().Name;

                if (providerStates.TryGetValue(key, out object state))
                {
                    provider.RestoreState(state);
                    restoredCount++;
                }
                else
                {
                    BFLogger.Trace(LogTag, $"No saved state found for '{key}', leaving as-is");
                }
            }

            BFLogger.Debug(LogTag, $"Restored state for {restoredCount} of {providers.Count} provider(s)");
            BFLogger.Trace(LogTag, "LoadAsync completed");

            return true;
        }
    }
}