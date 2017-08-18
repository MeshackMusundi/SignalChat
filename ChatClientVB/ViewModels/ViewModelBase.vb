Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class ViewModelBase
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Public Sub OnPropertyChanged(<CallerMemberName()> Optional ByVal name As String = Nothing)
        If name IsNot Nothing Then
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
        End If
    End Sub
End Class
