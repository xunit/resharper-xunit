namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public enum ServerAction
    {
        TaskDiscovered,
        TaskStarting,
        TaskProgress,
        TaskError,
        TaskOutput,
        TaskFinished,
        TaskDuration,
        TaskExplain,
        TaskException,
        CreateDynamicElement
    }
}