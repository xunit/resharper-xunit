using Xunit;

namespace tests.xunit.eyeball
{
    namespace Traits
    {
        // TEST: The gutter icon should not have any category menus
        public class Tests_with_category_traits
        {
            // TEST: The gutter icon should have 'Category "awesome"' submenu
            [Fact]
            [Trait("category", "awesome")]
            public void Should_have_category_of_awesome()
            {
            }

            // TEST: The gutter icon should have 'Category "rubbish"' submenu
            [Fact]
            [Trait("category", "rubbish")]
            public void Should_have_different_category_of_rubbish()
            {
            }

            // TEST: The gutter icon should have 'Category "casing"' submenu
            // TEST: Running the 'Category "casing"' tests should run 2 tests
            [Fact]
            [Trait("CATEGORY", "casing")]
            public void Should_ignore_uppercase_of_category()
            {
            }

            // TEST: The gutter icon should have 'Category "casing"' submenu
            // TEST: Running the 'Category "casing"' tests should run 2 tests
            [Fact]
            [Trait("CaTeGoRy", "casing")]
            public void Should_ignore_crazy_case_of_category()
            {
            }
        }

        // TEST: The gutter icon should not have any category menus
        public class Tests_with_arbitrary_traits
        {
            // TEST: The gutter icon should have 'Category "name[value]"' submenu
            // TEST: Running the 'Category "name[value]"' tests should run 2 tests
            [Fact]
            [Trait("name", "value")]
            public void Should_have_category_of_name_value()
            {
            }

            // TEST: The gutter icon should have 'Category "name[value]"' submenu
            // TEST: Running the 'Category "name[value]"' tests should run 2 tests
            [Fact]
            [Trait("name", "value")]
            public void Should_also_have_category_of_name_value()
            {
            }

            // TEST: The gutter icon should have 'Category "name[thing]"' submenu
            // TEST: Running the 'Category "name[thing]"' tests should run only 1 test
            // TEST: Ensure that the name traits are distinct by value, e.g. name[value] != name[thing]
            [Fact]
            [Trait("name", "thing")]
            public void Should_have_new_category_of_name_thing()
            {
            }

            // TEST: The gutter icon should have 'Category "Name[thing]"' submenu - note the casing
            // TEST: Running the 'Category "Name[thing]"' tests should run only 1 test
            [Fact]
            [Trait("Name", "thing")]
            public void Should_have_different_category_of_name_thing_with_different_casing()
            {
            }
        }

        // TEST: The gutter icon should not have any category menus
        public class Tests_with_multiple_traits
        {
            // TEST: The gutter icon should have 'Category "multiple"' and 'Category "owner[matt]"' submenus
            // TEST: Running the 'Category "multiple"' tests should run 2 tests
            // TEST: Running the 'Category "owner[matt]"' tests should run 2 tests
            [Fact]
            [Trait("category", "multiple")]
            [Trait("owner", "matt")]
            public void Should_have_category_of_multiple_and_owner_matt()
            {
            }

            // TEST: The gutter icon should have 'Category "multiple"' and 'Category "owner[matt]"' submenus
            // TEST: Running the 'Category "multiple"' tests should run 2 tests
            // TEST: Running the 'Category "owner[matt]"' tests should run 2 tests
            [Fact]
            [Trait("category", "multiple")]
            [Trait("owner", "matt")]
            public void Should_also_have_category_of_multiple_and_owner_matt()
            {
            }
        }

        // TEST: The gutter icon should have a 'Category "from_class_trait"' submenu - FAILS
        [Trait("category", "from_class_trait")]
        public class Class_with_traits
        {
            // TEST: The gutter icon should have a 'Category "from_class_trait"' submenu
            [Fact]
            public void Should_inherit_class_category()
            {
            }

            // TEST: The gutter icon should have 'Category "from_class_trait"' and 'Category "from_method_trait"' submenus
            [Fact]
            [Trait("category", "from_method_trait")]
            public void Should_merge_class_and_method_categories()
            {
            }
        }
    }
}
