using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class XunitRunnerAssemblyRegistrar
    {
        public XunitRunnerAssemblyRegistrar(UnitTestingAssemblyLoader assemblyLoader)
        {
            // This is required so that ReSharper can correctly deserialise messages from the external
            // runner. The tasks are serialised by the external runner process with a fully qualified
            // type name which is used to create a new instance in the ReSharper process. Somehow, the
            // bit of Reflection magic used to create the new instance fails to find the type, even
            // though the task runner is already loaded into the process (since it's a direct assembly
            // reference of this assembly). The UnitTestingAssemblyLoader will use an assembly resolve
            // event to make sure it's loaded properly
            assemblyLoader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }
    }
}