Imports System
Imports System.Collections.Generic
Imports Xunit

Public Class MyTest
    <Theory>
    <MemberData("NonPublicDataEnumerator")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("NonStaticDataEnumerator")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("InvalidReturnTypeDataEnumerator")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("NonPublicMethod")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("NonStaticMethod")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("InvalidReturnTypeMethod")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("NonPublicField")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("NonStaticField")>
    Public Sub Test1(ByVal value As Int32)
        Assert.Equal(42, value)
    End Sub

    <Theory>
    <MemberData("InvalidReturnTypeField")>
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

    Public Shared ReadOnly Iterator Property InvalidReturnTypeDataEnumerator() As IEnumerable(Of Int32())
        Get
            Yield New Int32() { 42 }
        End Get
    End Property

    Private Shared Iterator Function NonPublicMethod() As IEnumerable(Of Object())
        Yield New Object() { 42 }
    End Function

    Public Iterator Function NonStaticMethod() As IEnumerable(Of Object())
        Yield New Object() { 42 }
    End Function

    Public Shared Iterator Function InvalidReturnTypeMethod() As IEnumerable(Of Int32())
        Yield New Int32() { 42 }
    End Function

    Private Shared NonPublicField As IEnumerable(Of Object())
    Public NonStaticField As IEnumerable(Of Object())
    Public Shared InvalidReturnTypeField As IEnumerable(Of Int32())
End Class
