Imports System.IO

Namespace Migration
	Friend Class DeferredContainer

		Private privateFrames As UniqueMap(Of Int64, Byte())
		Public Property Frames() As UniqueMap(Of Int64, Byte())
			Get
				Return privateFrames
			End Get
			Private Set(ByVal value As UniqueMap(Of Int64, Byte()))
				privateFrames = value
			End Set
		End Property

		Public Sub New()
			Frames = New UniqueMap(Of Int64, Byte())()
		End Sub

		Public Sub AddFrame(ByVal inFrame As AnimationFrame)
			If inFrame.m_Bitmap Is Nothing Then
				Throw New ArgumentNullException("Given animation frame has no valid bitmap!")
			End If

			If Not(Frames.ContainsKey(inFrame.Checksum)) Then
				Frames.Add(inFrame.Checksum, inFrame.m_Bitmap)
			End If
		End Sub

		Public Shared Function Load(ByVal inSource As Stream) As DeferredContainer
			Dim result As New DeferredContainer()
			Dim reader As New BinaryReader(inSource)
			Dim frameCount As Int32 = reader.ReadInt32()

			For i As Integer = 0 To frameCount - 1
				Dim checksum As Int64 = reader.ReadInt64()
				Dim data() As Byte = reader.ReadBytes(reader.ReadInt32())

				result.Frames.Add(checksum, data)
			Next i

			Return result
		End Function

		Public Sub Store(ByVal inTarget As Stream)
			Dim writer As New BinaryWriter(inTarget)

			writer.Write(Convert.ToInt32(CInt(Frames.Count)))

			For Each frame As KeyValuePair(Of Int64, Byte()) In Frames
				writer.Write(Convert.ToInt64((frame.Key)))
				writer.Write(Convert.ToInt32(CInt(frame.Value.Length)))
				writer.Write(frame.Value)
			Next frame

			inTarget.Flush()
		End Sub
	End Class
End Namespace
