Imports Xunit

Public Class InvalidPropertyDataReference
    
    ' TEST: PropertyData attribute value should be marked invalid
    <Theory(Skip:="Invalid PropertyDataAttribute")>
    <MemberData("InvalidAccessibilityTheoryDataEnumerator")>
    Public Sub UsingInvalidDataProperty(ByVal value As Int32)
        Throw New InvalidOperationException("Should not run")
    End Sub
End Class