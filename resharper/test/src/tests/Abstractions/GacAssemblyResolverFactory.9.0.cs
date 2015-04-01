using JetBrains.Metadata.Reader.API;

namespace XunitContrib.Runner.ReSharper.Tests.Abstractions
{
    public class GacAssemblyResolverFactory
    {
        public static GacAssemblyResolver CreateOnCurrentRuntimeGac()
        {
            return GacAssemblyResolver.CreateOnCurrentRuntimeGac(GacAssemblyResolver.GacResolvePreferences.MatchSameOrNewer);
        }
    }
}