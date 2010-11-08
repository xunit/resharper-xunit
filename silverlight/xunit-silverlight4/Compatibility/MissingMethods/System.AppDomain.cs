namespace Xunit
{
    // Silverlight's AppDomain doesn't have a SetupInformation property. Disappointingly, we can't
    // monkey patch it on with an extension method, so we have to insert a pretend class
    // xunit uses it to report the configuration file to use for the new AppDomain. I haven't got as
    // far as running tests in a new AppDomain (the Silverlight unit testing framework does everything
    // for us right now) so this does nothing useful.
    internal class AppDomain
    {
        // And of course we have to supply a CurrentDomain
        public static AppDomain CurrentDomain
        {
            get { return new AppDomain(); }
        }

        public SetupInformation SetupInformation
        {
            get { return new SetupInformation(); }
        }
    }

    internal class SetupInformation
    {
        public string ConfigurationFile
        {
            // TODO: Return something meaningful here
            get { return string.Empty; }
        }
    }
}