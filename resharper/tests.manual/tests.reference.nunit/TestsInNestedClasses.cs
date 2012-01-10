using NUnit.Framework;

namespace tests.reference.nunit
{
    namespace TestsInNestedClasses
    {
        // Nunit doesn't mark OuterClass as being in use. Inner class is available as a test class
        // Test session doesn't mention OuterClass
        public class ParentClass
        {
            [TestFixture]
            public class NestedClass
            {
                [Test]
                public void NestedTest()
                {
                    Assert.AreEqual(1, 1);
                }
            }
        }
    }
}
