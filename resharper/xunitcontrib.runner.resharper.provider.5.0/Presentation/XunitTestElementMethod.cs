using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Text;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitTestElementMethod : XunitTestElement
    {
        readonly XunitTestElementClass @class;
        readonly string methodName;
        readonly int order;

        internal XunitTestElementMethod(IUnitTestProvider provider,
                                        XunitTestElementClass @class,
                                        ProjectModelElementEnvoy projectEnvoy,
                                        string declaringTypeName,
                                        string methodName,
                                        int order)
            : base(provider, @class, projectEnvoy, declaringTypeName)
        {
            this.@class = @class;
            this.order = order;
            this.methodName = methodName;
        }

        internal XunitTestElementClass Class
        {
            get { return @class; }
        }

        internal string MethodName
        {
            get { return methodName; }
        }

        public override string ShortName
        {
            get { return methodName; }
        }

        internal int Order
        {
            get { return order; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                var elementMethod = (XunitTestElementMethod)obj;

                if (Equals(@class, elementMethod.@class))
                    return (methodName == elementMethod.methodName);
            }

            return false;
        }

        public override IDeclaredElement GetDeclaredElement()
        {
            var declaredType = GetDeclaredType();
            if (declaredType != null)
            {
                return (from member in declaredType.EnumerateMembers(methodName, true)
                        let method = member as IMethod
                        where method != null && !method.IsAbstract && method.TypeParameters.Length <= 0 && method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC
                        select member).FirstOrDefault();
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

        public override bool Matches(string filter, IdentifierMatcher matcher)
        {
            return @class.Matches(filter, matcher) || matcher.Matches(methodName);
        }
    }
}