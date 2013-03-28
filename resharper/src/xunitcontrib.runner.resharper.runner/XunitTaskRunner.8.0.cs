namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class XunitTaskRunner
    {
        // 8.0 moved the abort mechanism from returning bools in IRemoteTaskServer
        // to a method on RecursiveRemoteTaskRunner
        public override void Abort()
        {
            taskServer.ShouldContinue = false;
        }
    }
}
