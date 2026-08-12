using BFTools.Core.Logger;

namespace BFTools.Systems.SaveSystem
{
    internal static class BFSaveVersionMigrator
    {
        private const string LogTag = "Save";

        public const int CurrentVersion = 1;

        public static object Migrate(object data, int fromVersion)
        {
            if (fromVersion == CurrentVersion)
            {
                BFLogger.Trace(LogTag, $"No migration needed, data already at version {CurrentVersion}");
                return data;
            }

            // Migration steps added here as versions increment.

            BFLogger.Warning(LogTag, $"No migration steps implemented for version {fromVersion} to {CurrentVersion}; passing data through unchanged.");

            return data;
        }
    }
}