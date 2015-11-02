Imports System
Imports System.Collections.Generic
Imports Xunit

Public Class MyTest
    <Theory>
    <MemberData("{caret}")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    Public Shared ReadOnly Iterator Property DataEnumerator() As IEnumerable(Of Object())
        Get
            Yield New Object() { 42 }
        End Get
    End Property

    Public Shared ReadOnly Iterator Property AnotherDataEnumerator() As IEnumerable(Of Object())
        Get
            Yield New Object() { 42 }
        End Get
    End Property

    Public Shared ReadOnly Iterator Property MoreDataEnumerator() As IEnumerable(Of Object())
        Get
            Yield New Object() { 42 }
        End Get
    End Property

    Public Shared Iterator Function GetDataMethod() As IEnumerable(Of Object())
        Yield New Object() { 42 }
    End Function

    Public Shared GetDataField As IEnumerable(Of Object())
End Class
