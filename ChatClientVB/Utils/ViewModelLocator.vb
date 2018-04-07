Imports Unity

Public Class ViewModelLocator
    Private container As UnityContainer

    Public Sub New()
        container = New UnityContainer
        container.RegisterType(Of IChatService, ChatService)()
        container.RegisterType(Of IDialogService, DialogService)()
    End Sub

    Public ReadOnly Property MainVM As MainWindowViewModel
        Get
            Return container.Resolve(Of MainWindowViewModel)()
        End Get
    End Property
End Class
