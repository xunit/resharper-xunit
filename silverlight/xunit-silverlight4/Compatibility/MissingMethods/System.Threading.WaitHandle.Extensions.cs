using System.Threading;

namespace Xunit
{
    // Just missing an overload. Can keep this internal
    internal static partial class Extensions
    {
        internal static bool WaitOne(this WaitHandle waitHandle, int timeout, bool exitSynchronisationContext)
        {
            // Just ignore the synchronisationContext flag. It's always false. 
            return waitHandle.WaitOne(timeout);
        }
    }
}