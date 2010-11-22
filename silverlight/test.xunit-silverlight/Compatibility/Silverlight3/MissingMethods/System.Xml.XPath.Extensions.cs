using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace System.Xml.XPath
{
    internal static class Extensions
    {
        // VERY simple xpath helper method. Only useful for VERY simple expressions. Like what we need.
        // Thanks to Chris Cavanagh: http://chriscavanagh.wordpress.com/2009/04/11/micro-xpath-almost-in-silverlight-2/
        public static XElement XPathSelectElement(this XElement element, string expression)
        {
            return expression.Split('/').Aggregate(element, (e, name) => e.Element(name));
        }

        public static IEnumerable<XElement> XPathSelectElements(this XElement element, string expression)
        {
            // Split expression in parts
            // Get candidate elements for first part
            // Interrogate each candidate for 2nd part => new candidates
            // Interrogate each candidate for 3rd part

            var candidates = new List<XElement> {element};
            var parts = expression.Split('/');
            foreach (var part in parts)
            {
                var newCandidates = new List<XElement>();
                foreach (var candidate in candidates)
                {
                    newCandidates.AddRange(candidate.Elements(part));
                }
                candidates = newCandidates;
            }

            return candidates;
        }
    }
}