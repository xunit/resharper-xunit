using System.Collections.Generic;

namespace Xunit
{
    // Used internally
    internal class Hashtable : IDictionary
    {
        private readonly Dictionary<object, object> dictionary = new Dictionary<object, object>();

        public object this[object key]
        {
            get { return dictionary[key]; }
            set { dictionary[key] = value; }
        }

        public bool Contains(object key)
        {
            return dictionary.ContainsKey(key);
        }
    }
}