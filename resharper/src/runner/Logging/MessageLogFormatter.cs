using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Logging
{
    public static class MessageLogFormatter
    {
        public static string GetIdentifier(IMessageSinkMessage message)
        {
            return GetIdentifier(message as ITestMessage)
                   ?? GetIdentifier(message as ITestCaseMessage)
                   ?? GetIdentifier(message as ITestMethodMessage)
                   ?? GetIdentifier(message as ITestClassMessage)
                   ?? GetIdentifier(message as ITestCollectionMessage)
                   ?? GetIdentifier(message as ITestAssemblyMessage)
                   ?? "Unknown message";
        }

        private static string GetIdentifier(ITestMessage message)
        {
            return message == null ? null : string.Format("test: «{0}» - {1}", message.Test.DisplayName, message.TestCase.Format());
        }

        private static string GetIdentifier(ITestCaseMessage message)
        {
            return message == null ? null : message.TestCase.Format();
        }

        private static string GetIdentifier(ITestMethodMessage message)
        {
            return message == null ? null : string.Format("{0}.{1}", message.TestMethod.TestClass.Class.Name,
                message.TestMethod.Method.Name);
        }

        private static string GetIdentifier(ITestClassMessage message)
        {
            return message == null ? null : message.TestClass.Class.Name;
        }

        private static string GetIdentifier(ITestCollectionMessage message)
        {
            return message == null ? null : message.TestCollection.DisplayName;
        }

        private static string GetIdentifier(ITestAssemblyMessage message)
        {
            return message == null ? null : message.TestAssembly.Assembly.Name;
        }
    }
}