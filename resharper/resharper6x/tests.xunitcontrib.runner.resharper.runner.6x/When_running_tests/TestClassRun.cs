using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class TestClassRun
    {
        private readonly IList<Method> methods;

        public TestClassRun(string typeName, string assemblyLocation = "assembly1.dll")
        {
            ClassTask = new XunitTestClassTask(assemblyLocation, typeName, true);
            methods = new List<Method>();
        }

        public XunitTestClassTask ClassTask { get; private set; }

        public IList<XunitTestMethodTask> MethodTasks
        {
            get
            {
                return (from m in methods
                        select m.Task).ToList();
            }
        }

        // Perhaps this should be an extension method?
        // It would be nice to move ClassStart/ClassFinished into the logger
        public void Run(ReSharperRunnerLogger logger)
        {
            // SMELL!!!
            logger.ClassStart();

            foreach (var node in LoggingSteps)
                XmlLoggerAdapter.LogNode(node, logger);

            // SMELL!!!!
            logger.ClassFinished();
        }

        private IEnumerable<XmlNode> LoggingSteps
        {
            get
            {
                return from method in methods
                       from result in new ITestResult[]
                                          {
                                              new MethodStartResult(ClassTask.TypeName, method.Name),
                                              method.Result
                                          }
                       select ToXml(result);
            }
        }

        private static XmlNode ToXml(ITestResult testResult)
        {
            var doc = new XmlDocument();
            doc.LoadXml("<foo />");
            return testResult.ToXml(doc.ChildNodes[0]);
        }

        public Method AddPassingTest(string methodName, string output = null, string displayName = null)
        {
            var result = new PassedResult(methodName, ClassTask.TypeName, displayName ?? methodName, GetEmptyTraits())
                             {
                                 Output = output
                             };
            return AddMethod(methodName, result);
        }

        public Method AddFailingTest(string methodName, Exception exception, string output = null, string displayName = null)
        {
            var result = new FailedResult(methodName, ClassTask.TypeName, displayName ?? methodName, GetEmptyTraits(),
                                          exception.GetType().FullName, exception.Message, exception.StackTrace)
                             {
                                 Output = output
                             };
            return AddMethod(methodName, result);
        }

        private Method AddMethod(string methodName, MethodResult result)
        {
            var method = new Method(ClassTask, methodName, result);
            methods.Add(method);
            return method;
        }

        private static Xunit.Sdk.MultiValueDictionary<string, string> GetEmptyTraits()
        {
            return new Xunit.Sdk.MultiValueDictionary<string, string>();
        }

        public class Method
        {
            public Method(XunitTestClassTask classTask, string methodName, MethodResult result)
            {
                Name = methodName;
                Result = result;
                Task = new XunitTestMethodTask(classTask.AssemblyLocation, classTask.TypeName, methodName, true);
            }

            public string Name { get; private set; }
            public MethodResult Result { get; private set; }
            public XunitTestMethodTask Task { get; private set; }
        }

        private class MethodStartResult : ITestResult
        {
            private readonly DummyTestMethodCommand testCommand;

            public MethodStartResult(string typeName, string methodName)
            {
                ExecutionTime = 0;
                testCommand = new DummyTestMethodCommand(typeName, methodName);
            }

            public XmlNode ToXml(XmlNode parentNode)
            {
                return testCommand.ToStartXml();
            }

            public double ExecutionTime { get; private set; }

            private class DummyTestMethodCommand : TestCommand
            {
                public DummyTestMethodCommand(string typeName, string methodName)
                    : base(GetDummyMethodInfo(), null, 0)
                {
                    TypeName = typeName;
                    MethodName = methodName;
                }

                private static IMethodInfo GetDummyMethodInfo()
                {
                    return Reflector.Wrap(typeof(DummyTestMethodCommand).GetMethod("Execute"));
                }

                public override MethodResult Execute(object testClass)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}