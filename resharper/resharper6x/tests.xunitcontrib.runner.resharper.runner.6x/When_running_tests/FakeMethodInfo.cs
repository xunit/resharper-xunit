using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    // Annoyingly, we need this to support theories and inline data
    public class FakeMethodInfo : MethodInfo
    {
        private readonly Method method;

        public FakeMethodInfo(Method method)
        {
            this.method = method;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return (from y in method.GetCustomAttributes(attributeType)
                    select y.GetInstance<Attribute>()).Cast<object>().ToArray();
        }

        private class FakeParameterInfo : ParameterInfo
        {
            private readonly string name;
            private readonly Type parameterType;

            public FakeParameterInfo(string name, Type parameterType)
            {
                this.name = name;
                this.parameterType = parameterType;
            }

            public override Type ParameterType { get { return parameterType; } }
            public override string Name { get { return name; } }
        }

        public override ParameterInfo[] GetParameters()
        {
            return (from p in method.Parameters
                    select (ParameterInfo) new FakeParameterInfo(p.Name, p.Type)).ToArray();
        }

        #region Not used

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override Type DeclaringType
        {
            get { throw new NotImplementedException(); }
        }

        public override Type ReflectedType
        {
            get { throw new NotImplementedException(); }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { throw new NotImplementedException(); }
        }

        public override MethodAttributes Attributes
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}