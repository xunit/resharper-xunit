using System.IO;

namespace Xunit
{
    namespace Sdk
    {
        // Silverlight doesn't allow you to set this. Or at least, it marks it as a
        // [SecurityCritical] which we can't call from [SecurityTransparent]
        // Is there something better we can do here?
        internal class Console
        {
            public static void SetOut(TextWriter writer)
            {
            }

            public static void SetError(TextWriter writer)
            {   
            }

            public static TextWriter Out
            {
                get { return null; }
                set { }
            }

            public static TextWriter Error
            {
                get { return null; }
                set { }
            }
        }
    }
}