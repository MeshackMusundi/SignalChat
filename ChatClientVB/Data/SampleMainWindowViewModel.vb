Imports System.Collections.ObjectModel

Public Class SampleMainWindowViewModel
    Inherits ViewModelBase

    Private _userName As String
    Public Property UserName As String
        Get
            If _userName Is Nothing Then _userName = String.Empty
            Return _userName
        End Get
        Set(value As String)
            _userName = value
            OnPropertyChanged()
        End Set
    End Property

    Private _participants As New ObservableCollection(Of Participant)
    Public Property Participants As ObservableCollection(Of Participant)
        Get
            Return _participants
        End Get
        Set(value As ObservableCollection(Of Participant))
            _participants = value
            OnPropertyChanged()
        End Set
    End Property

    Private _selectedParticipant As Participant
    Public Property SelectedParticipant As Participant
        Get
            Return _selectedParticipant
        End Get
        Set(value As Participant)
            _selectedParticipant = value
            OnPropertyChanged()
        End Set
    End Property

    Public Sub New()
        Dim someChatter As New ObservableCollection(Of ChatMessage)
        someChatter.Add(New ChatMessage With {.Author = "Batman", .Message = "The Batmobile sucks",
                        .Time = DateTime.Now, .IsOriginNative = True})
        someChatter.Add(New ChatMessage With {.Author = "Superman", .Message = "It always has...",
                        .Time = DateTime.Now})
        someChatter.Add(New ChatMessage With {.Author = "Batman", .Message = "Really?! You never said so before.",
                        .Time = DateTime.Now, .IsOriginNative = True})
        someChatter.Add(New ChatMessage With {.Author = "Superman",
                        .Message = "Didn't want to hurt your feelings :D. Lorem Ipsum something blah blah blah blah blah blah blah blah.
                        Lorem Ipsum something blah blah blah blah.",
                        .Time = DateTime.Now})
        someChatter.Add(New ChatMessage With {.Author = "Batman", .Message = "I have no feelings",
                        .Time = DateTime.Now, .IsOriginNative = True})
        someChatter.Add(New ChatMessage With {.Author = "Batman", .Message = "How's Martha?",
                        .Time = DateTime.Now, .IsOriginNative = True})

        Participants.Add(New Participant With {.Name = "Superman", .Chatter = someChatter, .IsTyping = True})
        Participants.Add(New Participant With {.Name = "Wonder Woman", .Chatter = someChatter, .IsLoggedIn = False})
        Participants.Add(New Participant With {.Name = "Aquaman", .Chatter = someChatter, .HasSentNewMessage = True})
        Participants.Add(New Participant With {.Name = "Captain Canada", .Chatter = someChatter, .HasSentNewMessage = True})
        Participants.Add(New Participant With {.Name = "Iron Man", .Chatter = someChatter, .IsTyping = True})

        SelectedParticipant = Participants.First
    End Sub
End Class
