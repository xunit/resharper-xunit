using System.Threading;

namespace Xunit
{
    internal enum ApartmentState
    {
        STA
    }

    internal static partial class Extensions
    {
        internal static void SetApartmentState(this Thread thread, ApartmentState apartmentState)
        {
            // Do nothing. Silverlight doesn't support ApartmentState 
        }

        internal static ApartmentState GetApartmentState(this Thread thread)
        {
            return ApartmentState.STA;
        }
    }
}