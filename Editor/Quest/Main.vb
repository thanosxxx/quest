Imports TextAdventures.Utility

Public Class Main

    Private m_currentFile As String
    Private m_playingEditorGame As Boolean = False
    Private m_cmdLineLaunch As String = Nothing
    Private m_fromEditor As Boolean
    Private m_editorSimpleMode As Boolean

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        AddHandler System.Windows.Threading.Dispatcher.CurrentDispatcher.UnhandledException, AddressOf CurrentDispatcher_UnhandledException

        ' Add any initialization after the InitializeComponent() call.
        ctlLauncher.QuestVersion = My.Application.Info.Version
        InitialiseMenuHandlers()

        Dim helper As New TextAdventures.Utility.WindowHelper(Me, "Quest", "Main")

        Dim args As New List(Of String)(Environment.GetCommandLineArgs())
        If args.Count > 1 Then
            CmdLineLaunch(args(1))
        End If

        AddHandler ctlLauncher.BrowseForGameEdit, AddressOf ctlLauncher_BrowseForGameEdit
        AddHandler ctlLauncher.CreateNewGame, AddressOf ctlLauncher_CreateNewGame
        AddHandler ctlLauncher.EditGame, AddressOf ctlLauncher_EditGame
        AddHandler ctlLauncher.Tutorial, AddressOf ctlLauncher_Tutorial
    End Sub

    Private Sub CurrentDispatcher_UnhandledException(sender As Object, e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)
        ErrorHandler.ShowError(e.Exception.ToString())
        e.Handled = True
    End Sub

    Private Sub InitialiseMenuHandlers()
        ctlMenu.AddMenuClickHandler("about", AddressOf AboutMenuClick)
        ctlMenu.AddMenuClickHandler("exit", AddressOf ExitMenuClick)
        ctlMenu.AddMenuClickHandler("openedit", AddressOf OpenEditMenuClick)
        ctlMenu.AddMenuClickHandler("createnew", AddressOf CreateNewMenuClick)
        ctlMenu.AddMenuClickHandler("viewhelp", AddressOf Help)
        ctlMenu.AddMenuClickHandler("forums", AddressOf Forums)
        ctlMenu.AddMenuClickHandler("logbug", AddressOf LogBug)
    End Sub

    Private Sub ctlLauncher_BrowseForGameEdit()
        BrowseEdit()
    End Sub

    Private Sub ctlLauncher_CreateNewGame()
        CreateNewMenuClick()
    End Sub

    Private Sub ctlLauncher_EditGame(filename As String)
        LaunchEdit(filename)
    End Sub

    Private Sub ctlLauncher_Tutorial()
        Tutorial()
    End Sub

    Private Sub LaunchEdit(filename As String)
        Dim ext As String

        Try
            Me.Cursor = Cursors.WaitCursor
            ext = System.IO.Path.GetExtension(filename)

            Select Case ext
                Case ".aslx"
                    Me.SuspendLayout()
                    ctlMenu.Mode = Quest.Controls.Menu.MenuMode.Editor
                    ctlLauncherHost.Visible = False
                    ctlEditor.Visible = True
                    ctlEditor.SetMenu(ctlMenu)
                    Me.ResumeLayout()
                    Application.DoEvents()
                    ctlEditor.Initialise(filename)
                    ctlEditor.Focus()
                Case Else
                    MsgBox(String.Format("Unrecognised file type '{0}'", ext))
            End Select

        Catch ex As Exception
            MsgBox("Error loading game: " + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical)
            Me.Cursor = Cursors.Default
        End Try

    End Sub

    Private Sub ctlEditor_InitialiseFinished(success As Boolean) Handles ctlEditor.InitialiseFinished
        Me.Cursor = Cursors.Default
        ctlMenu.Visible = True
        If Not success Then
            CloseEditor()
        End If
    End Sub

    Private Sub AboutMenuClick()
        Dim frmAbout As New About
        frmAbout.ShowDialog()
    End Sub

    Private Sub SetWindowTitle(Optional gameName As String = "")
        Dim caption As String
        caption = "Quest Editor"
        If Not String.IsNullOrEmpty(gameName) Then caption += " - " + gameName
        Me.Text = caption
    End Sub

    Private Sub ExitMenuClick()
        Me.Close()
    End Sub

    Private Sub OpenEditMenuClick()
        If Not ctlEditor.CheckGameIsSaved("Do you wish to save your changes before opening a new game?") Then Return
        BrowseEdit()
    End Sub

    Private Sub CreateNewMenuClick()
        If Not ctlEditor.CheckGameIsSaved("Do you wish to save your changes before creating a new game?") Then Return

        Dim newFile = ctlEditor.CreateNewGame()
        If String.IsNullOrEmpty(newFile) Then Return

        LaunchEdit(newFile)
    End Sub

    Private Sub BrowseEdit()
        Dim startFolder As String = DirectCast(Registry.GetSetting("Quest", "Settings", "StartFolder", _
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)), String)

        dlgOpenFile.InitialDirectory = startFolder
        dlgOpenFile.Multiselect = False
        dlgOpenFile.Filter = "Quest Games (*.aslx)|*.aslx|All files|*.*"
        dlgOpenFile.FileName = ""
        dlgOpenFile.ShowDialog()
        If dlgOpenFile.FileName.Length > 0 Then
            Registry.SaveSetting("Quest", "Settings", "StartFolder", System.IO.Path.GetDirectoryName(dlgOpenFile.FileName))

            LaunchEdit(dlgOpenFile.FileName)
        End If

    End Sub

    Private Sub ctlEditor_AddToRecent(filename As String, name As String) Handles ctlEditor.AddToRecent
        ctlLauncher.AddToEditorRecent(filename, name)
    End Sub

    Private Sub Main_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not ctlEditor.CloseEditor(False, True) Then
            e.Cancel = True
        End If
    End Sub

    Private Sub ctlEditor_Close() Handles ctlEditor.Close
        CloseEditor()
    End Sub

    Private Sub CloseEditor()
        ctlMenu.Mode = Quest.Controls.Menu.MenuMode.GameBrowser
        ctlLauncher.RefreshLists()
        ctlEditor.Visible = False
        ctlEditor.CancelUnsavedChanges()
        ctlLauncherHost.Visible = True
        SetWindowTitle()
    End Sub

    Private Sub ctlEditor_Loaded(name As String) Handles ctlEditor.Loaded
        SetWindowTitle(name)
    End Sub

    Private Sub ctlEditor_NewGame() Handles ctlEditor.NewGame
        CreateNewMenuClick()
    End Sub

    Private Sub ctlEditor_OpenGame() Handles ctlEditor.OpenGame
        OpenEditMenuClick()
    End Sub

    Private Sub ctlEditor_Play(filename As String) Handles ctlEditor.Play
        Dim assembly = System.Reflection.Assembly.GetEntryAssembly()
        Dim path = IO.Path.GetDirectoryName(assembly.Location)
        Dim questRunner = IO.Path.Combine(path, "QuestPlayer.exe")
        Process.Start(questRunner, String.Format("""" + filename + """"))
    End Sub

    Private Sub ctlEditor_PlayWalkthrough(filename As String, walkthrough As String, record As Boolean) Handles ctlEditor.PlayWalkthrough
    End Sub

    Private Sub Main_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        If m_cmdLineLaunch IsNot Nothing Then
            LaunchEdit(m_cmdLineLaunch)
        End If
    End Sub

    Private Sub CmdLineLaunch(filename As String)
        ctlLauncherHost.Visible = False
        m_cmdLineLaunch = filename
    End Sub

    Private Sub LogBug()
        LaunchURL("https://github.com/textadventures/quest/issues")
    End Sub

    Private Sub Forums()
        LaunchURL("http://forum.textadventures.co.uk/")
    End Sub

    Private Sub Help()
        LaunchURL("http://docs.textadventures.co.uk/quest/")
    End Sub

    Private Sub Tutorial()
        LaunchURL("http://docs.textadventures.co.uk/quest/tutorial/")
    End Sub

    Private Sub LaunchURL(url As String)
        Try
            System.Diagnostics.Process.Start(url)
        Catch ex As Exception
            MsgBox(String.Format("Error launching {0}{1}{2}", url, Environment.NewLine + Environment.NewLine, ex.Message), MsgBoxStyle.Critical, "Quest Editor")
        End Try
    End Sub

    Private m_fullScreen As Boolean

    Public Property FullScreen As Boolean
        Get
            Return m_fullScreen
        End Get
        Set(value As Boolean)
            If m_fullScreen <> value Then
                m_fullScreen = value
                Me.FormBorderStyle = If(m_fullScreen, Windows.Forms.FormBorderStyle.None, Windows.Forms.FormBorderStyle.Sizable)
                Me.WindowState = If(m_fullScreen, FormWindowState.Maximized, FormWindowState.Normal)
                ctlMenu.Visible = Not m_fullScreen
            End If
        End Set
    End Property
End Class
