namespace BFTools.Core.SaveSystem
{
    public static class BFSaveVersionMigrator
    {
        public const int CurrentVersion = 1;

        public static object Migrate(object data, int fromVersion)
        {
            if (fromVersion == CurrentVersion)
                return data;

            // Migration steps added here as versions increment.

            return data;
        }
    }
}