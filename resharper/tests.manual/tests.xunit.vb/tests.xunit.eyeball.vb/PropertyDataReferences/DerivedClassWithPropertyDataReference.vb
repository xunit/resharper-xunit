Imports Xunit

Public Class DerivedClassWithPropertyDataReference
    Inherits PropertyDataBase

    ' TEST: Should reference TheoryDataEnumerator in base class
    <Theory>
    <MemberData("TheoryDataEnumerator")>
    Public Sub Test(ByVal value As Int32)
    End Sub
End Class

Public Class PropertyDataBase
    
    ' TEST: This should be marked as in use
    ' TEST: Find usages should navigate to the PropertyData attribute above
    ' TODO: Stop ReSharper marking this as can be turned into protected
    ' (see XamlUsageAnalyzer.ProcessNameDeclaration)
    Public Shared ReadOnly Property TheoryDataEnumerator() As IEnumerable(Of Object())
        Get
            Return Enumerable.Range(1, 10).Select(Function(x) New Object() {x})
        End Get
    End Property
End Class