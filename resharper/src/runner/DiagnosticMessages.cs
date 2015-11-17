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
        private readonly IList<string> messages; 

        public DiagnosticMessages(bool enabled)
        {
            messages = new List<string>();
            visitor = new DiagnosticsVisitor(enabled, messages);
        }

        public IMessageSink Visitor { get { return visitor; } }
        public bool HasMessages { get { return messages.Count > 0; } }

        public string Messages
        {
            get { return string.Join(Environment.NewLine, messages.ToArray()); }
        }

        public void Report(IRemoteTaskServer server)
        {
            if (HasMessages)
                server.ShowNotification("xUnit.net ReSharper runner reported diagnostic messages:", Messages);
        }

        private class DiagnosticsVisitor : TestMessageVisitor
        {
            private readonly bool enabled;
            private readonly IList<string> messages;

            public DiagnosticsVisitor(bool enabled, IList<string> messages)
            {
                this.enabled = enabled;
                this.messages = messages;
            }

            protected override bool Visit(IDiagnosticMessage diagnosticMessage)
            {
                if (enabled && !string.IsNullOrEmpty(diagnosticMessage.Message))
                {
                    Logger.LogVerbose("xunit diagnostic: {0}", diagnosticMessage.Message);
                    messages.Add(diagnosticMessage.Message);
                }
                return base.Visit(diagnosticMessage);
            }
        }

        public void Add(string message)
        {
            messages.Add(message);
        }
    }
}