Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Windows
Imports System.Windows.Threading

Namespace Migration.Editor
	''' <summary>
	''' Interaction logic for MainWindow.xaml
	''' </summary>
	Partial Public Class MainWindow
		Inherits Window

		Public Shared CurrentSequenceProperty As DependencyProperty = DependencyProperty.Register("CurrentSequence", GetType(GFXSequence), GetType(MainWindow))

		Private Shared privateInstance As MainWindow
		Public Shared Property Instance() As MainWindow
			Get
				Return privateInstance
			End Get
			Private Set(ByVal value As MainWindow)
				privateInstance = value
			End Set
		End Property

		Private m_UpdateTimer500 As New DispatcherTimer()
		Private m_GFXFile As GFXFile
		Private m_IsLoading As Boolean
		Private m_HasGFXFileChanged As Boolean

		Private privateGUISeqs As BindingList(Of GFXSequence)
		Public Property GUISeqs() As BindingList(Of GFXSequence)
			Get
				Return privateGUISeqs
			End Get
			Private Set(ByVal value As BindingList(Of GFXSequence))
				privateGUISeqs = value
			End Set
		End Property
		Private privateObjectSeqs As BindingList(Of GFXSequence)
		Public Property ObjectSeqs() As BindingList(Of GFXSequence)
			Get
				Return privateObjectSeqs
			End Get
			Private Set(ByVal value As BindingList(Of GFXSequence))
				privateObjectSeqs = value
			End Set
		End Property
		Private privateTorsoSeqs As BindingList(Of GFXSequence)
		Public Property TorsoSeqs() As BindingList(Of GFXSequence)
			Get
				Return privateTorsoSeqs
			End Get
			Private Set(ByVal value As BindingList(Of GFXSequence))
				privateTorsoSeqs = value
			End Set
		End Property
		Private privateShadowSeqs As BindingList(Of GFXSequence)
		Public Property ShadowSeqs() As BindingList(Of GFXSequence)
			Get
				Return privateShadowSeqs
			End Get
			Private Set(ByVal value As BindingList(Of GFXSequence))
				privateShadowSeqs = value
			End Set
		End Property
		Private privateLandscapeSeqs As BindingList(Of GFXSequence)
		Public Property LandscapeSeqs() As BindingList(Of GFXSequence)
			Get
				Return privateLandscapeSeqs
			End Get
			Private Set(ByVal value As BindingList(Of GFXSequence))
				privateLandscapeSeqs = value
			End Set
		End Property

		Public Property CurrentSequence() As GFXSequence
			Get
				Return CType(GetValue(CurrentSequenceProperty), GFXSequence)
			End Get
			Set(ByVal value As GFXSequence)
				SetValue(CurrentSequenceProperty, value)
			End Set
		End Property

		Public Sub New()
			'VisualUtilities.Instance = New AnimationUtilities()
			AnimationLibrary.OpenFromDirectory("./Resources/Animations/")

			If Instance IsNot Nothing Then
				Throw New ApplicationException("AnimationEditor is already opened.")
			End If

			Instance = Me

			AddHandler Application.Current.DispatcherUnhandledException, AddressOf Current_DispatcherUnhandledException

			GUISeqs = New BindingList(Of GFXSequence)()
			ObjectSeqs = New BindingList(Of GFXSequence)()
			TorsoSeqs = New BindingList(Of GFXSequence)()
			ShadowSeqs = New BindingList(Of GFXSequence)()
			LandscapeSeqs = New BindingList(Of GFXSequence)()

			InitializeComponent()

			DataContext = Me
			m_UpdateTimer500.Interval = TimeSpan.FromMilliseconds(500)
			AddHandler m_UpdateTimer500.Tick, AddressOf m_UpdateTimer500_Tick
			m_UpdateTimer500.Start()

			m_GFXFile = New GFXFile()
		End Sub

		Private Sub LoadGFXFile(ByVal inFileName As String)
			m_HasGFXFileChanged = False
			m_IsLoading = True
			m_GFXFile.BeginLoad(inFileName)
		End Sub

		Private Sub Current_DispatcherUnhandledException(ByVal sender As Object, ByVal e As DispatcherUnhandledExceptionEventArgs)
			Log.LogExceptionModal(e.Exception)
			e.Handled = True
		End Sub

		Private Sub m_UpdateTimer500_Tick(ByVal sender As Object, ByVal e As EventArgs)
			PROGRESS_GfxLoad.Value = m_GFXFile.Progress

			If m_IsLoading AndAlso m_GFXFile.IsLoaded Then
				m_IsLoading = False

				TEXT_GfxFilePath.Text = m_GFXFile.FileName

				LandscapeSeqs.Clear()
				GUISeqs.Clear()
				ObjectSeqs.Clear()
				TorsoSeqs.Clear()
				ShadowSeqs.Clear()

				For Each seq In m_GFXFile.LandscapeSeqs
					LandscapeSeqs.Add(seq)
				Next seq

				For Each seq In m_GFXFile.GUISeqs
					GUISeqs.Add(seq)
				Next seq

				For Each seq In m_GFXFile.ObjectSeqs
					ObjectSeqs.Add(seq)
				Next seq

				For Each seq In m_GFXFile.TorsoSeqs
					TorsoSeqs.Add(seq)
				Next seq

				For Each seq In m_GFXFile.ShadowSeqs
					ShadowSeqs.Add(seq)
				Next seq

				TAB_Landscapes.Header = "Landscapes (" & LandscapeSeqs.Count & ")"
				TAB_GfxGUI.Header = "GUI (" & GUISeqs.Count & ")"
				TAB_GfxObjects.Header = "Objects (" & ObjectSeqs.Count & ")"
				TAB_GfxShadows.Header = "Shadows (" & ShadowSeqs.Count & ")"
				TAB_GfxTorso.Header = "Torso (" & TorsoSeqs.Count & ")"
			End If
		End Sub

		Private Sub Window_Closing(ByVal sender As Object, ByVal e As CancelEventArgs)
			If m_HasGFXFileChanged Then
				m_GFXFile.Save()
			End If

			TAB_AnimLibrary.Close()
		End Sub

		Private Sub BTN_GfxLoad_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim dlg As New System.Windows.Forms.OpenFileDialog() With {.Title = "Open GFX-File...", .Multiselect = False, .CheckFileExists = True, .CheckPathExists = True, .Filter = "Settlers 3 GFX Files (*.dat)|*.dat|All Files (*.*)|*.*"}
			'dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

			If dlg.ShowDialog() <> System.Windows.Forms.DialogResult.OK Then
				Return
			End If

			If Not(File.Exists(dlg.FileName)) Then
				Throw New FileNotFoundException("The given file does not exist.", dlg.FileName)
			End If

			LoadGFXFile(dlg.FileName)
		End Sub

		Private Sub BTN_GfxRemoveSequence_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			If CurrentSequence Is Nothing Then
				Return
			End If

			If LandscapeSeqs.Contains(CurrentSequence) Then
				m_GFXFile.LandscapeSeqs.Remove(CurrentSequence)
				LandscapeSeqs.Remove(CurrentSequence)
			ElseIf GUISeqs.Contains(CurrentSequence) Then
				m_GFXFile.GUISeqs.Remove(CurrentSequence)
				GUISeqs.Remove(CurrentSequence)
			ElseIf ObjectSeqs.Contains(CurrentSequence) Then
				m_GFXFile.ObjectSeqs.Remove(CurrentSequence)
				ObjectSeqs.Remove(CurrentSequence)
			ElseIf TorsoSeqs.Contains(CurrentSequence) Then
				m_GFXFile.TorsoSeqs.Remove(CurrentSequence)
				TorsoSeqs.Remove(CurrentSequence)
			ElseIf ShadowSeqs.Contains(CurrentSequence) Then
				m_GFXFile.ShadowSeqs.Remove(CurrentSequence)
				ShadowSeqs.Remove(CurrentSequence)
			End If

			TAB_Landscapes.Header = "Landscapes (" & LandscapeSeqs.Count & ")"
			TAB_GfxGUI.Header = "GUI (" & GUISeqs.Count & ")"
			TAB_GfxObjects.Header = "Objects (" & ObjectSeqs.Count & ")"
			TAB_GfxShadows.Header = "Torso (" & ShadowSeqs.Count & ")"
			TAB_GfxTorso.Header = "Shadows (" & TorsoSeqs.Count & ")"

			m_HasGFXFileChanged = True
			CurrentSequence = Nothing
		End Sub

		Private Sub BTN_GfxRemoveFrame_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim frame = CType(LIST_SeqFrames.SelectedItem, GFXFrame)
			Dim seq = CurrentSequence

			If frame Is Nothing Then
				Return
			End If

			m_HasGFXFileChanged = True
			seq.Frames.Remove(frame)
			CurrentSequence = Nothing
			CurrentSequence = seq
		End Sub

		Private Sub BTN_GfxExportAllSeqs_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim seqList() As List(Of GFXSequence) = { m_GFXFile.LandscapeSeqs, m_GFXFile.GUISeqs, m_GFXFile.ObjectSeqs, m_GFXFile.TorsoSeqs, m_GFXFile.ShadowSeqs }

			For i As Integer = 0 To 4
				If seqList(i).Count = 0 Then
					Continue For
				End If

				For Each seq In seqList(i)
					GfxExportSequence(seq)
				Next seq
			Next i
		End Sub

		Private Sub BTN_GfxExportSeq_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			If CurrentSequence Is Nothing Then
				Return
			End If

			GfxExportSequence(CurrentSequence)
		End Sub

		Private Sub GfxExportSequence(ByVal inSequence As GFXSequence)
			Dim gfxDir As String = System.IO.Path.GetDirectoryName(m_GFXFile.FileName) & "/" & System.IO.Path.GetFileNameWithoutExtension(m_GFXFile.FileName)
			Dim names() As String = { "Landscapes", "GUI", "Objects", "Torso", "Shadows" }
			Dim seqList() As List(Of GFXSequence) = { m_GFXFile.LandscapeSeqs, m_GFXFile.GUISeqs, m_GFXFile.ObjectSeqs, m_GFXFile.TorsoSeqs, m_GFXFile.ShadowSeqs }

			If File.Exists(gfxDir) Then
				Throw New ArgumentException("A file with the target directory name """ & gfxDir & """ does already exist!")
			End If

			For i As Integer = 0 To 4
'INSTANT VB NOTE: The variable name was renamed since Visual Basic does not handle local variables named the same as class members well:
				Dim name_Renamed As String = names(i)

				If Not(seqList(i).Contains(inSequence)) Then
					Continue For
				End If

				Directory.CreateDirectory(gfxDir & "/" & name_Renamed & "/" & inSequence.Index.ToString())

				For Each frame In inSequence.Frames
					frame.Image.Save(gfxDir & "/" & name_Renamed & "/" & inSequence.Index & "/" & frame.Index & "_" & frame.OffsetX & "_" & frame.OffsetY & ".png")
				Next frame
			Next i
		End Sub

		Private Sub BTN_GfxFrameToClipboard_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim frame = CType(LIST_SeqFrames.SelectedItem, GFXFrame)

			If frame Is Nothing Then
				Return
			End If

			System.Windows.Forms.Clipboard.SetImage(frame.Image)
		End Sub
	End Class



	Public Class DrawingToImageSource
		Implements IValueConverter

		Public Shared ReadOnly Instance As New DrawingToImageSource()

		Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.Convert
			Dim bitmap As System.Drawing.Bitmap = TryCast(value, System.Drawing.Bitmap)

			If bitmap Is Nothing Then
				Return Nothing
			End If

			Dim stream As New MemoryStream()

			bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png)

			Return BitmapFrame.Create(stream)
		End Function

		Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
			Throw New NotImplementedException()
		End Function
	End Class
End Namespace
