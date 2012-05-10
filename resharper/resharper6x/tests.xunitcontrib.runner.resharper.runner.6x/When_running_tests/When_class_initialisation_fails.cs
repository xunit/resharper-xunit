namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_class_fixture_cleanup_fails
    {
        // An exception is thrown in a test class' fixture object's dispose method
        // IRunnerLogger.ClassFailed
    }

    public class When_a_catastrophic_error_occurs
    {
        // An exception is thrown in the xunit infrastructure. Not related to test failures
        // Calls IRunnerLogger.ExceptionThrown, then nothing else is called - we're done
        // Should call TaskServer.TaskException + TaskServer.TaskFinished on class
    }
}