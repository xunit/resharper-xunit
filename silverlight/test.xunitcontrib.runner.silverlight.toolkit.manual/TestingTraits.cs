using Xunit;

namespace test.xunitcontrib.runner.silverlight.toolkit.manual
{
    public class TestingTraits
    {
        [Fact, Trait("owner", "matt")]
        public void ShouldHaveOwnerOfMatt()
        {
        }

        [Fact, Trait("OWNER", "matt")]
        public void OwnerTraitShouldBeCaseInsensitive()
        {
        }

        [Fact, Trait("Description", "This is the description")]
        public void ShouldHaveDescription()
        {
        }

        [Fact, Trait("DESCRIPTION", "This is the description")]
        public void DescriptionTraitShouldBeCaseInsensitive()
        {
        }

        [Fact, Trait("category", "easy")]
        public void ShouldHaveCategoryOfEasy()
        {
        }

        [Fact, Trait("CATEGORY", "easy")]
        public void CategoryTraitShouldBeCaseInsensitive()
        {
        }

        [Fact, Trait("category", "easy"), Trait("category", "another")]
        public void ShouldHaveOnlyOneCategory()
        {
        }
    }
}