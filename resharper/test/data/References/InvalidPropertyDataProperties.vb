Imports System
Imports System.Collections.Generic
Imports Xunit
Imports Xunit.Extensions

Public Class MyTest
    <Theory>
    <PropertyData("NonPublicDataEnumerator")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <PropertyData("NonStaticDataEnumerator")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <PropertyData("InvalidReturnTypeDataEnumerator")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    Private Shared ReadOnly Iterator Property NonPublicDataEnumerator() As IEnumerable(Of Object())
        Get
            Yield New Object() { 42 }
        End Get
    End Property

    Public ReadOnly Iterator Property NonStaticDataEnumerator() As IEnumerable(Of Object())
        Get
            Yield New Object() { 42 }
        End Get
    End Property

    Public Shared ReadOnly Iterator Property NonStaticDataEnumerator() As IEnumerable(Of Int32())
        Get
            Yield New Int32() { 42 }
        End Get
    End Property
End Class
