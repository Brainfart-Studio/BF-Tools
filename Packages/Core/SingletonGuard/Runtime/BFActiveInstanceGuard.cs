namespace BFTools.Core.SingletonGuard
{
    public static class BFActiveInstanceGuard<TOwner> where TOwner : class
    {
        private static TOwner activeInstance;

        public static bool TryActivate(TOwner instance)
        {
            if (activeInstance != null)
                return false;

            activeInstance = instance;
            return true;
        }

        public static bool IsActive(TOwner instance)
        {
            return activeInstance == instance;
        }

        public static void Deactivate(TOwner instance)
        {
            if (activeInstance == instance)
                activeInstance = null;
        }

        public static void ResetState()
        {
            activeInstance = null;
        }
    }
}