using Xunit;

namespace Foo
{
    public class Tests
    {
        [Fact(DisplayName = "NameComesFromAttribute")]
        public void ShouldNotSeeThis_NameShouldComeFromAttribute()
        {
        }

        [Fact(DisplayName = "Name contains spaces")]
        public void ShouldNotSeeThis_NameShouldContainSpaces()
        {
        }

        [Theory(DisplayName = "This is a theory with a display name")]
        [InlineData("cheese")]
        public void ShouldNotSeeThis_NameShouldComeFromTheoryAttribute(string data)
        {
        }
    }
}

