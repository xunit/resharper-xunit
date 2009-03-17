using JetBrains.ProjectModel;
using JetBrains.ReSharper.CodeInsight.Services.CamelTyping;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestExplorer;

namespace XunitContrib.Runner.ReSharper
{
    public class XunitTestElementClass : XunitTestElement
    {
        readonly string assemblyLocation;

        public XunitTestElementClass(IUnitTestProvider provider,
                                     IProjectModelElement project,
                                     string typeName,
                                     string assemblyLocation)
            : base(provider, null, project, typeName)
        {
            this.assemblyLocation = assemblyLocation;
        }

        public string AssemblyLocation
        {
            get { return assemblyLocation; }
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            ISolution solution = GetSolution();

            if (solution == null)
                return null;

            PsiManager manager = PsiManager.GetInstance(solution);
            DeclarationsCacheScope scope = DeclarationsCacheScope.ProjectScope(GetProject(), false);
            IDeclarationsCache cache = manager.GetDeclarationsCache(scope, true);
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
            foreach (UnitTestElementCategory category in GetCategories())
                if (matcher.IsMatch(category.Name))
                    return true;

            return matcher.IsMatch(GetTypeClrName());
        }
    }
}