using System.Threading.Tasks;
using Xunit;

namespace Foo
{
    public class Tests
    {
        async static Task<int> Fibonacci(int n)
        {
            if (n == 0) { return 0; }
            if (n == 1) { return 1; }

            // run one of the recursions concurrently. This runs on a background thread
            var t1 = Task.Factory.StartNew(() => Fibonacci(n - 1));
            var t2 = Fibonacci(n - 2);
            return (await (await t1)) + (await t2);
        }

        [Fact]
        public async Task SimpleAsyncTest()
        {
            var result = await Fibonacci(10);
            Assert.Equal(56, result);
        }

        [Fact]
        public async void VoidTestNotSureIfThisIsLegal()
        {
            var result = await Fibonacci(10);
            Assert.Equal(56, result);
        }
    }
}
