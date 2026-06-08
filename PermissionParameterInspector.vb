Imports System.Security
Imports CoreWCF
Imports CoreWCF.Dispatcher

Public Class PermissionParameterInspector
    Implements IParameterInspector

    Private ReadOnly _requiredRole As String

    Public Sub New(requiredRole As String)
        _requiredRole = requiredRole
    End Sub

    Public Function BeforeCall(operationName As String, inputs() As Object) As Object _
        Implements IParameterInspector.BeforeCall

        ' This is where enforcement happens.
        ' In real life: check Thread.CurrentPrincipal or ServiceSecurityContext
        Dim currentUser = ServiceSecurityContext.Current?.PrimaryIdentity?.Name

        ' Hardcoded rule for illustration:
        If _requiredRole <> "Admin" Then
            Throw New SecurityException(
                $"Operation '{operationName}' requires role '{_requiredRole}', which is not permitted.")
        End If

        Return Nothing  ' correlationState passed to AfterCall — not needed here
    End Function

    Public Sub AfterCall(operationName As String, outputs() As Object,
                         returnValue As Object, correlationState As Object) _
        Implements IParameterInspector.AfterCall
        ' nothing needed
    End Sub
End Class