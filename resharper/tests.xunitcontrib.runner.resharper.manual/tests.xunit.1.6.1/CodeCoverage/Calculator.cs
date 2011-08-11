namespace tests.xunit.CodeCoverage
{
    // Testing integration with dotCover
    // TEST: All methods (inc ctor) should be covered, except for Divide
    public class Calculator
    {
        private readonly int value;

        public Calculator(int value)
        {
            this.value = value;
        }

        public int Add(int x)
        {
            return value + x;
        }

        public int Subtract(int x)
        {
            return value - x;
        }

        public int Multiply(int multiplier)
        {
            return value*multiplier;
        }

        // TEST: This should not be covered
        public int Divide(int divisor)
        {
            return value/divisor;
        }
    }
}