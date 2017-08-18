Imports System.Collections.Specialized
Imports System.Windows.Interactivity

Public Class BringNewItemIntoViewBehavior
    Inherits Behavior(Of ItemsControl)

    Private notifier As INotifyCollectionChanged

    Protected Overrides Sub OnAttached()
        MyBase.OnAttached()
        notifier = CType(AssociatedObject.Items, INotifyCollectionChanged)
        AddHandler notifier.CollectionChanged, AddressOf ItemsControl_CollectionChanged
    End Sub

    Protected Overrides Sub OnDetaching()
        MyBase.OnDetaching()
        RemoveHandler notifier.CollectionChanged, AddressOf ItemsControl_CollectionChanged
    End Sub

    Private Sub ItemsControl_CollectionChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
        If e.Action = NotifyCollectionChangedAction.Add Then
            Dim newIndex = e.NewStartingIndex
            Dim newElement = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(newIndex)
            Dim item = CType(newElement, FrameworkElement)
            If item IsNot Nothing Then item.BringIntoView()
        End If
    End Sub
End Class
