Imports System.ComponentModel
Imports System.IO
Imports Migration.Core

Namespace Migration
	''' <summary>
	''' An animation set is able to play several animations simultaneously.
	''' </summary>
	Public Class AnimationSet
		Private m_Animations As New UniqueMap(Of String, Animation)()
		Private m_DurationMillis As Int64

		Private privateCharacter As Character
		Public Property Character() As Character
			Get
				Return privateCharacter
			End Get
			Private Set(ByVal value As Character)
				privateCharacter = value
			End Set
		End Property

		Private privateName As String
		Public Property Name() As String
			Get
				Return privateName
			End Get
			Friend Set(ByVal value As String)
				privateName = value
			End Set
		End Property

		Private privateIndex As Int32
		Public Property Index() As Int32
			Get
				Return privateIndex
			End Get
			Friend Set(ByVal value As Int32)
				privateIndex = value
			End Set
		End Property

		Private privateRenderIndex As Int32
		Public Property RenderIndex() As Int32
			Get
				Return privateRenderIndex
			End Get
			Set(ByVal value As Int32)
				privateRenderIndex = value
			End Set
		End Property

		Public Property DurationMillis() As Int64
			Get
				Return m_DurationMillis
			End Get
			Set(ByVal value As Int64)
				ForceWriteable()

				m_DurationMillis = value
				DurationMillisBounded = ((DurationMillis + Convert.ToInt64((CyclePoint.CYCLE_MILLIS)) - 1) \ Convert.ToInt64((CyclePoint.CYCLE_MILLIS))) * Convert.ToInt64((CyclePoint.CYCLE_MILLIS))
			End Set
		End Property

		Private privateDurationMillisBounded As Int64
		Public Property DurationMillisBounded() As Int64
			Get
				Return privateDurationMillisBounded
			End Get
			Private Set(ByVal value As Int64)
				privateDurationMillisBounded = value
			End Set
		End Property

		Public ReadOnly Property Animations() As BindingList(Of Animation)
			Get
				Return m_Animations.GetValueBinding()
			End Get
		End Property

		Public ReadOnly Property Children() As BindingList(Of Animation)
			Get
				Return Animations
			End Get
		End Property

		Public ReadOnly Property Library() As AnimationLibrary
			Get
				Return Character.Library
			End Get
		End Property

		Private privateWidth As Int32
		Public Property Width() As Int32
			Get
				Return privateWidth
			End Get
			Private Set(ByVal value As Int32)
				privateWidth = value
			End Set
		End Property

		Private privateHeight As Int32
		Public Property Height() As Int32
			Get
				Return privateHeight
			End Get
			Private Set(ByVal value As Int32)
				privateHeight = value
			End Set
		End Property

		Public Sub Save(ByVal inWriter As BinaryWriter)
			' write set to stream
			inWriter.Write(Convert.ToByte(3)) ' set type ID
			inWriter.Write(Convert.ToUInt16(&H1000)) ' set version

			inWriter.Write(Convert.ToString(Name))
			inWriter.Write(RenderIndex)
			inWriter.Write(Convert.ToInt64((DurationMillis)))
			inWriter.Write(Convert.ToInt32(CInt(m_Animations.Count)))

			For Each anim As Animation In m_Animations.Values
				anim.Save(inWriter)
			Next anim
		End Sub

		Public Shared Function Load(ByVal inClass As Character, ByVal inReader As BinaryReader) As AnimationSet
			Dim result As AnimationSet = Nothing

			If inReader.ReadByte() <> 3 Then
				Throw New InvalidDataException()
			End If

			Select Case inReader.ReadUInt16()
				Case &H1000
					result = New AnimationSet(inReader.ReadString(), inClass)
					result.RenderIndex = inReader.ReadInt32()
					result.DurationMillis = inReader.ReadInt64()

					Dim i As Integer = 0
					Dim count As Integer = inReader.ReadInt32()

					Do While i < count
						Dim anim As Animation = Animation.Load(inClass.Library, result, inReader)

						result.m_Animations.Add(anim.Name, anim)
						i += 1
					Loop

				Case Else
					Throw New InvalidDataException()
			End Select

			For Each anim As Animation In result.m_Animations.Values
				AddHandler anim.OnDimensionChanged, AddressOf result.ComputeDimension
			Next anim

			Return result
		End Function


		Friend Sub New(ByVal inName As String, ByVal inClass As Character)
			Name = inName
			Character = inClass
		End Sub

		Public Sub ComputeDimension()
			ComputeDimension(True)
		End Sub

		Public Sub ComputeDimension(ByVal inNotifyParent As Boolean)
			ForceWriteable()

			Dim newWidth As Int32 = 0
			Dim newHeight As Int32 = 0

			For Each anim As Animation In m_Animations.Values
				newWidth = Math.Max(newWidth, anim.OffsetX + anim.Width)
				newHeight = Math.Max(newHeight, anim.OffsetY + anim.Height)
			Next anim

			Width = newWidth
			Height = newHeight

			If inNotifyParent Then
				Character.ComputeDimension()
			End If
		End Sub

		Public Sub Rename(ByVal inAnimation As Animation, ByVal inNewName As String)
			ForceWriteable()

			Library.ValidateName(inNewName)

			If m_Animations.ContainsKey(inNewName) Then
				Throw New ArgumentException("An animation named """ & inNewName & """ does already exist!")
			End If

			Dim pos As Integer = 0

			pos = m_Animations.Values.IndexOf(inAnimation)
			If pos < 0 Then
				Throw New ApplicationException("Animation does not belong to this set.")
			End If

			m_Animations.Remove(inAnimation.Name)
			inAnimation.Name = inNewName
			m_Animations.Add(inAnimation.Name, inAnimation)
		End Sub

		Public Function FindAnimation(ByVal inName As String) As Animation
			Return m_Animations(inName)
		End Function

		Public Function HasAnimation(ByVal inName As String) As Boolean
			Return m_Animations.ContainsKey(inName)
		End Function

		Public Function AddAnimation(ByVal inName As String) As Animation
			ForceWriteable()

			Dim result As New Animation(inName, Me)

			Library.ValidateName(inName)

			If m_Animations.ContainsKey(inName) Then
				Throw New ArgumentException("An animation set named """ & inName & """ does already exist!")
			End If

			m_Animations.Add(inName, result)

			AddHandler result.OnDimensionChanged, AddressOf ComputeDimension
			ComputeDimension()

			Return result
		End Function

		Public Sub RemoveAnimation(ByVal inAnimation As Animation)
			ForceWriteable()

			If m_Animations.Remove(inAnimation.Name) Then
				RemoveHandler inAnimation.OnDimensionChanged, AddressOf ComputeDimension

				ComputeDimension()
			End If
		End Sub

		Private Sub ForceWriteable()
			Library.ForceWriteable()
		End Sub
	End Class
End Namespace
