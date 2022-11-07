Namespace Migration.Configuration
	Public Class ToolConfiguration
		Private m_Percentage As Double
		Private m_Todo As Integer
		Private m_Config As GameConfiguration

		Public Property Todo() As Integer
			Get
				Return m_Todo
			End Get
			Friend Set(ByVal value As Integer)
				m_Todo = Math.Max(0, Math.Min(8, value))
			End Set
		End Property
		Public Property Percentage() As Double
			Get
				Return m_Percentage
			End Get
			Friend Set(ByVal value As Double)
				m_Percentage = Math.Max(0, Math.Min(1.0, value))
			End Set
		End Property

		Friend Sub New(ByVal inConfig As GameConfiguration)
			If inConfig Is Nothing Then
				Throw New ArgumentNullException()
			End If

			m_Config = inConfig
		End Sub
	End Class
End Namespace
