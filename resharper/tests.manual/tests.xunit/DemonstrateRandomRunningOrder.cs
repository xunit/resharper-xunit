using System;
using System.Threading;
using Xunit;

namespace tests.xunit.eyeball
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

    public class DemonstrateTheoryTestRandomOrdering
    {
        private const int SleepTimeout = 100;

        // TEST: Executed out of order
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public void Test(int indexOfDataAttribute)
        {
            Thread.Sleep(SleepTimeout);
            Console.WriteLine("DemonstrateTheoryTestRandomOrdering(" + indexOfDataAttribute + ")");
        }
    }
}