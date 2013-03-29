using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Extensions;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class Method : IMethodInfo
    {
        private readonly Class @class;
        private readonly Action<object[]> body;
        private readonly Parameter[] parameters;
        private readonly Attribute[] attributes;

        public Method(Class typeInfo, XunitTestClassTask classTask, string methodName, Action<object[]> methodBody, Parameter[] parameters, Attribute[] attributes)
        {
            @class = typeInfo;
            Name = methodName;
            Task = new XunitTestMethodTask(classTask.AssemblyLocation, classTask.TypeName, methodName, true, false);
            body = methodBody;
            this.parameters = parameters;
            this.attributes = attributes;

            TheoryTasks = GetTheoryTasks();
        }

        public XunitTestMethodTask Task { get; private set; }
        public IList<XunitTestTheoryTask> TheoryTasks { get; private set; } 

        public object CreateInstance()
        {
            return @class.CreateInstance();
        }

        public IEnumerable<Parameter> Parameters
        {
            get { return parameters; }
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
        {
            return from attribute in attributes
                   where attributeType.IsInstanceOfType(attribute)
                   select Reflector.Wrap(attribute);
        }

        public bool HasAttribute(Type attributeType)
        {
            return attributes.Any(attributeType.IsInstanceOfType);
        }

        public void Invoke(object testClass, params object[] parameterValues)
        {
            if (parameters.Length != (parameterValues != null ? parameterValues.Length : 0))
                throw new ParameterCountMismatchException();
            body(parameterValues);
        }

        public ITypeInfo Class { get { return @class; } }
        public bool IsAbstract { get { return false; } }
        public bool IsStatic { get { return false; } }
        public MethodInfo MethodInfo { get { return new FakeMethodInfo(this); } }
        public string Name { get; private set; }
        public string ReturnType { get { return "System.Void"; } }
        public string TypeName { get { return Task.TypeName; }}

        private IList<XunitTestTheoryTask> GetTheoryTasks()
        {
            var tasks = new List<XunitTestTheoryTask>();

            var inlineDataAttributes = GetCustomAttributes(typeof (InlineDataAttribute));
            foreach (var attribute in inlineDataAttributes)
            {
                var inlineDataAttribute = attribute.GetInstance<InlineDataAttribute>();
                if (inlineDataAttribute != null)
                {
                    var data = inlineDataAttribute.GetData(null, null).First();

                    var i = 0;
                    var formattedParameters = string.Join(", ", (from p in parameters
                                                                 select string.Format("{0}: {1}", p.Name, data[i++])).ToArray());
                    var theoryName = string.Format("{0}({1})", Name, formattedParameters);
                    tasks.Add(new XunitTestTheoryTask(Task, theoryName));
                }
            }

            return tasks;
        }
    }
}