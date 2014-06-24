using Xunit;

namespace Foo
{
    public class ConcreteBaseClass
    {
        [Fact]
        public void BaseTestMethod()
        {
        }
    }

    public class DerivedFromConcreteBaseClass : ConcreteBaseClass
    {
        [Fact]
        public void DerivedTestMethod()
        {
        }
    }

    public abstract class AbstractBaseClass
    {
        [Fact]
        public void AbstractBaseTestMethod()
        {
        }
    }

    public class DerivedFromAbstractBaseClass : AbstractBaseClass
    {
        [Fact]
        public void DerivedTestMethod()
        {
        }
    }

    public class AlsoDerivedFromAbstractBaseClass : AbstractBaseClass
    {
        [Fact]
        public void AnotherDerivedTestMethod()
        {
        }
    }
}
