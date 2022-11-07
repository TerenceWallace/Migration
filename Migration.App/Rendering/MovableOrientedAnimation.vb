Imports Migration.Common
Imports Migration.Interfaces
Imports Migration.Jobs

Namespace Migration.Rendering
	Public Class MovableOrientedAnimation
		Inherits RenderableCharacter

		Private Shared ReadOnly m_ConstructorLock As New Object()
		Private Shared ReadOnly m_DirToAnim As New SortedDictionary(Of String, DirectionCache)()

		Private m_Direction As Direction
		Private m_PlayingDirection As Direction
		Private m_IsMoving As Boolean
		Private m_HasJob As Boolean
		Private m_DirectionCache As DirectionCache

		Public Event OnDirectionChanged As DChangeHandler(Of IPositionTracker, Direction)
		Private privateMovable As Movable
		Public Property Movable() As Movable
			Get
				Return privateMovable
			End Get
			Private Set(ByVal value As Movable)
				privateMovable = value
			End Set
		End Property

		Private privateDirectionOverride? As Direction
		Public Property DirectionOverride() As Direction?
			Get
				Return privateDirectionOverride
			End Get
			Set(ByVal value? As Direction)
				privateDirectionOverride = value
			End Set
		End Property

		Public Property Direction() As Direction
			Get
				Return m_Direction
			End Get
			Set(ByVal value As Direction)
				Dim old As Direction = m_Direction

				m_Direction = value

				RaiseEvent OnDirectionChanged(Me, old, value)
			End Set
		End Property

		Public Sub New(ByVal inParent As TerrainRenderer, ByVal inCharacter As Character, ByVal inMovable As Movable)
			MyBase.New(inParent, inCharacter, inMovable.Position)
			If inMovable Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Movable = inMovable

			SyncLock m_ConstructorLock
				If Not(m_DirToAnim.TryGetValue(inCharacter.Name, m_DirectionCache)) Then
					m_DirectionCache = New DirectionCache() With {.DirectionToFrozen = New AnimationSet(CInt(Direction.Count) - 1){}, .DirectionToAnimation = New AnimationSet(CInt(Direction.Count) - 1){}}

                    If inCharacter.Sets.AsEnumerable.Count(Function(e) e.Name = "Default") > 0 Then
                        Dim animSet As AnimationSet = inCharacter.FindSet("Default")

                        For i As Integer = 0 To m_DirectionCache.DirectionToAnimation.Length - 1
                            m_DirectionCache.DirectionToAnimation(i) = animSet
                        Next i
                    Else
                        For i As Integer = 0 To m_DirectionCache.DirectionToAnimation.Length - 1
                            Dim mDirection As Direction = CType(i, Direction)
                            Dim Title As String = mDirection.ToString()

                            m_DirectionCache.DirectionToAnimation(i) = inCharacter.FindSet("Angle" & Title)
                            m_DirectionCache.DirectionToFrozen(i) = inCharacter.FindSet("FrozenAngle" & Title)
                        Next i
                    End If

					m_DirToAnim.Add(inCharacter.Name, m_DirectionCache)
				End If
			End SyncLock

			m_Direction = Direction._225
			m_PlayingDirection = m_Direction
			PlayFrozen()
		End Sub

		Private Sub PlayFrozen()
			m_IsMoving = False
			m_HasJob = False

			If m_DirectionCache.DirectionToFrozen(CInt(m_PlayingDirection)) IsNot Nothing Then
				Play(m_DirectionCache.DirectionToFrozen(CInt(m_PlayingDirection)))
			Else
				Play(m_DirectionCache.DirectionToAnimation(CInt(m_PlayingDirection)))
				Pause()
			End If
		End Sub

		Friend Overrides Sub Render(ByVal inPass As RenderPass)
			If inPass = RenderPass.Pass1_Shadow Then
				'                
				'                 * For performance reason this branch is based on the assumption that the first
				'                 * thing in the rendering pipeline is calculating the Z-Values... So for the
				'                 * additional two calls there is no need to do this caluclation again!
				'                 

				If Movable IsNot Nothing Then
					Dim newDirection As Direction = Direction
					Dim newPosition As New CyclePoint()
					Dim newZShift As Double = 0

					If TypeOf Movable.Job Is JobOrder Then
						Movable = Movable
					End If

					Movable.InterpolateMovablePosition(newDirection, newPosition, newZShift)

					If newDirection <> Direction Then
						Direction = newDirection
					End If

					ZShiftOverride = newZShift
					Position = newPosition
				End If
			End If

			If (Movable Is Nothing) OrElse Movable.IsMoving OrElse Movable.HasJob Then
				If (DirectionOverride IsNot Nothing) AndAlso DirectionOverride.HasValue Then
					Direction = DirectionOverride.Value
				End If

				If (m_PlayingDirection <> Direction) OrElse Not(m_IsMoving OrElse m_HasJob) Then
					m_IsMoving = Movable.IsMoving
					m_HasJob = Movable.HasJob
					m_PlayingDirection = Direction

					Play(m_DirectionCache.DirectionToAnimation(CInt(m_PlayingDirection)))
				End If
			Else
				If m_IsMoving OrElse m_HasJob Then
					PlayFrozen()
				End If
			End If

			MyBase.Render(inPass)
		End Sub

	End Class
End Namespace
