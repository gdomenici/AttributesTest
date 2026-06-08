Imports System.Reflection
Imports System.ServiceModel
Imports System.ServiceModel.Description
Imports System.ServiceModel.Dispatcher
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