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
            var typeShortName = typeName.Split('.').Last();
            var typeNamespace = typeName.Replace("." + typeShortName, "");

            ClassResult = new ClassResult(typeShortName, typeName, typeNamespace);
            ClassTask = new XunitTestClassTask(assemblyLocation, typeName, true);
            methods = new List<Method>();
        }

        public XunitTestClassTask ClassTask { get; private set; }
        public ClassResult ClassResult { get; private set; }

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
            // TODO: We should let the logger keep a track of when it's starting and finishing a class
            // The only awkward bits are ExceptionThrown + ClassFailed
            // ClassFailed is sent via ClassResult + XmlLoggingNode when there's a failure
            // ExceptionThrown is called directly
            // SMELL!!!
            logger.ClassStart();

            if (Exception != null)
                logger.ExceptionThrown(ClassTask.AssemblyLocation, Exception);

            foreach (var node in LoggingSteps)
                XmlLoggerAdapter.LogNode(node, logger);

            // SMELL!!!!
            logger.ClassFinished();
        }

        private IEnumerable<XmlNode> LoggingSteps
        {
            get
            {
                return (from method in methods
                        from testResult in method.Results
                        from result in new ITestResult[]
                                           {
                                               new MethodStartResult(ClassTask.TypeName, method.Name),
                                               testResult
                                           }
                        select ToXml(result)).Concat(new[] {ToXml(ClassResult)});
            }
        }

        public Exception Exception { get; set; }

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
                                          exception.GetType().FullName, ExceptionUtility.GetMessage(exception), ExceptionUtility.GetStackTrace(exception))
                             {
                                 Output = output
                             };
            return AddMethod(methodName, result);
        }

        public Method AddTestWithInvalidParameters(string methodName, out Exception exception)
        {
            exception = new InvalidOperationException(string.Format("Fact method {0}.{1} cannot have parameters", ClassTask.TypeName, methodName));
            return AddFailingTest(methodName, exception);
        }

        public Method AddSkippedTest(string methodName, string skippedReason, string displayName = null)
        {
            var result = new SkipResult(methodName, ClassTask.TypeName, displayName ?? methodName, GetEmptyTraits(), skippedReason);
            return AddMethod(methodName, result);
        }

        public Method AddMethod(string methodName)
        {
            var method = new Method(ClassTask, methodName);
            methods.Add(method);
            return method;
        }

        private Method AddMethod(string methodName, MethodResult result)
        {
            ClassResult.Add(result);
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
            public Method(XunitTestClassTask classTask, string methodName)
            {
                Name = methodName;
                Results = new List<MethodResult>();
                Task = new XunitTestMethodTask(classTask.AssemblyLocation, classTask.TypeName, methodName, true);
            }

            public Method(XunitTestClassTask classTask, string methodName, MethodResult result)
                : this(classTask, methodName)
            {
                Results.Add(result);
            }

            public string Name { get; private set; }
            public IList<MethodResult> Results { get; private set; }
            public XunitTestMethodTask Task { get; private set; }

            public void AddPassingTheoryTest(string testName, string output = null)
            {
                var result = new PassedResult(Name, Task.TypeName, testName, GetEmptyTraits())
                                 {
                                     Output = output
                                 };
                Results.Add(result);
            }

            public void AddFailingTheoryTest(string testName, Exception exception, string output = null)
            {
                var result = new FailedResult(Name, Task.TypeName, testName, GetEmptyTraits(),
                                              exception.GetType().FullName, exception.Message, exception.StackTrace)
                                 {
                                     Output = output
                                 };
                Results.Add(result);
            }
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