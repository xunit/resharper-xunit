using System;
using Xunit;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    // TODO: Remove this once TestMessageVisitor supports the cleanup messages
    // (see xunit/xunit#134)
    public class CleanupHandlingTestMessageVisitor<T> : TestMessageVisitor<T>
        where T : IMessageSinkMessage
    {
        private static bool DoVisit<TMessage>(IMessageSinkMessage message, Func<TMessage, bool> callback)
            where TMessage : class, IMessageSinkMessage
        {
            var castMessage = message as TMessage;
            return castMessage == null || callback(castMessage);
        }

        public override bool OnMessage(IMessageSinkMessage message)
        {
            return base.OnMessage(message)
                   && DoVisit<ITestAssemblyCleanupFailure>(message, Visit)
                   && DoVisit<ITestCollectionCleanupFailure>(message, Visit)
                   && DoVisit<ITestClassCleanupFailure>(message, Visit)
                   && DoVisit<ITestMethodCleanupFailure>(message, Visit)
                   && DoVisit<ITestCaseCleanupFailure>(message, Visit)
                   && DoVisit<ITestCleanupFailure>(message, Visit);
        }

        protected virtual bool Visit(ITestAssemblyCleanupFailure arg)
        {
            return true;
        }

        protected virtual bool Visit(ITestCollectionCleanupFailure arg)
        {
            return true;
        }

        protected virtual bool Visit(ITestClassCleanupFailure arg)
        {
            return true;
        }

        protected virtual bool Visit(ITestMethodCleanupFailure arg)
        {
            return true;
        }

        protected virtual bool Visit(ITestCaseCleanupFailure arg)
        {
            return true;
        }

        protected virtual bool Visit(ITestCleanupFailure arg)
        {
            return true;
        }
    }
}