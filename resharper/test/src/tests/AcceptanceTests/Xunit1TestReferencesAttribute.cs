using JetBrains.ReSharper.TestFramework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public class Xunit1TestReferencesAttribute : TestReferencesAttribute
    {
        public Xunit1TestReferencesAttribute()
            : base(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.dll",
                   EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.extensions.dll")
        {
        }
    }
}