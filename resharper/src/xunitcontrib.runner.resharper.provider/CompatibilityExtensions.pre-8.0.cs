using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class CompatibilityExtensions
    {
        public static FileSystemPath GetOutputFilePath(this IProject project)
        {
            return project.GetOutputAssemblyFile().Location;
        }

        public static PredefinedType GetPredefinedType(this ITypeMember scope)
        {
            return scope.Module.GetPredefinedType();
        }
    }
}