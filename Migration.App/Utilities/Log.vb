Imports System.IO
Imports Migration.Common


Namespace Migration
	Public NotInheritable Class Log
		Private Shared ReadOnly m_Lock As New Object()

		Public Shared Event OnCriticalException As Procedure(Of Exception)

		Private Sub New()
		End Sub
		Shared Sub New()
			File.Delete("./log.txt")
		End Sub

		Public Shared Sub LogExceptionModal(ByVal e As Exception)
			SyncLock m_Lock
				LogRaw("### ERROR ==> ", e.ToString())

				VisualUtilities.ShowError(e.ToString())
			End SyncLock
		End Sub

		Public Shared Sub LogExceptionCritical(ByVal e As Exception)
			LogExceptionModal(e)

			Try
				RaiseEvent OnCriticalException(e)
			Catch
			End Try

			' give a chance to look at application state
			System.Diagnostics.Debugger.Break()

			[Exit]()
		End Sub

		Public Shared Sub LogException(ByVal e As Exception)
			LogRaw("### ERROR ==> ", e.ToString())
		End Sub

		Public Shared Sub LogMessage(ByVal inMessage As String)
			LogRaw("# INFO ", inMessage)
		End Sub

		Public Shared Sub LogMessageModal(ByVal inMessage As String)
			LogRaw("# INFO ", inMessage)

			VisualUtilities.ShowMessage(inMessage)
		End Sub

		Private Shared Sub LogRaw(ByVal inPrefix As String, ByVal inMessage As String)
			SyncLock m_Lock
				Dim message As String = inPrefix & "[" & Date.Now.ToString("dd.MM.yyyy HH:mm:ss") & "]: " & inMessage & Environment.NewLine

				Dim Writer As StreamWriter = File.AppendText("./log.txt")

				Using Writer
					Writer.Write(message)

					Writer.Flush()
				End Using
			End SyncLock
		End Sub

		Public Shared Sub [Exit]()
			System.Diagnostics.Process.GetCurrentProcess().Kill()
		End Sub
	End Class
End Namespace
