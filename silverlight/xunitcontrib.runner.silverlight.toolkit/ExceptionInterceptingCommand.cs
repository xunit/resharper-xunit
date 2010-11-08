using System;
using System.Reflection;
using Xunit.Sdk;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    public class ExceptionInterceptingCommand : DelegatingTestCommand
    {
        private readonly IMethodInfo method;

        public ExceptionInterceptingCommand(ITestCommand wrappedCommand, IMethodInfo method) : base(wrappedCommand)
        {
            this.method = method;
        }

        public override MethodResult Execute(object testClass)
        {
            MethodResult result;

            try
            {
                result = InnerCommand.Execute(testClass);
            }
            catch (TargetInvocationException ex)
            {
                result = new ExceptionResult(method, ex.InnerException, DisplayName);
            }
            catch (Exception ex)
            {
                result = new ExceptionResult(method, ex, DisplayName);
            }

            return result;
        }
    }
}