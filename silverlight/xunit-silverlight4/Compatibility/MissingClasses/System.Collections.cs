using System.Collections.Generic;

namespace Xunit
{
    // Used as part of the remoting interface by the runners. Needs to be public
    public interface IDictionary
    {
        object this[object key] { get; }
        bool Contains(object key);
    }

    // Keep this internal. It's used internal by xunit, and internally by xunit.runner.utility
    // We'll reimplement this as an internal class in xunit.runner.utility. I want to keep sharing
    // and exposing of these types to a minimum
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