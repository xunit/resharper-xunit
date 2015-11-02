Imports System
Imports System.Collections.Generic
Imports Xunit

Public Class MyTest
    <Theory>
    <MemberData("DataEnumerator")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("ListReturnTypeDataEnumerator")>
    Public Sub Test2(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    Public Shared ReadOnly Iterator Property DataEnumerator() As IEnumerable(Of Object())
        Get
            Yield New Object() { 42 }
        End Get
    End Property

    Public Shared ReadOnly Property ListReturnTypeDataEnumerator() As IList(Of Object())
        Get
            Return New List(Of Object()) From { New Object() { 42 } }
        End Get
    End Property
End Class
