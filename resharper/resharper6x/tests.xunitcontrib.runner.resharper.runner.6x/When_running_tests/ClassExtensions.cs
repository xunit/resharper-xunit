using System;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class ClassExtensions
    {
        public static Method AddPassingTest(this Class self, string methodName, string output = null, string displayName = null)
        {
            return self.AddMethod(methodName, _ =>
                                                  {
                                                      if (!string.IsNullOrEmpty(output))
                                                          Console.Write(output);
                                                  }, null, new FactAttribute { DisplayName = displayName });
        }

        public static Method AddFailingTest(this Class self, string methodName, Exception exception, string output = null, string displayName = null)
        {
            return self.AddMethod(methodName, _ =>
                                                  {
                                                      if (!string.IsNullOrEmpty(output))
                                                          Console.Write(output);
                                                      throw exception;
                                                  }, null, new FactAttribute { DisplayName = displayName });
        }

        public static Method AddTestWithInvalidParameters(this Class self, string methodName)
        {
            return self.AddMethod(methodName, _ => { }, new[] { typeof(string) }, new FactAttribute());
        }

        public static Method AddSkippedTest(this Class self, string methodName, string skippedReason, string displayName = null)
        {
            return self.AddMethod(methodName, _ => { }, null, new FactAttribute { Skip = skippedReason, DisplayName = displayName });
        }
    }
}