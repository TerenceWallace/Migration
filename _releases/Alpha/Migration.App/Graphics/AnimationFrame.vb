Imports System.IO

Namespace Migration
	<Serializable> _
	Public Class AnimationFrame

		Private m_AnimationOrNull As Animation
		Friend m_Bitmap() As Byte
		Private m_OffsetX As Integer
		Private m_OffsetY As Integer
		Private m_Width As Integer
		Private m_Height As Integer

		<NonSerialized()> _
		Private m_Source As Bitmap

		Private privateGFXFileChecksum As Int32
		Public Property GFXFileChecksum() As Int32
			Get
				Return privateGFXFileChecksum
			End Get
			Set(ByVal value As Int32)
				privateGFXFileChecksum = value
			End Set
		End Property

		Private privateGFXFrame As Int32
		Public Property GFXFrame() As Int32
			Get
				Return privateGFXFrame
			End Get
			Set(ByVal value As Int32)
				privateGFXFrame = value
			End Set
		End Property

		Private privateGFXSequence As Int32
		Public Property GFXSequence() As Int32
			Get
				Return privateGFXSequence
			End Get
			Set(ByVal value As Int32)
				privateGFXSequence = value
			End Set
		End Property

		Private privateOriginalOffsetX As Int32
		Public Property OriginalOffsetX() As Int32
			Get
				Return privateOriginalOffsetX
			End Get
			Set(ByVal value As Int32)
				privateOriginalOffsetX = value
			End Set
		End Property

		Private privateOriginalOffsetY As Int32
		Public Property OriginalOffsetY() As Int32
			Get
				Return privateOriginalOffsetY
			End Get
			Set(ByVal value As Int32)
				privateOriginalOffsetY = value
			End Set
		End Property

		Private privateChecksum As Int64
		Public Property Checksum() As Int64
			Get
				Return privateChecksum
			End Get
			Friend Set(ByVal value As Int64)
				privateChecksum = value
			End Set
		End Property

		Public Property OffsetX() As Integer
			Get
				Return m_OffsetX
			End Get
			Set(ByVal value As Integer)
				ForceWriteable()
				m_OffsetX = value
			End Set
		End Property

		Public Property OffsetY() As Integer
			Get
				Return m_OffsetY
			End Get
			Set(ByVal value As Integer)
				ForceWriteable()
				m_OffsetY = value
			End Set
		End Property

		Public Property Width() As Integer
			Get
				Return m_Width
			End Get
			Set(ByVal value As Integer)
				ForceWriteable()
				m_Width = value
			End Set
		End Property

		Public Property Height() As Integer
			Get
				Return m_Height
			End Get
			Set(ByVal value As Integer)
				ForceWriteable()
				m_Height = value
			End Set
		End Property

		Private privateIndex As Integer
		Public Property Index() As Integer
			Get
				Return privateIndex
			End Get
			Friend Set(ByVal value As Integer)
				privateIndex = value
			End Set
		End Property

		Public ReadOnly Property AnimationOrNull() As Animation
			Get
				Return m_AnimationOrNull
			End Get
		End Property

		Friend Sub Save(ByVal inWriter As BinaryWriter)
			' write frame to stream
			inWriter.Write(Convert.ToByte(5)) ' frame type ID
			inWriter.Write(Convert.ToUInt16(&H1000)) ' frame version

			inWriter.Write(Width)
			inWriter.Write(Checksum)
			inWriter.Write(OffsetX)
			inWriter.Write(OffsetY)
			inWriter.Write(GFXFileChecksum)
			inWriter.Write(GFXFrame)
			inWriter.Write(GFXSequence)
			inWriter.Write(Height)
		End Sub

		Friend Shared Function Load(ByVal inAnim As Animation, ByVal inReader As BinaryReader) As AnimationFrame
			Dim result As New AnimationFrame(inAnim)

			If inReader.ReadByte() <> 5 Then
				Throw New InvalidDataException()
			End If

			Select Case inReader.ReadUInt16()
				Case &H1000
					result.Width = inReader.ReadInt32()
					result.Checksum = inReader.ReadInt64()
					result.OffsetX = inReader.ReadInt32()
					result.OffsetY = inReader.ReadInt32()
					result.GFXFileChecksum = inReader.ReadInt32()
					result.GFXFrame = inReader.ReadInt32()
					result.GFXSequence = inReader.ReadInt32()
					result.Height = inReader.ReadInt32()

				Case Else
					Throw New InvalidDataException()
			End Select

			Return result
		End Function

		Public Function ToArray() As Byte()
			If Source Is Nothing Then
				Return Nothing
			End If

			Return CType(m_Bitmap.Clone(), Byte())
		End Function

		Friend Sub New(ByVal inParent As Animation)
			m_AnimationOrNull = inParent
			OriginalOffsetY = Int32.MinValue
			OriginalOffsetX = OriginalOffsetY
		End Sub

		Public Sub SetBitmap(ByVal inBitmap As System.Drawing.Bitmap)
			ForceWriteable()

			Dim stream As New MemoryStream()

			inBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png)

			SetBitmap(stream.ToArray())
		End Sub

		Public Sub SetBitmap(ByVal inSource() As Byte)
			ForceWriteable()
			Try
				m_Bitmap = CType(inSource.Clone(), Byte())
				m_Source = CType(Bitmap.FromStream(New MemoryStream(m_Bitmap)), Bitmap)
				Dim hash() As Byte = System.Security.Cryptography.MD5.Create().ComputeHash(New MemoryStream(inSource))

				Checksum = 0

				For i As Integer = 0 To 7
					Checksum = Checksum Or ((Convert.ToInt64(CInt(hash(i)))) << (i * 8))
				Next i
			Catch ex As Exception
				m_Bitmap = Nothing
				m_Source = Nothing
				Checksum = 0
			End Try
		End Sub

		Public ReadOnly Property Source() As Bitmap
			Get

				If m_Bitmap Is Nothing Then
					If AnimationOrNull Is Nothing Then
						Return Nothing
					End If

					If AnimationOrNull.SetOrNull IsNot Nothing Then
						Dim Character As Character = AnimationOrNull.SetOrNull.Character

						m_Bitmap = Character.LoadFrame(Me)
					Else
						m_Bitmap = AnimationOrNull.Library.LoadFrame(Me)
					End If

					' m_Bitmap is now set, otherwise an exception is thrown by LoadFrame().
				End If


				If m_Source Is Nothing Then
					m_Source = CType(Bitmap.FromStream(New MemoryStream(m_Bitmap)), Bitmap)
					m_Source = m_Source
				End If

				Return m_Source
			End Get
		End Property

		Public Sub Clone(ByVal outClone As AnimationFrame)
			outClone.SetBitmap(Source)
			outClone.Height = Height
			outClone.GFXFileChecksum = GFXFileChecksum
			outClone.GFXFrame = GFXFrame
			outClone.GFXSequence = GFXSequence
			outClone.OffsetX = OffsetX
			outClone.OffsetY = OffsetY
			outClone.Width = Width
		End Sub

		Private Sub ForceWriteable()
			If AnimationOrNull IsNot Nothing Then
				AnimationOrNull.Library.ForceWriteable()
			End If
		End Sub
	End Class
End Namespace
