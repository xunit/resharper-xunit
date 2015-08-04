using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Psi.Modules;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class CompatibilityExtensions
    {
        public static IPsiModule GetPrimaryPsiModule(this IPsiModules psiModules, IModule module)
        {
            return psiModules.GetPrimaryPsiModule(module, TargetFrameworkId.Default);
        }

        public static bool IsAutomaticCompletion(this CodeCompletionParameters parameters)
        {
            return parameters.IsAutomaticCompletion;
        }
    }
}