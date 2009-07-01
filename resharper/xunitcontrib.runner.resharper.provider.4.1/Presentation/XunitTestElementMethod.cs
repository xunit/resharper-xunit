using JetBrains.ProjectModel;
using JetBrains.ReSharper.CodeInsight.Services.CamelTyping;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.UnitTestExplorer;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestElementMethod : XunitTestElement
    {
        readonly XunitTestElementClass @class;
        readonly string methodName;
        readonly int order;

        public XunitTestElementMethod(IUnitTestProvider provider,
                                      XunitTestElementClass @class,
                                      IProjectModelElement project,
                                      string declaringTypeName,
                                      string methodName,
                                      int order)
            : base(provider, @class, project, declaringTypeName)
        {
            this.@class = @class;
            this.order = order;
            this.methodName = methodName;
        }

        public XunitTestElementClass Class
        {
            get { return @class; }
        }

        public string MethodName
        {
            get { return methodName; }
        }

        public int Order
        {
            get { return order; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                XunitTestElementMethod elementMethod = (XunitTestElementMethod)obj;

                if (Equals(@class, elementMethod.@class))
                    return (methodName == elementMethod.methodName);
            }

            return false;
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            ITypeElement declaredType = GetDeclaredType();

            if (declaredType != null)
            {
                foreach (ITypeMember member in MiscUtil.EnumerateMembers(declaredType, methodName, true))
                {
                    IMethod method = member as IMethod;

                    if (method != null && !method.IsAbstract && method.TypeParameters.Length <= 0 &&
                        method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC)
                        return member;
                }
            }

            return null;
        }

        public override string GetKind()
        {
            return "xUnit.net Test";
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1}", @class.GetTitle(), methodName);
        }

        public override bool Matches(string filter, PrefixMatcher matcher)
        {
            foreach (UnitTestElementCategory category in GetCategories())
                if (matcher.IsMatch(category.Name))
                    return true;

            if (!@class.Matches(filter, matcher))
                return matcher.IsMatch(methodName);

            return true;
        }
    }
}