Public Class Launcher
    Public Event BrowseForGame()
    Public Event LaunchGame(filename As String)
    Public Event EditGame(filename As String)
    Public Event CreateNewGame()
    Public Event BrowseForGameEdit()
    Public Event Tutorial()

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ctlVersionInfo.Visibility = Windows.Visibility.Collapsed
        AddHandler ctlEditBrowser.EditGame, AddressOf ctlEditBrowser_EditGame
        AddHandler ctlEditBrowser.CreateNewGame, AddressOf ctlEditBrowser_CreateNewGame
        AddHandler ctlEditBrowser.OpenGame, AddressOf ctlEditBrowser_OpenGame
        AddHandler ctlEditBrowser.Tutorial, AddressOf ctlEditBrowser_Tutorial
    End Sub

    Public Sub AddToEditorRecent(filename As String, name As String)
        ctlEditBrowser.AddToRecent(filename, name)
    End Sub

    Private Sub ctlPlayBrowser_LaunchGame(filename As String)
        RaiseEvent LaunchGame(filename)
    End Sub

    Private Sub ctlEditBrowser_EditGame(filename As String)
        RaiseEvent EditGame(filename)
    End Sub

    Private Sub ctlEditBrowser_CreateNewGame()
        RaiseEvent CreateNewGame()
    End Sub

    Private Sub ctlEditBrowser_OpenGame()
        RaiseEvent BrowseForGameEdit()
    End Sub

    Private Sub ctlEditBrowser_Tutorial()
        RaiseEvent Tutorial()
    End Sub

    Public Sub RefreshLists()
        ctlEditBrowser.Populate()
    End Sub

    Private m_questVersion As Version

    Public Property QuestVersion As Version
        Get
            Return m_questVersion
        End Get
        Set(value As Version)
            m_questVersion = value
        End Set
    End Property

    Private Sub ctlPlayBrowser_GotUpdateData(data As UpdatesData)
        ctlVersionInfo.UpdateInfo = data
        If IsNewVersion(data) Then
            Dispatcher.BeginInvoke(Sub()
                                       ctlVersionInfo.UpdateInfo = data
                                       ctlVersionInfo.Visibility = Windows.Visibility.Visible
                                   End Sub)
        End If
    End Sub

    Private Function IsNewVersion(updateData As UpdatesData) As Boolean
        If updateData Is Nothing Then Return False
        Dim latestMajor As Integer
        Dim latestMinor As Integer
        Dim latestBuild As Integer
        Dim latestRevision As Integer

        Integer.TryParse(updateData.Major, latestMajor)
        Integer.TryParse(updateData.Minor, latestMinor)
        Integer.TryParse(updateData.Build, latestBuild)
        Integer.TryParse(updateData.Revision, latestRevision)

        Dim latestVersion As New Version(latestMajor, latestMinor, latestBuild, latestRevision)
        Return latestVersion > QuestVersion
    End Function

End Class
