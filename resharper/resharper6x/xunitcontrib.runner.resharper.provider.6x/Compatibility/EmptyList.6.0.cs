using System.Collections.Generic;

namespace JetBrains.Util
{
    public static class EmptyList<T>
    {
        static EmptyList()
        {
            InstanceList = new ReadOnlyCollection<T>(new List<T>());
        }

        public static ICollection<T> InstanceList { get; private set; }
    }
}