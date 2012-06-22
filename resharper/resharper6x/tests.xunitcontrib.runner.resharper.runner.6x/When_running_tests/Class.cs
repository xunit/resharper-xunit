using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    // I don't like the fact that I have to implement ITypeInfo and also derive
    // from Type (the xunit sdk drops out of the ITypeInfo abstraction here and
    // there). Perhaps I should just implement (some of) Type and use Reflector
    // .Wrap to give me the same ITypeInfo xunit uses internally
    public class Class : ITypeInfo
    {
        private readonly IList<Method> methodInfos;
        private readonly FakeType fakeType;
        private Action constructor;
        private Action dispose;

        public Class(string typeName, string assemblyLocation = "assembly1.dll")
        {
            var typeShortName = typeName.Split('.').Last();
            var typeNamespace = typeName.Replace("." + typeShortName, "");

            fakeType = new FakeType(typeNamespace, typeShortName);

            ClassTask = new XunitTestClassTask(assemblyLocation, typeName, true);
            methodInfos = new List<Method>();
            SetConstructor(() => { });
        }

        public string Typename { get { return ClassTask.TypeName; } }
        public string AssemblyLocation { get { return ClassTask.AssemblyLocation; } }
        public Exception Exception { get; private set; }

        public XunitTestClassTask ClassTask { get; private set; }

        public IEnumerable<XunitTestMethodTask> MethodTasks
        {
            get { return (from m in methodInfos select m.Task).ToList(); }
        }

        public void AddFixture<T>()
        {
            var type = typeof (IUseFixture<>);
            var genericType = type.MakeGenericType(typeof (T));
            fakeType.AddInterface(genericType);
        }

        public void SetConstructor(Action ctor)
        {
            constructor = ctor;
        }

        public void SetDispose(Action d)
        {
            dispose = d;
        }

        public object CreateInstance()
        {
            constructor();
            return dispose != null ? new Disposable(dispose) : new object();
        }

        private class Disposable : IDisposable
        {
            private readonly Action dispose;

            public Disposable(Action dispose)
            {
                this.dispose = dispose;
            }

            public void Dispose()
            {
                dispose();
            }
        }

        public Method AddMethod(string methodName, Action<object[]> methodBody, Parameter[] parameters, params Attribute[] attributes)
        {
            var method = new Method(this, ClassTask, methodName, methodBody, parameters ?? new Parameter[0], attributes);
            methodInfos.Add(method);
            return method;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
        {
            return Enumerable.Empty<IAttributeInfo>();
        }

        public IMethodInfo GetMethod(string methodName)
        {
            var methods = (from m in methodInfos
                           where m.Name == methodName
                           select m).ToList();

            if (methods.Count > 1)
            {
                Exception = new ArgumentException("Ambiguous method named " + methodName + " in type " + ClassTask.TypeName);
                throw Exception;
            }

            return methods.First();
        }

        public IEnumerable<IMethodInfo> GetMethods()
        {
            return methodInfos.Cast<IMethodInfo>();
        }

        public bool HasAttribute(Type attributeType)
        {
            return false;
        }

        public bool HasInterface(Type interfaceType)
        {
            return fakeType.GetInterfaces().Any(interfaceType.IsInstanceOfType);
        }

        public bool IsAbstract { get { return false; } }
        public bool IsSealed { get { return false; } }
        public Type Type { get { return fakeType; } }
    }
}