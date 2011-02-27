using System.Threading;
using Xunit;

namespace tests.xunit
{
    public class DemonstrateRandomRunningOrder
    {
        private const int SleepTimeout = 100;

        [Fact]
        public void Test01()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test02()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test03()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test04()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test05()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test06()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test07()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test08()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test09()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test10()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test11()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test12()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test13()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test14()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test15()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test16()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test17()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test18()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test19()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }

        [Fact]
        public void Test20()
        {
            Thread.Sleep(SleepTimeout);
            Assert.Equal(2, 2);
        }
    }
}