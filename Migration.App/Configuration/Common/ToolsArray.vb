Imports Migration.Common

Namespace Migration.Configuration
	Public Class ToolsArray

		Private ReadOnly m_Tools(CInt(Resource.Max) - 1) As ToolConfiguration

		Friend Sub New(ByVal inConfig As GameConfiguration)
			For i As Integer = 0 To m_Tools.Length - 1

				Dim mResource As Resource = CType(i, Resource)
				Select Case mResource
					Case Resource.Axe, Resource.Bow, Resource.Hammer, Resource.Hook, Resource.PickAxe, Resource.Saw, Resource.Scythe, Resource.Shovel, Resource.Spear, Resource.Sword
					Case Else
						Continue For
				End Select

				m_Tools(i) = New ToolConfiguration(inConfig)
			Next i
		End Sub

		Default Public ReadOnly Property Item(ByVal tool As Resource) As ToolConfiguration
			Get
				Dim res As ToolConfiguration = m_Tools(Convert.ToInt32(tool))

				If res Is Nothing Then
					Throw New ArgumentOutOfRangeException("The given resource is not a tool!")
				End If

				Return res
			End Get
		End Property
	End Class
End Namespace
