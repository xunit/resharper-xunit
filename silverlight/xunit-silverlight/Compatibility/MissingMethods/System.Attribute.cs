namespace Xunit
{
    // A couple of the attributes override TypeId, which Silverlight doesn't have. I don't really know why.
    // Insert this class as a shim to provide the virtual TypeId method. This method needs to be public, as
    // it's a base class to other public classes.
    // This could cause problems in other people's code, by failing to assign attributes returned from Reflection
    // to Attribute instance variables. They will need to adequately scope their Attributes. If this turns out to
    // be a big issue, we can #ifdef out the overrides in the xunit source
    public class Attribute : System.Attribute
    {
        public virtual object TypeId { get { return GetType(); } }
    }
}