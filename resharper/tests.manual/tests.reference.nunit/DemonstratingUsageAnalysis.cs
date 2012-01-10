using NUnit.Framework;

namespace tests.reference.nunit
{
    // TEST: This namespace should be marked as in use
    namespace NamespaceContainingSingleClassMarkedAsUsed
    {
        // TEST: This class should be marked as in use
        [TestFixture]
        public class SingleTestClass
        {
            // TEST: This method should be marked as in use
            [Test]
            public void TestMethodMarkedAsInUse()
            {
                Assert.AreEqual(1, 1);
            }
        }
    }

    // For completeness...
    namespace EmptyNamespaceMarkedAsNotInUse
    {
    }

    public class ParentClassNotMarkedAsInUse
    {
        [TestFixture]
        public class NestedClassMarkedAsInUse
        {
            [Test]
            public void TestMethodMarkedAsInUse()
            {
                Assert.AreEqual(1, 1);
            }
        }
    }

    // TEST: This class not marked as in use
    // TEST: This class should *NOT* be flagged as a test class
    public class NotInUse
    {
        // TEST: This method should be marked as in use
        // TEST: This method should *NOT* be flagged as a test
        [Test]
        public void TestMethodMarkedInUse()
        {
        }
    }

    // TEST: This class should be marked as in use
    [TestFixture]
    public class BaseClassFixtureMarkedAsInUse
    {
    }

    // TEST: This class should be marked as in use
    public class DerivedClassMarkedAsInUse : BaseClassFixtureMarkedAsInUse
    {
    }
}
