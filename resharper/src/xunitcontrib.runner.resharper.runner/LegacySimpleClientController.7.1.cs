namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class LegacySimpleClientController
    {
        // Introduced in ReSharper 7.1, but I don't think it was ever used
        partial void ReportAdditionalInfoToClientController()
        {
            clientController.AdditionalControllerInfo(server.GetAdditionalControllerInfo());
        }
    }
}