Imports System.Collections.ObjectModel

Public Class Participant
    Inherits ViewModelBase

    Public Property Name As String
    Public Property Photo As Byte()
    Public Property Chatter As New ObservableCollection(Of ChatMessage)

    Private _isLoggedIn As Boolean = True
    Public Property IsLoggedIn As Boolean
        Get
            Return _isLoggedIn
        End Get
        Set(value As Boolean)
            _isLoggedIn = value
            OnPropertyChanged()
        End Set
    End Property

    Private _hasSentNewMessage As Boolean
    Public Property HasSentNewMessage As Boolean
        Get
            Return _hasSentNewMessage
        End Get
        Set(value As Boolean)
            _hasSentNewMessage = value
            OnPropertyChanged()
        End Set
    End Property

    Private _isTyping As Boolean
    Public Property IsTyping As Boolean
        Get
            Return _isTyping
        End Get
        Set(value As Boolean)
            _isTyping = value
            OnPropertyChanged()
        End Set
    End Property
End Class
