Imports System
Imports System.Collections.Generic
Imports Xunit

Public Class MyTest
    Inherits MemberDataBase

    <Theory>
    <MemberData("DataEnumerator")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub
End Class

Public Class MemberDataBase
    Public Shared ReadOnly Iterator Property DataEnumerator() As IEnumerable(Of Object())
        Get
            Yield New Object() { 42 }
        End Get
    End Property
End Class
