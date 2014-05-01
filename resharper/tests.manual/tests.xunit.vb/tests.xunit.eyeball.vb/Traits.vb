Imports Xunit

' TEST: The gutter icon should not have any category menus
Public Class Tests_with_catgeory_traits
    ' TEST: The gutter icon should have 'Category "awesome"' submenu
    <Fact>
    <Trait("category", "awesome")>
    Public Sub Should_have_category_of_awesome()
    End Sub

    ' TEST: The gutter icon should have 'Category "rubbish"' submenu
    <Fact>
    <Trait("category", "rubbish")>
    Public Sub Should_have_different_category_of_rubbish()
    End Sub

    ' TEST: The gutter icon should have 'Category "casing"' submenu
    ' TEST: Running the 'Category "casing"' tests should run 2 tests
    <Fact>
    <Trait("CATEGORY", "casing")>
    Public Sub Should_ignore_uppercase_of_category()
    End Sub

    ' TEST: The gutter icon should have 'Category "casing"' submenu
    ' TEST: Running the 'Category "casing"' tests should run 2 tests
    <Fact>
    <Trait("CaTeGoRy", "casing")>
    Public Sub Should_ignore_crazy_case_of_category()
    End Sub
End Class

' TEST: The gutter icon should not have any category menus
Public Class Tests_with_arbitrary_traits
    ' TEST: The gutter icon should have 'Category "name[value]"' submenu
    ' TEST: Running the 'Category "name[value]"' tests should run 2 tests
    <Fact>
    <Trait("name", "value")>
    Public Sub Should_have_category_of_name_value()
    End Sub

    ' TEST: The gutter icon should have 'Category "name[value]"' submenu
    ' TEST: Running the 'Category "name[value]"' tests should run 2 tests
    <Fact>
    <Trait("name", "value")>
    Public Sub Should_also_have_category_of_name_value()
    End Sub

    ' TEST: The gutter icon should have 'Category "name[thing]"' submenu
    ' TEST: Running the 'Category "name[thing]"' tests should run only 1 test
    ' TEST: Ensure that the name traits are distinct by value, e.g. name[value> != name[thing>
    <Fact>
    <Trait("name", "thing")>
    Public Sub Should_have_new_category_of_name_thing()
    End Sub

    ' TEST: The gutter icon should have 'Category "Name[thing]"' submenu - note the casing
    ' TEST: Running the 'Category "Name[thing]"' tests should run only 1 test
    <Fact>
    <Trait("Name", "thing")>
    Public Sub Should_have_different_category_of_name_thing_with_different_casing()
    End Sub
End Class

' TEST: The gutter icon should not have any category menus
Public Class Tests_with_multiple_traits
    ' TEST: The gutter icon should have 'Category "multiple"' and 'Category "owner[matt]"' submenus
    ' TEST: Running the 'Category "multiple"' tests should run 2 tests
    ' TEST: Running the 'Category "owner[matt]"' tests should run 2 tests
    <Fact>
    <Trait("category", "multiple")>
    <Trait("owner", "matt")>
    Public Sub Should_have_category_of_multiple_and_owner_matt()
    End Sub

    ' TEST: The gutter icon should have 'Category "multiple"' and 'Category "owner[matt]"' submenus
    ' TEST: Running the 'Category "multiple"' tests should run 2 tests
    ' TEST: Running the 'Category "owner[matt]"' tests should run 2 tests
    <Fact>
    <Trait("category", "multiple")>
    <Trait("owner", "matt")>
    Public Sub Should_also_have_category_of_multiple_and_owner_matt()
    End Sub
End Class

' TEST: The gutter icon should have a 'Category "from_class_trait"' submenu - FAILS
<Trait("category", "from_class_trait")>
Public Class Class_with_traits
    ' TEST: The gutter icon should have a 'Category "from_class_trait"' submenu
    <Fact>
    Public Sub Should_inherit_class_category()
    End Sub

    ' TEST: The gutter icon should have 'Category "from_class_trait"' and 'Category "from_method_trait"' submenus
    <Fact>
    <Trait("category", "from_method_trait")>
    Public Sub Should_merge_class_and_method_categories()
    End Sub
End Class

#If False Then
Namespace CustomAttributes

    Public Class Fact_with_normal_and_custom_traits

        <Fact>
        <Trait("Category", "from_trait_attribute")>
        <MyCategory("from_custom_category_attribute")>
        Public Sub Should_show_categories_from_trait_and_custom_trait_attribute()
        End Sub

        <Fact>
        <MyCategory("from_custom_category_attribute")>
        <MyDerivedCategory("from_derived_custom_category_attribute")>
        <MyDerivedAndDelegatingCategory("from_derived_and_delegating_custom_category_attribute")>
        Public Sub Should_show_categories_from_custom_trait_attributes()
        End Sub
    End Class

    Public Class MyCategoryAttribute
        Inherits TraitAttribute

        Public Sub New(ByVal value As String)
            MyBase.New("Category", value)
        End Sub
    End Class

    Public Class MyDerivedCategoryAttribute
        Inherits MyCategoryAttribute

        Public Sub New(ByVal value As String)
            MyBase.New(value)
        End Sub
    End Class

    Public Class MyDerivedAndDelegatingCategoryAttribute
        Inherits MyCategoryAttribute

        Public Sub New(ByVal value As String)
            Me.New(value, True)
        End Sub


        Public Sub New(ByVal value As String, ByVal whatever As Boolean)
            MyBase.New(value)
        End Sub

    End Class
End Namespace
#End If