using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.Util;
using Xunit;
using Xunit.Abstractions;
using XunitContrib.Runner.ReSharper.RemoteRunner.Logging;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class DiagnosticMessages
    {
        private readonly DiagnosticsVisitor visitor;

        public DiagnosticMessages(TestAssemblyConfiguration xunitConfiguration)
        {
            visitor = new DiagnosticsVisitor(xunitConfiguration);
        }

        public IMessageSink Visitor { get { return visitor; } }
        public bool HasMessages { get { return visitor.Messages.Count > 0; } }

        public string Messages
        {
            get { return string.Join(Environment.NewLine, visitor.Messages.ToArray()); }
        }

        public void Report(IRemoteTaskServer server)
        {
            if (HasMessages)
                server.ShowNotification("xUnit.net ReSharper runner reported diagnostic messages:", Messages);
        }

        private class DiagnosticsVisitor : TestMessageVisitor
        {
            private readonly bool enabled;

            public DiagnosticsVisitor(TestAssemblyConfiguration xunitConfiguration)
            {
                Messages = new List<string>();
                enabled = xunitConfiguration.DiagnosticMessagesOrDefault || TaskExecutor.Configuration.IsInInternalDebug;
            }

            protected override bool Visit(IDiagnosticMessage diagnosticMessage)
            {
                if (enabled && !string.IsNullOrEmpty(diagnosticMessage.Message))
                {
                    Logger.LogVerbose("xunit diagnostic: {0}", diagnosticMessage.Message);
                    Messages.Add(diagnosticMessage.Message);
                }
                return base.Visit(diagnosticMessage);
            }

            public IList<string> Messages { get; private set; }
        }
    }
}