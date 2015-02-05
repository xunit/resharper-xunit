using System;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // Repeat previous run causes issues for dynamic elements (i.e. a Theory
    // with random data). When running normally, a dynamic element is added
    // in run #1. In run #2, it is marked as pending. If that particular set
    // of data is run again in run #2, the dynamic element is marked as valid
    // and has a test result (pass or fail, doesn't matter). If that set of
    // data is NOT run again in run #2, the pending dynamic element is marked
    // as invalid (and in the UI is displayed with strike out). Run #3 will
    // remove any invalid elements.
    // When you click the repeat previous run button, resharper uses the same
    // list of elements from run #1 in run #2. All of the elements are marked
    // as pending, and some will run (and get marked valid), some won't. These
    // get marked as invalid and removed in run #3. So far so good.
    // If you click repeat previous run again, it reuses the list of elements
    // valid at the end of run #2. This does NOT include new elements added
    // during the run. These new elements are marked as outdated and displayed
    // with a question mark. This is where the issues start:
    // 1. If a new element (marked outdated) runs during run #3, it is not
    //    marked as not-outdated, and still maintains the question mark
    // 2. Any new element not run will remain as an outdated element, while
    //    existing elements that are not run are marked invalid, which is very
    //    confusing to the user
    // This class listens for tests to finish, and ensures that any newly
    // run dynamic element is no longer marked outdated, and that any outdated
    // dynamic elements are marked invalid
    //
    // Required for dotCover 2.0 RTM + 2.1 RTM
    [SolutionComponent]
    public class RepeatPreviousRunDynamicElementHandler : IDisposable
    {
        private readonly UnitTestSessionManager sessionManager;

        public RepeatPreviousRunDynamicElementHandler(UnitTestSessionManager sessionManager)
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
            e.Session.TestFinished += OnTestFinished;
        }

        void OnSessionClosed(object sender, SessionEventArgs e)
        {
            e.Session.TestFinished -= OnTestFinished;
        }

        void OnTestFinished(object sender, UnitTestEventArgs e)
        {
            var session = (UnitTestSession)sender;
            var theoryElement = e.Element as XunitTestTheoryElement;
            if (theoryElement != null)
            {
                var result = session.GetResult(theoryElement);
                if (result != null)
                    result.Outdated = false;
            }

            var methodElement = e.Element as XunitTestMethodElement;
            if (methodElement != null)
            {
                foreach (var theory in methodElement.Children)
                {
                    var result = session.GetResult(theory);
                    if (result != null && result.Outdated)
                        theory.State = UnitTestElementState.Invalid;
                }
            }
        }
    }
}