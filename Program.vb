Imports System
Imports CoreWCF
Imports CoreWCF.Configuration
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.Extensions.DependencyInjection

Module Program
    Sub Main(args As String())

        Dim builder = WebApplication.CreateBuilder(args)

        builder.Services.AddServiceModelServices()

        builder.WebHost.UseUrls("http://0.0.0.0:5000")
        Dim app As IApplicationBuilder = builder.Build()

        app.UseServiceModel(Sub(serviceBuilder)
                                serviceBuilder.AddService(Of MyService)(Sub(serviceOptions)
                                                                            serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = True
                                                                        End Sub)
                                serviceBuilder.AddServiceEndpoint(Of MyService, IMyService)(
                                    New BasicHttpBinding(),
                                    "/MyService.svc")
                            End Sub)

        Dim webApp As WebApplication = app
        webApp.Run()
    End Sub
End Module
