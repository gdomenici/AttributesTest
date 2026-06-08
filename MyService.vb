Public Class MyService
    Implements IMyService

    Public Function Hello(name As String) As String Implements IMyService.Hello
        Return $"Hello, {name}!"
    End Function
End Class