namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    internal partial class ClientController
    {
        partial void ReportAdditionalControllerInfo()
        {
            clientController.AdditionalControllerInfo(Server.GetAdditionalControllerInfo());
        }
    }
}