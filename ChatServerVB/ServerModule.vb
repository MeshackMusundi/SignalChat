Imports Microsoft.AspNet.SignalR
Imports Microsoft.Owin.Cors
Imports Microsoft.Owin.Hosting
Imports Owin

Module ServerModule

    Sub Main()
        Dim url = "http://localhost:8080/"
        Using WebApp.Start(Of Startup)(url)
            Console.WriteLine($"Server running at {url}")
            Console.ReadLine()
        End Using
    End Sub

End Module

Public Class Startup
    Public Sub Configuration(app As IAppBuilder)
        app.UseCors(CorsOptions.AllowAll)
        app.MapSignalR("/signalchat", New HubConfiguration())

        GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = Nothing
    End Sub
End Class