using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BFTools.Core.FileIO;
using BFTools.Core.Logger;
using BFTools.Core.Serialization;

namespace BFTools.Systems.SettingsManager
{
    public static class BFSettingsManager
    {
        private const string LogTag = "Settings";
        private const string FileName = "settings.json";

        private class BFSettingsData
        {
            public Dictionary<string, object> providerStates = new Dictionary<string, object>();
        }

        private static readonly BFAllowlistJsonSerializer serializer = new BFAllowlistJsonSerializer(LogTag);
        private static readonly BFStateRegistry<ISettingsProvider> providers = new BFStateRegistry<ISettingsProvider>(LogTag, serializer);

        public static void Register(ISettingsProvider provider)
        {
            providers.Register(provider);
        }

        public static void Unregister(ISettingsProvider provider)
        {
            providers.Unregister(provider);
        }

        public static Task<bool> SaveAsync()
        {
            return SaveAsync(BFPersistentDataPath.Directory);
        }

        public static Task<bool> LoadAsync()
        {
            return LoadAsync(BFPersistentDataPath.Directory);
        }

        public static async Task<bool> SaveAsync(string directoryPath)
        {
            BFLogger.Trace(LogTag, "SaveAsync started");

            string filePath = Path.Combine(directoryPath, FileName);

            try
            {
                BFSettingsData settingsData = new BFSettingsData
                {
                    providerStates = providers.CaptureAll()
                };

                string json = serializer.Serialize(settingsData);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                await BFFileIO.WriteAsync(filePath, bytes).ConfigureAwait(false);

                BFLogger.Trace(LogTag, $"SaveAsync completed ({bytes.Length} byte(s) written to '{filePath}')");
            }
            catch (Exception exception)
            {
                BFLogger.Warning(LogTag, $"Failed to save settings: {exception.Message}. The settings file may now be missing or incomplete.");
                return false;
            }

            return true;
        }

        public static async Task<bool> LoadAsync(string directoryPath)
        {
            BFLogger.Trace(LogTag, "LoadAsync started");

            string filePath = Path.Combine(directoryPath, FileName);
            byte[] bytes;

            try
            {
                bytes = await BFFileIO.ReadAsync(filePath).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                BFLogger.Warning(LogTag, $"Failed to read settings file: {exception.Message}. Using defaults.");
                return false;
            }

            if (bytes == null)
            {
                BFLogger.Trace(LogTag, "No settings file found, using defaults");
                return false;
            }

            string json = Encoding.UTF8.GetString(bytes);
            BFSettingsData settingsData;

            try
            {
                settingsData = serializer.Deserialize<BFSettingsData>(json);
            }
            catch (Exception exception)
            {
                BFLogger.Warning(LogTag, $"Failed to parse settings file: {exception.Message}. Using defaults.");
                return false;
            }

            if (settingsData == null)
            {
                BFLogger.Warning(LogTag, "Settings file was empty or invalid. Using defaults.");
                return false;
            }

            providers.RestoreAll(settingsData.providerStates);

            BFLogger.Trace(LogTag, "LoadAsync completed");

            return true;
        }
    }
}