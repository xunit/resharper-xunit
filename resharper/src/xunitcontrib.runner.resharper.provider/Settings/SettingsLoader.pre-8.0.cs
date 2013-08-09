using JetBrains.DataFlow;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Settings
{
    public partial class SettingsLoader
    {
        public static readonly PropertyId<bool> IsNonUserEditable = new PropertyId<bool>("BelongsToXunitcontrib");
    }
}