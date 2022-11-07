Imports Migration.Configuration

Namespace Migration
	Public NotInheritable Class Program

		<STAThread()> _
		Public Shared Sub Main(ByVal inArgs() As String)
			AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf CurrentDomain_UnhandledException
			Try
				Game.Setup.Language = MarkupLanguage.Load("Migration.config.xml")
				MarkupLanguage.Save(Game.Setup.Language, "Migration.config.xml")

				' parse command line arguments
				If inArgs.Length >= 1 Then
					Select Case inArgs(0)
						Case "atlas:generate"
							GenerateTextureAtlas()
							Return

						Case "atlas:createcache"
							CreateTextureAtlasCache()
							Return

						Case Else
					End Select
				End If

				AddHandler Log.OnCriticalException, AddressOf HandleCriticalException

                ' Use a Main thread to kickstart things off
				Dim thread As New System.Threading.Thread(Sub() InitializeProgram())
				thread.IsBackground = True
				thread.Start()
				thread.Join()

			Catch e As Exception
				Log.LogExceptionCritical(e)
			End Try
		End Sub

		Private Shared Sub HandleCriticalException(ByVal ex As System.Exception)
			CurrentDomain_UnhandledException(Nothing, Nothing)
		End Sub

		Private Shared Sub InitializeProgram()
			Game.Setup.Initialize()
			Game.Setup.Renderer.WaitForTermination()
		End Sub

		Private Shared Sub CurrentDomain_UnhandledException(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
			Try
				If e IsNot Nothing Then
					Log.LogException(CType(e.ExceptionObject, Exception))
				End If
			Catch
			End Try

			Try
				StoreGameState(True)
			Catch
			End Try
		End Sub

		Public Shared Sub StoreGameState(ByVal inFromException As Boolean)
			' store current game state
			Dim mMap As Migration.Game.Map = Game.Setup.Map

			If mMap IsNot Nothing Then
				Dim filename As String = ".\GameLog." & Date.Now.ToString("yyyy-MM-dd_HH-mm-ss") & ".s3g"

				mMap.Save(filename)

				If Not inFromException Then
					System.IO.File.Copy(filename, ".\GameLog.s3g", True)
				End If
			End If
		End Sub

		Private Shared Sub GenerateTextureAtlas()

		End Sub

		Private Shared Sub CreateTextureAtlasCache()

		End Sub

	End Class
End Namespace
