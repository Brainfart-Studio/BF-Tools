using BFTools.Core.Logger;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveVersionMigrator
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

            BFLogger.Trace(LogTag, $"Migrated data from version {fromVersion} to {CurrentVersion}");

            return data;
        }
    }
}