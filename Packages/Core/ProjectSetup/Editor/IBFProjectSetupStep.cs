namespace BFTools.Core.ProjectSetup.Editor
{
    public interface IBFProjectSetupStep
    {
        int Order { get; }
        string DisplayName { get; }
        string Run();
    }
}