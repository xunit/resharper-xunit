Imports Xunit.Extensions
Imports Xunit

Public Class SimpleTest
    <Fact>
    Public Sub TestMethod()
        Console.WriteLine("Test")
    End Sub

    <Theory>
    <InlineData(12)>
    Public Sub TheoryMethod(ByVal value As Int32)
        Assert.Equal(12, value)
    End Sub
End Class
