Imports System
Imports System.Collections.Generic
Imports Xunit
Imports Xunit.Extensions

Public Class MyTest
    <Theory>
    <PropertyData("{caret}", PropertyType := GetType(ProvidesPropertyData))>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub
End Class

Public Class ProvidesPropertyData
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
End Class
