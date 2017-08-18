Public Class RelayCommandAsync
    Implements ICommand

    Private ReadOnly _execute As Func(Of Task)
    Private ReadOnly _canExecute As Predicate(Of Object)
    Private isExecuting As Boolean

    Public Sub New(execute As Func(Of Task))
        Me.New(execute, Nothing)
    End Sub

    Public Sub New(execute As Func(Of Task), canExecute As Predicate(Of Object))
        _execute = execute
        _canExecute = canExecute
    End Sub

    Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
        If Not isExecuting AndAlso _canExecute Is Nothing Then Return True
        Return Not isExecuting AndAlso _canExecute(parameter)
    End Function

    Public Custom Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        AddHandler(value As EventHandler)
            AddHandler CommandManager.RequerySuggested, value
        End AddHandler

        RemoveHandler(value As EventHandler)
            RemoveHandler CommandManager.RequerySuggested, value
        End RemoveHandler

        RaiseEvent(sender As Object, e As EventArgs)
        End RaiseEvent
    End Event

    Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
        isExecuting = True
        Try
            Await _execute()
        Finally
            isExecuting = False
        End Try
    End Sub
End Class
