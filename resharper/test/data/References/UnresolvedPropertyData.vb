Imports System
Imports System.Collections.Generic
Imports Xunit
Imports Xunit.Extensions

Public Class MyTest
    <Theory>
    <PropertyData("NotDataEnumerator")>
    Public Sub DataComesFromProperty(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    Public Shared ReadOnly Iterator Property DataEnumerator() As IEnumerable(Of Object())
        Get
            Yield New Object() { 42 }
        End Get
    End Property
End Class
