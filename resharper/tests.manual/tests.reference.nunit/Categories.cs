using NUnit.Framework;

namespace tests.reference.nunit
{
    namespace Categories
    {
        // TEST: The gutter icon should not have any category menus
        [TestFixture]
        public class Methods_with_categories
        {
            // TEST: The gutter icon should have 'Category "stuff"' submenu
            // TEST: Running the 'Category "stuff"' tests should run 2 tests
            [Test]
            [Category("stuff")]
            public void Should_have_category_of_stuff()
            {
            }

            // TEST: The gutter icon should have 'Category "stuff"' submenu
            // TEST: Running the 'Category "stuff"' tests should run 2 tests
            [Test]
            [Category("stuff")]
            public void Should_also_have_category_of_stuff()
            {
            }

            // TEST: The gutter icon should not have any category menus
            [Test]
            [Property("name", "value")]
            public void Should_not_have_category()
            {
            }

            // TEST: The gutter icon should have 'Category "category_one"' and 'Category "category_two"' submenus
            [Test]
            [Category("category_one")]
            [Category("category_two")]
            public void Should_have_two_categories()
            {
            }
        }


        // TEST: The gutter icon should have a 'Category "from_class"' submenu
        [TestFixture]
        [Category("from_class")]
        public class Class_with_categories
        {
            // TEST: The gutter icon should have a 'Category "from_class"' submenu only
            [Test]
            public void Should_inherit_class_category()
            {
            }

            // TEST: The gutter icon should have 'Category "from_class"' and 'Category "from_method"' submenus
            [Test]
            [Category("from_method")]
            public void Should_have_two_categories_from_class_and_from_method()
            {
            }
        }
    }
}