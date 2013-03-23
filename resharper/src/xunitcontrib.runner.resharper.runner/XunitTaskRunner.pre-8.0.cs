using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    // ReSharper 7.1 has made Start/Execute/Finish obsolete - they are going to be
    // replaced with ExecuteRecursive in future versions
    // These methods are not called if you implement RecursiveTaskRunner, marked as
    // obsolete in ReSharper 7.1 and removed in 8.0
#pragma warning disable 672
    public partial class XunitTaskRunner
    {
        public override sealed TaskResult Start(TaskExecutionNode node)
        {
            return TaskResult.Success;
        }

        public override sealed TaskResult Execute(TaskExecutionNode node)
        {
            return TaskResult.Success;
        }

        public override sealed TaskResult Finish(TaskExecutionNode node)
        {
            return TaskResult.Success;
        }
    }
#pragma warning restore 672
}
