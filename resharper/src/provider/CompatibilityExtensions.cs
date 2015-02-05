using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class CompatibilityExtensions
    {
        public static string GetOutputAssemblyPath(this IProject project)
        {
            return project.GetOutputFilePath().FullPath;
        }

        public static PredefinedType GetPredefinedType(this ITypeMember scope)
        {
            return scope.Module.GetPredefinedType(scope.ResolveContext);
        }
    }
}