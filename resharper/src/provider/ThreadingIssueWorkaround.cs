using System;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // There is a threading issue in UnitTestSession which manifests in theory results
    // not getting added to the UI, even though they are run:
    // * Adding a dynamic element calls UnitTestSession.AddElement, potentially async
    // * AddElement adds the element to the list of elements and if the session isn't
    //   running, calls UnitTestSession.OnSessionContentsChanged, which causes the UI
    //   to update. If the session is running, it sets an internal flag
    // * UTS.TestStarting checks the internal flag and calls OnSessionContentsChanged
    //   if it needs to, and then resets the flag
    // * NOBODY ELSE CHECKS THE FLAG
    // And there is the race condition. If the call to AddElement is followed quickly
    // enough by the last call to TestStarting in a particular test run, the flag is
    // set after it's checked and the UI is never updated.
    // The workaround is to call TestStarting with a null Element when the run is
    // finished. This assumes knowledge of the implementation, but sorts everything
    // out. The flag is checked and the UI is updated, and the null Element causes
    // the method to exit early
    //
    // Required for ReSharper 6.1 RTM dotCover 2.0 RTM
    // Not needed for ReSharper 7.x RTM and later, or dotCover 2.1 RTM or later
    [SolutionComponent]
    public class ThreadingIssueWorkaround : IDisposable
    {
        private readonly UnitTestSessionManager sessionManager;

        public ThreadingIssueWorkaround(UnitTestSessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
            sessionManager.SessionCreated += OnSessionCreated;
            sessionManager.SessionClosed += OnSessionClosed;
        }

        public void Dispose()
        {
            sessionManager.SessionCreated -= OnSessionCreated;
            sessionManager.SessionClosed -= OnSessionClosed;
        }

        void OnSessionCreated(object sender, SessionEventArgs e)
        {
            e.Session.SessionStateChanged += OnSessionStateChanged;
        }

        void OnSessionClosed(object sender, SessionEventArgs e)
        {
            e.Session.SessionStateChanged -= OnSessionStateChanged;
        }

        void OnSessionStateChanged(object sender, SessionStateUpdateEventArgs e)
        {
            var session = sender as UnitTestSession;
            if (session != null && (e.State == UnitTestSessionState.Stopping || e.State == UnitTestSessionState.Aborting))
                session.TestStarting(null);
        }
    }
}