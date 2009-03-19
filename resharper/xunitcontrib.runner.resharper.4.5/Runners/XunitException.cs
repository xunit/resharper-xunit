using Xunit.Sdk;

// Not in a namespace, per issue #5841
// http://www.codeplex.com/xunit/WorkItem/View.aspx?WorkItemId=5841
class XunitException : AssertException
{
    public XunitException(string message,
                          string stackTrace)
        : base(message, stackTrace) { }
}