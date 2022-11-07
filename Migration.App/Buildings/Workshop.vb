Imports Migration.Common
Imports Migration.Configuration

Namespace Migration.Buildings
	Public Class Workshop
		Inherits Business

		''' <summary>
		''' Is called whenever a production cycle for a <see cref="Resource.Max"/> provider
		''' is completed. Please be very careful, since this event is raised (for sake of synchronization)
		''' within the main simulation loop and should be completed within microsecond orders of magnitudes!
		''' </summary>
		Friend Event OnProducedCustomItem As DNotifyHandler(Of Workshop)

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			MyBase.New(inParent, inConfig, inPosition)
		End Sub

		Protected Class ProviderSelection
			Private privateProvider As GenericResourceStack
			Friend Property Provider() As GenericResourceStack
				Get
					Return privateProvider
				End Get
				Set(ByVal value As GenericResourceStack)
					privateProvider = value
				End Set
			End Property
			Private privateOnProduced As Procedure
			Friend Property OnProduced() As Procedure
				Get
					Return privateOnProduced
				End Get
				Set(ByVal value As Procedure)
					privateOnProduced = value
				End Set
			End Property
		End Class

		''' <summary>
		''' This method may be overwritten to weight production probabilities according to
		''' user configuration (weapon and tool smith).
		''' </summary>
		Protected Overridable Function SelectProvider() As ProviderSelection
			' randomly select a provider and complete production
			Dim trials As Integer = 0
			Dim prov As GenericResourceStack = Nothing

			Do
				prov = Providers(m_Random.Next(0, ProviderConfigs.Count))
				trials += 1

				If trials > 8 Then
					Return New ProviderSelection()
				End If

				Loop While Not prov.HasSpace

			Dim mProvider As New ProviderSelection()
			mProvider.Provider = prov

			Return mProvider
		End Function

		''' <summary>
		''' Is not to be called every cycle, since it is an expensive function.
		''' </summary>
		Friend Overrides Function Update() As Boolean
			If Not(MyBase.Update()) Then
				Return False
			End If

			' already producing?
			Dim currentTime As Long = Parent.MoveManager.CurrentCycle
			Dim timeOffset As Long = (currentTime - ProductionStart)

			If timeOffset >= ProductionTime Then
				' ### START NEW PRODUCTION CYCLE

				' are there missing resources for next production cycle?
				If UseQualityIndex Then
					If Not(Queries.Any(AddressOf IsMissing)) Then
						Return False
					End If
				Else
					If Not(Queries.All(AddressOf IsMissing)) Then
						Return False
					End If
				End If

				' has space for outcomes?
				If Not(HasFreeProvider()) Then
					Return False
				End If

				' can select provider?
				If SelectProvider().Provider Is Nothing Then
					Return False
				End If

				timeOffset = 0
				ProductionStart = currentTime

				' We will directly enter the new production cycle...
			End If


			If timeOffset < ProductionTime Then
				' ### UPDATE ONGOING PRODUCTION CYCLE

				' update queries
				For i As Integer = 0 To Queries.Count - 1
					Dim cfg As ResourceStack = QueriesConfig(i)
					Dim query As GenericResourceStack = Queries(i)

					If timeOffset <> cfg.TimeOffset Then
						Continue For
					End If
					'                    
					'                     * When there is set a CycleCount greater one in resource stack config,
					'                     * one decrease in stack size will last for CycleCount production cycles
					'                     * without again reducing the stack within that period.
					'                     
					If QueriesPreload(i) <= 0 Then
						'                         
						'                         * Is stack still available, since a thief might have stolen it?
						'                         
						If query.Available = 0 Then
							'                            
							'                             * In case resources are not available during production (stolen)
							'                             * currently we just ignore it...
							'                             
							Continue For
						End If

						query.RemoveResource()

						QueriesPreload(i) += Math.Max(0, cfg.CycleCount - 1)
					Else
						' still allowed to use resource from previous cycle (for example fish or meat)
						QueriesPreload(i) -= 1
					End If

					If UseQualityIndex Then
						Exit For
					End If
				Next i

				' update providers
				If Providers.Count > 0 Then
					If timeOffset = Config.ProductionAnimTime Then

						' by assumption all providers are expected to have the same time offset
						'if (ProviderConfigs[0].TimeOffset == timeOffset)
						' has production animation?
						animName = Nothing
						Dim dueTimeMillis As Int32 = 0

						If (animName Is Nothing) AndAlso VisualUtilities.HasAnimation(Me, "Produce") Then
							animName = "Produce"
						End If
						If (animName Is Nothing) AndAlso VisualUtilities.HasAnimation(Me, "Produce0") Then
							animName = "Produce0"
						ElseIf (animName Is Nothing) AndAlso VisualUtilities.HasAnimation(Me, "Succeeded") Then
							animName = "Succeeded"
						End If

						If animName IsNot Nothing Then
							VisualUtilities.Animate(Me, animName)
							dueTimeMillis = VisualUtilities.GetDurationMillis(Me, animName)

							If animName = "Produce0" Then
								VisualUtilities.SynchronizeTask(dueTimeMillis, AddressOf AnimateProduction)

								dueTimeMillis += VisualUtilities.GetDurationMillis(Me, "Produce1")
							End If
						End If

						' finally add resource to choosen provider
						Parent.QueueWorkItem(dueTimeMillis, AddressOf AnimateProvider)
					End If
				End If
			End If

			Return True
		End Function

		Private Function IsMissing(ByVal e As GenericResourceStack) As Boolean
			If e Is Nothing Then
				Return False
			Else
				Return e.Available > 0
			End If
		End Function

		Private Sub AnimateProduction()
			VisualUtilities.Animate(Me, "Produce1")
		End Sub

		Private animName As String = String.Empty
		Private Sub AnimateProvider()
			If animName IsNot Nothing Then
				VisualUtilities.Animate(Me, Nothing)
			End If
			Dim prov As GenericResourceStack = Providers(0)
			If Providers.Count > 1 Then
				Dim selection As ProviderSelection = SelectProvider()
				prov = selection.Provider
				If selection.OnProduced IsNot Nothing Then
					selection.OnProduced.Invoke()
				End If
			End If
			If prov IsNot Nothing Then
				If prov.HasSpace Then
					prov.AddResource()
				End If
			Else
				RaiseEvent OnProducedCustomItem(Me)
			End If

		End Sub
	End Class
End Namespace
