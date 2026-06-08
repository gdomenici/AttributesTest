Imports CoreWCF

<ServiceContract>
Public Interface IMyService
    <OperationContract>
    <CustomPermission("Admin")>
    Function Hello(name As String) As String
End Interface