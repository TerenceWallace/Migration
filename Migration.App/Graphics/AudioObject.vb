Imports System.IO
Imports Migration.Common
Imports Migration.Interfaces

Namespace Migration


	Public Class AudioObject
		Private privateChecksum As Int64
		Public Property Checksum() As Int64
			Get
				Return privateChecksum
			End Get
			Private Set(ByVal value As Int64)
				privateChecksum = value
			End Set
		End Property
		Private privateLibrary As AnimationLibrary
		Public Property Library() As AnimationLibrary
			Get
				Return privateLibrary
			End Get
			Private Set(ByVal value As AnimationLibrary)
				privateLibrary = value
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
		Private privateUsageCount As Int32
		Public Property UsageCount() As Int32
			Get
				Return privateUsageCount
			End Get
			Friend Set(ByVal value As Int32)
				privateUsageCount = value
			End Set
		End Property
		Private privateDuration As TimeSpan
		Public Property Duration() As TimeSpan
			Get
				Return privateDuration
			End Get
			Friend Set(ByVal value As TimeSpan)
				privateDuration = value
			End Set
		End Property
		Friend m_AudioBytes() As Byte

		Friend Sub Save(ByVal inWriter As BinaryWriter)
			' write audio to stream
			inWriter.Write(Convert.ToByte(6)) ' audio type ID
			inWriter.Write(Convert.ToUInt16(&H1000)) ' audio version

			inWriter.Write(Convert.ToString(Name))
			inWriter.Write(Convert.ToInt64((Checksum)))
			inWriter.Write(Convert.ToInt32(CInt(UsageCount)))
			inWriter.Write(Convert.ToInt64((Duration.Ticks)))
		End Sub

		Friend Shared Function Load(ByVal inLibrary As AnimationLibrary, ByVal inReader As BinaryReader) As AudioObject
			Dim result As New AudioObject()

			If inReader.ReadByte() <> 6 Then
				Throw New InvalidDataException()
			End If

			Select Case inReader.ReadUInt16()

				Case &H1000
					result.Name = inReader.ReadString()
					result.Checksum = inReader.ReadInt64()
					result.UsageCount = inReader.ReadInt32()
					result.Duration = New TimeSpan(inReader.ReadInt64())
				Case Else
					Throw New InvalidDataException()
			End Select

			result.Library = inLibrary

			Return result
		End Function

		Private Sub New()
		End Sub

		Friend Sub New(ByVal inLibrary As AnimationLibrary, ByVal inAudioData() As Byte)
			If inLibrary Is Nothing Then
				Throw New ArgumentNullException()
			End If

			m_AudioBytes = inAudioData
			Library = inLibrary
			Dim hash() As Byte = System.Security.Cryptography.MD5.Create().ComputeHash(New MemoryStream(inAudioData))

			Checksum = 0

			For i As Integer = 0 To 7
				Checksum = Checksum Or ((Convert.ToInt64((hash(i)))) << (i * 8))
			Next i

			' validate file
			CreatePlayer(Nothing).Dispose()

			'Duration = ComputeDuration();
		End Sub

		'private TimeSpan ComputeDuration()
		'{
		'    WaveStream readerStream = new WaveFileReader(new MemoryStream(m_AudioBytes));

		'    try
		'    {
		'        WaveChannel32 waveStream = new WaveChannel32(readerStream);

		'        using (waveStream)
		'        {
		'            return waveStream.TotalTime;
		'        }
		'    }
		'    finally
		'    {
		'        readerStream.Dispose();
		'    }
		'}

		Friend Sub Load()
			If m_AudioBytes Is Nothing Then
				m_AudioBytes = Library.LoadAudio(Me)
			End If
		End Sub

		Public Function CreatePlayer(ByVal inAnimation As Animation) As ISoundPlayer
			Load()

			Dim result As New SoundPlayer(m_AudioBytes)

			result.Animation = inAnimation

			Return result
		End Function
	End Class
End Namespace
