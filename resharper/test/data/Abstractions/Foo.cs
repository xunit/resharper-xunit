using System.Runtime.InteropServices;

[assembly:Guid("C9F4F774-D383-42AA-BED9-A27E16C5FD53")]

namespace Foo
{
    public class PublicType
    {
        public class PublicNestedType
        {
        }

        private class PrivateNestedType
        {
        }
    }

    class PrivateType
    {
    }
}
