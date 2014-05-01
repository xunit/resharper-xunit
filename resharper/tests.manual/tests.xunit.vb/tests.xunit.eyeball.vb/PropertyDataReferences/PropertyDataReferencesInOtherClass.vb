Imports Xunit

Public Class PropertyDataReferencesInOtherClass
    <Theory>
    <MemberData("TheoryDataEnumerator", MemberType:=GetType(ProvidesPropertyData))>
    Public Sub Test(ByVal value As Int32)
    End Sub
End Class

Public Class ProvidesPropertyData
    
    ' TEST: This should be marked as in use
    Public Shared ReadOnly Property TheoryDataEnumerator() As IEnumerable(Of Object())
        Get
            Return Enumerable.Range(1, 10).Select(Function(x) New Object() {x})
        End Get
    End Property

    
    ' TEST: This should be marked as not in use
    Public Shared ReadOnly Property UnusedTheoryDataEnumerator() As IEnumerable(Of Object())
        Get
            Return Enumerable.Range(1, 10).Select(Function(x) New Object() {x})
        End Get
    End Property
End Class