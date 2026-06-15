Imports System.Security
Imports CoreWCF
Imports CoreWCF.Channels
Imports CoreWCF.Description
Imports CoreWCF.Dispatcher

<AttributeUsage(AttributeTargets.Method)>
Public Class CustomPermissionAttribute
    Inherits Attribute
    Implements IOperationBehavior

    Public Property RoleName As String

    Public Sub New(roleName As String)
        Me.RoleName = roleName
    End Sub

    ' IOperationBehavior — only AddBindingParameters is interesting to us
    Public Sub AddBindingParameters(operationDescription As OperationDescription,
                                    bindingParameters As BindingParameterCollection) _
        Implements IOperationBehavior.AddBindingParameters
        ' nothing needed
    End Sub

    Public Sub ApplyClientBehavior(operationDescription As OperationDescription,
                                   clientOperation As ClientOperation) _
        Implements IOperationBehavior.ApplyClientBehavior
        ' nothing needed (client side)
    End Sub

    Public Sub ApplyDispatchBehavior(operationDescription As OperationDescription,
                                     dispatchOperation As DispatchOperation) _
        Implements IOperationBehavior.ApplyDispatchBehavior
        ' THIS is where we inject our inspector into the WCF pipeline
        dispatchOperation.ParameterInspectors.Add(New PermissionParameterInspector(Me.RoleName))
    End Sub

    Public Sub Validate(operationDescription As OperationDescription) _
        Implements IOperationBehavior.Validate
        ' nothing needed
    End Sub
End Class




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
