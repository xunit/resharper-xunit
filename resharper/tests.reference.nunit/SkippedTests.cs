using System;
using NUnit.Framework;

namespace tests.reference.nunit
{
    [TestFixture]
    public class SkippedClass
    {
        [Test, Ignore("This is the ignore reason")]
        public void SkippedTest()
        {
            throw new NotImplementedException();
        }
    }
}
