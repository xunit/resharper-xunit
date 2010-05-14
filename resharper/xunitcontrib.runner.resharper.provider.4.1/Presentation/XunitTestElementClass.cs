using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.CodeInsight.Services.CamelTyping;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestExplorer;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitTestElementClass : XunitTestElement
    {
        readonly string assemblyLocation;

        internal XunitTestElementClass(IUnitTestProvider provider,
                                       IProject project,
                                       string typeName,
                                       string assemblyLocation)
            : base(provider, null, project, typeName)
        {
            this.assemblyLocation = assemblyLocation;
        }

        internal string AssemblyLocation
        {
            get { return assemblyLocation; }
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            var solution = GetSolution();
            if (solution == null)
                return null;

            var manager = PsiManager.GetInstance(solution);
            var scope = DeclarationsCacheScope.ProjectScope(GetProject(), false);
            var cache = manager.GetDeclarationsCache(scope, true);
            return cache.GetTypeElementByCLRName(GetTypeClrName());
        }

        public override string GetKind()
        {
            return "xUnit.net Test Class";
        }

        public override string GetTitle()
        {
            return new CLRTypeName(GetTypeClrName()).ShortName;
        }

        public override bool Matches(string filter, PrefixMatcher matcher)
        {
            return GetCategories().Any(category => matcher.IsMatch(category.Name)) || matcher.IsMatch(GetTypeClrName());
        }
    }
}