using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace tests.xunit
{
    namespace SetsCurrentDirectoryCorrectly
    {
        public class SetsCurrentDirectoryCorrectly
        {
            [Fact]
            public void CurrentDirectoryIsNonShadowCopiedLocationOfTestAssembly()
            {
                Assert.Equal(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), Environment.CurrentDirectory);
            }
        }
    }
}