Imports Migration.Common
Imports Migration.Core
Imports Migration.Interfaces

#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering
	''' <summary>
	''' Renders a whole Character.
	''' </summary>
	Public Class RenderableCharacter
		Inherits RenderableVisual

		Private Shared s_FrameCache As New List(Of NativeTexture)()

		Private ReadOnly m_PlayingSets As New List(Of RuntimeAnimSet)()
		Private m_AnimScale As Double
		Private m_ResetTime As Boolean = False

		Private privateMilliDiff As Int64
		Public Property MilliDiff() As Int64
			Get
				Return privateMilliDiff
			End Get
			Private Set(ByVal value As Int64)
				privateMilliDiff = value
			End Set
		End Property

		Private privateStartTime As Int64
		Public Property StartTime() As Int64
			Get
				Return privateStartTime
			End Get
			Private Set(ByVal value As Int64)
				privateStartTime = value
			End Set
		End Property

		Private privateClass As Character
		Public Property Character() As Character
			Get
				Return privateClass
			End Get
			Private Set(ByVal value As Character)
				privateClass = value
			End Set
		End Property

		Public ReadOnly Property PlayingSets() As IEnumerable(Of AnimationSet)
			Get
				Return m_PlayingSets.Select(Function(e As RuntimeAnimSet) e.Set)
			End Get
		End Property

		Private privateFrozenFrameIndex As Int32
		Public Property FrozenFrameIndex() As Int32
			Get
				Return privateFrozenFrameIndex
			End Get
			Set(ByVal value As Int32)
				privateFrozenFrameIndex = value
			End Set
		End Property

		Private privateIsRepeated As Boolean
		Public Property IsRepeated() As Boolean
			Get
				Return privateIsRepeated
			End Get
			Set(ByVal value As Boolean)
				privateIsRepeated = value
			End Set
		End Property

		Public Sub New(ByVal inRenderer As TerrainRenderer, ByVal inCharacter As Character, ByVal inInitialPosition As CyclePoint)
			MyBase.New(inRenderer, inInitialPosition)
			Character = inCharacter
			m_AnimScale = 1 / 20.0
			AspectRatio = (Character.Width / Convert.ToDouble(Character.Height))
			IsRepeated = True

			SetupPositionShift()

			Play()
		End Sub

		Public Sub New(ByVal inRenderer As TerrainRenderer, ByVal inCharacter As Character, ByVal inTracker As IPositionTracker)
			MyBase.New(inRenderer, inTracker.Position)
			Character = inCharacter
			m_AnimScale = 1 / 20.0
			AspectRatio = (Character.Width / Convert.ToDouble(Character.Height))
			IsRepeated = True

			AddHandler inTracker.OnPositionChanged, AddressOf OnChangePosition

			SetupPositionShift()

			Play()
		End Sub

		Private Sub OnChangePosition(ByVal unused As IPositionTracker, ByVal olsPos As CyclePoint, ByVal newPos As CyclePoint)
			Position = newPos
		End Sub

		Private Sub SetupPositionShift()
			If (Character.GroundPlane.Count = 1) AndAlso ((Character.ShiftX <> 0) OrElse (Character.ShiftY <> 0)) AndAlso (AnimationLibrary.Instance.IsReadonly OrElse AnimationLibrary.Instance.ForcePositionShift) Then
				'                 
				'                 * Adjust animation shift to snap the ground plane point directly to its IPositionTracker. This
				'                 * will cause movables and foilage to be where it ought to be, instead of having its snap point
				'                 * at the top-left, which is precisly always wrong for those object types!
				'                 
				PositionShiftX = Convert.ToSByte(-Character.GroundPlane(0).X)
				PositionShiftY = Convert.ToSByte(-Character.GroundPlane(0).Y)
			End If
		End Sub

		Public Sub Play()
			Play(New String(){})
		End Sub

		Public Sub Play(ParamArray ByVal inAnimSets() As String)
			Dim sets(inAnimSets.Length - 1) As AnimationSet

			For i As Integer = 0 To inAnimSets.Length - 1
				sets(i) = Character.FindSet(inAnimSets(i))
			Next i

			Play(sets)
		End Sub

		Public Sub Pause()
		End Sub

		Public Sub Play(ParamArray ByVal inAnimationSet() As AnimationSet)
			SyncLock m_PlayingSets

				If Character.Name = "WoodCutter" Then
					Me.Stop()
				End If

				Me.Stop()

				For Each mSet As AnimationSet In inAnimationSet.OrderBy(Function(e) e.RenderIndex)
#If DEBUG Then
					If Not(Character.Sets.Contains(mSet)) Then
						Throw New ArgumentException("At least one of the given sets is not a member of this class.")
					End If
#End If

					m_PlayingSets.Add(New RuntimeAnimSet(mSet))
				Next mSet
			End SyncLock
		End Sub

		Public Sub [Stop]()
			SyncLock m_PlayingSets
				m_PlayingSets.Clear()

				If Character.AmbientSet IsNot Nothing Then
					m_PlayingSets.Add(New RuntimeAnimSet(Character.AmbientSet))
				End If
			End SyncLock
		End Sub

		Public Overrides Sub SetAnimationTime(ByVal inTime As Int64)
			If m_ResetTime Then
				m_ResetTime = False
				StartTime = inTime
			End If

			MilliDiff = Math.Abs(inTime - StartTime)
		End Sub

		Public Sub ResetTime()
			m_ResetTime = True
		End Sub

		Friend Overrides Sub Render(ByVal inPass As RenderPass)
			'            
			'             * Rendering is now done for all current frames with the same Z-Value.
			'             * This implies that the order of sets and animations within the animation
			'             * class has a direct impact on rendering (blending) order.
			'             
			For Each mSet As RuntimeAnimSet In m_PlayingSets
				Dim duration As Long = Math.Max(1, mSet.Duration)

				For Each anim As Animation In mSet.Animations
					If anim Is Nothing Then
						Continue For
					End If

					' select frame
					Dim frameIndex As Int32 = 0

					If anim.IsFrozen Then
						'                        
						' * There shall only be one animation flagged frozen, but either way we
						' * make sure that index is within bounds
						'                         
						frameIndex = FrozenFrameIndex Mod anim.Frames.Count
					Else
						If (MilliDiff >= duration) AndAlso ((Not IsRepeated)) Then
							frameIndex = anim.Frames.Count - 1
						Else
							frameIndex = Convert.ToInt32(CInt(Fix((MilliDiff Mod duration) * (anim.Frames.Count / Convert.ToDouble(duration)))))
						End If
					End If

					If anim.Frames.Count = 0 Then
						Continue For
					End If

					If frameIndex < 0 Then
						frameIndex = 0
					End If
					If frameIndex >= anim.Frames.Count Then
						frameIndex = anim.Frames.Count - 1
					End If
					Dim frame As AnimationFrame = anim.Frames(frameIndex)

					If Renderer.SpriteEngine Is Nothing Then
						Dim frameCount As Integer = s_FrameCache.Count

						' this fallback is primarily intended for the AnimationEditor
						Do While frame.Index >= frameCount
							s_FrameCache.Add(Nothing)
							frameCount += 1
						Loop

						' apply texture
						Dim entry As NativeTexture = s_FrameCache(frame.Index)

						If entry Is Nothing Then
							s_FrameCache(frame.Index) = New NativeTexture(frame.Source)
							entry = s_FrameCache(frame.Index)
						End If

						Texture = entry
					End If

					' render frame
					MyBase.Render(inPass, frame.Index, (anim.OffsetX + frame.OffsetX + anim.SetOrNull.Character.ShiftX) * m_AnimScale, (anim.OffsetY + frame.OffsetY + anim.SetOrNull.Character.ShiftY) * m_AnimScale, frame.Width * m_AnimScale, frame.Height * m_AnimScale)
				Next anim
			Next mSet
		End Sub
	End Class
End Namespace
