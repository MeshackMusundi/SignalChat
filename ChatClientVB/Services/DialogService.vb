Imports Microsoft.Win32

Public Class DialogService
    Implements IDialogService

    Public Sub ShowNotification(message As String, Optional caption As String = "") Implements IDialogService.ShowNotification
        MessageBox.Show(message, caption)
    End Sub

    Public Function OpenFile(caption As String, Optional filter As String = "All files (*.*)|*.*") As String Implements IDialogService.OpenFile
        Dim diag As New OpenFileDialog
        diag.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        diag.Title = caption
        diag.Filter = filter
        diag.CheckFileExists = True
        diag.CheckPathExists = True
        diag.RestoreDirectory = True

        If diag.ShowDialog() = True Then Return diag.FileName
        Return String.Empty
    End Function

    Public Function ShowConfirmationRequest(message As String, Optional caption As String = "") As Boolean Implements IDialogService.ShowConfirmationRequest
        Dim result = MessageBox.Show(message, caption, MessageBoxButton.OKCancel)
        Return result.HasFlag(MessageBoxResult.OK)
    End Function
End Class
