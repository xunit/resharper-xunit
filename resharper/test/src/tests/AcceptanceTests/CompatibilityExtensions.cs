using System;
using JetBrains.DataFlow;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TestFramework.Components.UnitTestSupport;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using JetBrains.ReSharper.UnitTestSupportTests;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public static class CompatibilityExtensions
    {
        private static readonly Key<Action> FinishActionKey = new Key<Action>("Finish Action");

        public static void OnFinish(this UnitTestSessionTestImpl session, Action action)
        {
            session.PutData(FinishActionKey, action);
        }

        public static void Run(this UnitTestSessionTestImpl session, Lifetime lifetime, ITaskRunnerHostController runController, IUnitTestRunStrategy strategy)
        {
            var finishAction = session.GetData(FinishActionKey);
            session.Run(lifetime, runController, strategy, finishAction);
        }

        public static void OnFinish(this IRemoteChannel remoteChannel, Action action)
        {
            throw new NotImplementedException();
        }
    }
}