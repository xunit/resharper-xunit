using Xunit.Sdk;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    internal interface IProvideTestClassCommand
    {
        ITestClassCommand TestClassCommand { get; }
    }
}