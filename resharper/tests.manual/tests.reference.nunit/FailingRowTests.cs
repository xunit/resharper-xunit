using System;
using NUnit.Framework;

namespace tests.reference.nunit.failing
{
    [TestFixture]
    public class FailingRowTests
    {
        // TEST: Each TestCase needs to display an exception
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void EachRowThrowsAnException(int value)
        {
            throw new Exception(string.Format("Exception no: {0}", value));
        }

        // TEST: Only one row should throw an exception
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void JustOneRowThrowsAnException(int value)
        {
            if (value == 3)
                throw new Exception(string.Format("Exception no: {0}", value));
        }
    }
}