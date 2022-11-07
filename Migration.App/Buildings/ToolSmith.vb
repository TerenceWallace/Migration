Imports Migration.Configuration
Imports Migration.Core

Namespace Migration.Buildings
	Public Class ToolSmith
		Inherits Workshop

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			MyBase.New(inParent, inConfig, inPosition)
		End Sub

		Protected Overrides Function SelectProvider() As ProviderSelection
			' process todos first
			For Each prov As GenericResourceStack In MyBase.Providers

				Dim  m_config As ToolConfiguration = MyBase.Parent.Map.Configuration.Tools(prov.Resource)

				If ( m_config.Todo > 0) AndAlso prov.HasSpace Then
					Dim mProviderSelection1 As New ProviderSelection()
					mProviderSelection1.Provider = prov
					mProviderSelection1.OnProduced = Sub()  m_config.Todo -= 1
					Return mProviderSelection1
				End If
			Next prov

			Dim totalProb As Double = 0
			Dim result As GenericResourceStack = Nothing

			For Each prov As GenericResourceStack In MyBase.Providers
				If prov.HasSpace Then
					totalProb = (totalProb + MyBase.Parent.Map.Configuration.Tools(prov.Resource).Percentage)
				End If
			Next prov
			If totalProb > 0 Then
				Dim val As Double = (BaseBuilding.m_Random.NextDouble() * totalProb)
				Dim lastProb As Double = 0
				totalProb = 0

				'				GenericResourceStack prov = null;
				For Each prov As GenericResourceStack In MyBase.Providers
					If prov.HasSpace Then
						lastProb = totalProb
						totalProb = (totalProb + MyBase.Parent.Map.Configuration.Tools(prov.Resource).Percentage)
						If (lastProb <= val) AndAlso (val <= totalProb) Then
							result = prov
							Exit For
						End If
					End If
				Next prov
			End If

			Dim mProviderSelection As New ProviderSelection()
			mProviderSelection.Provider = result
			Return mProviderSelection

		End Function
	End Class
End Namespace
