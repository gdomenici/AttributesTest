# Info

VB.NET reference: https://learn.microsoft.com/en-us/dotnet/api/

# To test

On the server:
```
cd AttributesTest\MyWcfService

dotnet run
```

On the client:
```
test.ps1
```


# claude.ai's take on how to intercept WCF calls

From: https://claude.ai/chat/4d66f9b0-685a-4c35-9591-0ef3f852bed0


WCF actually has a **built-in extensibility mechanism** designed exactly for this: **`IOperationInvoker`** and, more conveniently, **`IOperationBehavior`** combined with **`IParameterInspector`**.

You don't need Castle or any AOP library. WCF's pipeline has well-defined hooks.

---

## The right hook: `IParameterInspector`

This is the cleanest fit for your use case. WCF calls `BeforeCall` **before** invoking the operation, and `AfterCall` after. You inject it via an `IOperationBehavior`, which you can wire directly to your attribute.

The elegant trick: **make your attribute implement `IOperationBehavior` itself.**

```vbnet
Imports System.ServiceModel
Imports System.ServiceModel.Description
Imports System.ServiceModel.Dispatcher
Imports System.Reflection

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
```

```vbnet
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
```

Then your service contract looks like this:

```vbnet
<ServiceContract>
Public Interface IMyService
    <OperationContract>
    <CustomPermission("Admin")>
    Sub DoSomething()

    <OperationContract>
    <CustomPermission("SuperUser")>   ' will always throw with the hardcoded rule
    Sub DoSomethingElse()
End Interface
```

---

## Why this works

WCF reads `IOperationBehavior` attributes off the service contract methods **automatically** when building the dispatch runtime — you don't need to register anything. The moment WCF sees that your attribute implements `IOperationBehavior`, it calls `ApplyDispatchBehavior` during host initialization, which is your chance to mutate the `DispatchOperation` and plant your inspector.

The call flow at runtime becomes:

```
WCF receives SOAP request
    → deserializes parameters
    → calls PermissionParameterInspector.BeforeCall   ← your code
    → (if no exception) calls the actual service method
    → calls PermissionParameterInspector.AfterCall
    → serializes response
```

---

## One caveat for .NET Core / CoreWCF

If you're using **CoreWCF** (the .NET Core port), the pipeline is the same and `IOperationBehavior` / `IParameterInspector` are fully supported. Just make sure you're referencing `CoreWCF.Primitives` rather than the old `System.ServiceModel`. The interface names and method signatures are identical, so the code above works as-is.