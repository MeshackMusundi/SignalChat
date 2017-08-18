Public Interface IDialogService
    Sub ShowNotification(message As String, Optional caption As String = "")
    Function ShowConfirmationRequest(message As String, Optional caption As String = "") As Boolean
    Function OpenFile(caption As String, Optional filter As String = "All files (*.*)|*.*") As String
End Interface
