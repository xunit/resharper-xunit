Imports Xunit

Public Class PropertyDataReferences
    ' TEST: TheoryDataEnumerator should be highlighted correctly
    ' TEST: Intellisense should work in PropertyData, including TheoryDataEnumerator and DerivedReturnTypeTheoryDataEnumerator
    <Theory>
    <MemberData("TheoryDataEnumerator")>
    Public Sub DataFromProperty(ByVal value As Int32)
        Console.WriteLine("DataFromProperty({0})", value)
    End Sub

    <Theory>
    <MemberData("TheoryDataEnumerator")>
    Public Sub DataFromProperty2(ByVal value As Int32)
        Console.WriteLine("DataFromProperty({0})", value)
    End Sub

    ' TEST: This should be marked as in use - and it must be public static
    ' TEST: Find usages should navigate to the PropertyData attributes above
    Public Shared ReadOnly Property TheoryDataEnumerator() As IEnumerable(Of Object())
        Get
            Return Enumerable.Range(1, 10).Select(Function(x) New Object() {x})
        End Get
    End Property

    ' TEST: Should be marked as not in use
    ' TEST: Should appear in intellisense - note the different return value
    Public Shared ReadOnly Property DerivedReturnTypeTheoryDataEnumerator As IList(Of Object())
        Get
            Return Enumerable.Range(1, 10).Select(Function(x) New Object() {x}).ToList()
        End Get
    End Property

    ' TEST: Should be marked as not in use
    ' TEST: Should not be included in intellisense for PropertyData
    Private Shared ReadOnly Property InvalidAccessibilityTheoryDataEnumerator As IEnumerable(Of object())
        Get
            Return Enumerable.Range(1, 10).Select(Function(x) New Object() {x}).ToList()
        End Get
    End Property

    ' TEST: Should be marked as not in use
    ' TEST: Should not be included in intellisense for PropertyData
    Public ReadOnly Property InvalidNonStaticTheoryDataEnumerator As IEnumerable(Of Object())
        Get
            Return Enumerable.Range(1, 10).Select(Function(x) New Object() {x})
        End Get
    End Property

End Class