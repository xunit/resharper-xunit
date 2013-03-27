namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    partial class RemoteTaskServer
    {
        // Added in 7.1, but I don't think anyone (i.e. dotCover) uses it
        partial void ReportAdditionInfoToClientController()
        {
            clientController.AdditionalControllerInfo(server.GetAdditionalControllerInfo());
        }
    }
}
