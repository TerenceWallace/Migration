Imports Migration.Common
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports Migration.Core

#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering
	''' <summary>
	''' Seems a little odd to me that some hundred texture switches and GL.Begin/End clauses in conjunction
	''' with alpha blending are killing even a GTX 260, so here is the solution, a fast sprite engine based
	''' on alpha test instead of blending and using texture atlas as well as radix sort to schedule rendering
	''' tasks according to these atlas. Further, it supports three different modes of texture atlas 
	''' generation. The first one just generates them on the fly, which is rather expensive but fills the
	''' cache file with data for mode two, which will be used on next start then. Mode two uses usual bitmap
	''' images stored in the texture cache as atli, but still has to convert them into the driver internal format
	''' on the fly. The third mode needs manual processing by letting Migration generate texture atli for
	''' all animation library frames. These frames are stored as PNG files and can now be converted into DXT3
	''' compressed DDS files (NVidia Photoshop plugin or whatever) manually. Then another invokation of Migration
	''' will extend the image data in the texture cache with its DDS data. Next time the game loads, it will now
	''' directly use the compressed images within the texture cache, without any postprocessing done by the driver.
	''' Mode three is meant for production use, whereas the first two modes are the default setting for development,
	''' since they work fully automatic. We still have to keep the original image data in mode three, since some
	''' systems might not support DXT3 and thus need the uncompressed image data...
	''' </summary>
	<Serializable> _
	Public Class SpriteEngine
		Implements IDisposable

		<NonSerialized()> _
		Private m_NameToClass As New SortedDictionary(Of String, Character)()
		Private ReadOnly m_AtlasList As New List(Of TextureAtlas)()
		Private ReadOnly m_ChecksumToEntry As New SortedDictionary(Of Long, TextureAtlasEntry)()
		Private ReadOnly m_FrameToEntry As New List(Of TextureAtlasEntry)()

		<NonSerialized()> _
		Private m_CacheFile As FileStream

		Private privateCustomChecksums As List(Of Long)
		Public Property CustomChecksums() As List(Of Long)
			Get
				Return privateCustomChecksums
			End Get
			Private Set(ByVal value As List(Of Long))
				privateCustomChecksums = value
			End Set
		End Property

		<OnDeserialized()> _
		Private Sub OnDeserialized(ByVal ctx As StreamingContext)
			m_NameToClass = New SortedDictionary(Of String, Character)()
		End Sub

		Private privateAtlasSize As Integer = 2048
		Public Property AtlasSize() As Integer
			Get
				Return privateAtlasSize
			End Get
			Private Set(ByVal value As Integer)
				privateAtlasSize = value
			End Set
		End Property

		Public Sub Dispose() Implements IDisposable.Dispose
			If m_CacheFile IsNot Nothing Then
				m_CacheFile.Dispose()
			End If

			m_CacheFile = Nothing
		End Sub

		Private Sub New(ByVal inCacheFile As FileStream, ByVal inAtlasSize As Integer)
			CustomChecksums = New List(Of Long)()
			m_CacheFile = inCacheFile
			AtlasSize = inAtlasSize

			Dim log2Factor As Double = 1.0 / Math.Log(2.0)
			Dim SizeShift As Integer = Convert.ToInt32(CInt(Fix(Math.Floor((Math.Log(Convert.ToDouble(AtlasSize)) * log2Factor) + 0.5))))

			If Convert.ToInt32(CInt(Fix(Math.Pow(2, Convert.ToDouble(SizeShift))))) <> AtlasSize Then
				Throw New ArgumentException("Texture atlas width and height must be a power of two!")
			End If
		End Sub

		Public Shared Function OpenOrCreate(ByVal inCacheFile As String, ByVal inAtlasSize As Int32) As SpriteEngine
			Dim fileStream As New FileStream(inCacheFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read)
			Dim engine As SpriteEngine = Nothing

			If fileStream.Length > 0 Then
				' deserialize sprite engine
				Dim format As New BinaryFormatter()

				Try
					engine = CType(format.Deserialize(fileStream), SpriteEngine)
					engine.m_CacheFile = fileStream
				Catch
					' create a new one
					engine = New SpriteEngine(fileStream, inAtlasSize)
				End Try
			Else
				' create a new one
				engine = New SpriteEngine(fileStream, inAtlasSize)
			End If

			Return engine
		End Function

		Public Sub BeginUpdate()
			m_AtlasList.Clear()
			m_ChecksumToEntry.Clear()
			m_FrameToEntry.Clear()
			m_NameToClass.Clear()
		End Sub

		Public Sub UpdateClass(ByVal inClass As Character)
			If m_NameToClass.ContainsKey(inClass.Name) Then
				Return
			End If

			m_NameToClass.Add(inClass.Name, inClass)

			For Each mSet As AnimationSet In inClass.Sets
				For Each anim As Animation In mSet.Animations
					For Each Frame As AnimationFrame In anim.Frames
						Dim entry As New TextureAtlasEntry() With {.PixRect = New Rectangle(0, 0, Frame.Width, Frame.Height), .Image = Frame.Source, .AreaSize = Frame.Width * Frame.Height, .Checksum = Frame.Checksum}

#If DEBUG Then
						If AtlasSize <= 0 Then
							AtlasSize = 2048
						End If
#End If
						If (entry.PixRect.Width > AtlasSize) OrElse (entry.PixRect.Height > AtlasSize) Then
							Throw New ArgumentOutOfRangeException("Frame size exceed texture atlas size!")
						End If

						Do While m_FrameToEntry.Count <= Frame.Index
							m_FrameToEntry.Add(Nothing)
						Loop

						If m_ChecksumToEntry.ContainsKey(Frame.Checksum) Then
							' create reference to existing frame (currently this is already done implicitly by the animation library but may change in future)
							m_FrameToEntry(Frame.Index) = m_ChecksumToEntry(Frame.Checksum)

							Continue For
						End If

						m_ChecksumToEntry.Add(Frame.Checksum, entry)
						m_FrameToEntry(Frame.Index) = entry
					Next Frame
				Next anim
			Next mSet
		End Sub

		Public Sub EndUpdate()
			Dim m_AtlasTree As New AtlasTree(AtlasSize, AtlasSize)
			Dim atlasNode As AtlasTree = Nothing
			Dim orderedEntries() As TextureAtlasEntry = m_ChecksumToEntry.Values.OrderByDescending(Function(e) e.AreaSize).ToArray()
			Dim dAtlasSize As Double = AtlasSize

			' TODO: use a better algorithm for texture atlas generation, maybe genetic or swarm...
			Do While orderedEntries.Any(Function(e) e IsNot Nothing)
				For i As Integer = 0 To orderedEntries.Length - 1
					Dim entry As TextureAtlasEntry = orderedEntries(i)

					If entry Is Nothing Then
						Continue For
					End If

					atlasNode = m_AtlasTree.Insert(entry)
					If atlasNode Is Nothing Then
						Continue For
					End If

					entry.PixRect = atlasNode.Rect
					entry.TexRect = New RectangleDouble(atlasNode.Rect.Left / dAtlasSize, atlasNode.Rect.Top / dAtlasSize, atlasNode.Rect.Width / dAtlasSize, atlasNode.Rect.Height / dAtlasSize)

					orderedEntries(i) = Nothing
				Next i

				m_AtlasList.Add(New TextureAtlas(m_AtlasTree))
				m_AtlasTree = New AtlasTree(AtlasSize, AtlasSize)
			Loop

			' save state to disk
			Dim format As New BinaryFormatter()

			m_CacheFile.Position = 0
			m_CacheFile.SetLength(0)

			format.Serialize(m_CacheFile, Me)
			m_CacheFile.Flush()

			m_CacheFile.Position = 0

			m_NameToClass.Clear()
		End Sub

		Public Sub BeginRadixRender()
			For Each atlas As TextureAtlas In m_AtlasList
				atlas.Tasks.Clear()
			Next atlas
		End Sub

		''' <summary>
		''' This is a little specialized, but since we are only dealing with frames in the main render pipeline,
		''' it is suiteable to have such a shortcut. The following will schedule the given task according to the
		''' atlas containing the given frame. When <see cref="EndRadixRender"/> is finally called, each atlas is
		''' only bound once to OpenGL and all tasks scheduled for this particular atlas are invoked. Further, we
		''' know that we are dealing with sprites here and thus only one GL.Begin/End clause is used per atlas,
		''' together resulting in the maximum performance one can archieve with sprites. The scheduling is performed
		''' in practical constant time, radix sort...
		''' </summary>
		''' <param name="inFrameIndex"></param>
		''' <param name="inTask">A callback, receiving the texture atlas subimage area in UV coordinates.</param>
		Public Sub RadixRenderSchedule(ByVal inFrameIndex As Integer, ByVal inTask As Procedure(Of RectangleDouble))
			If inTask Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Dim frame As TextureAtlasEntry = m_FrameToEntry(inFrameIndex)
			frame.Atlas.Tasks.Add(New TaskEntry() With {.Handler = inTask, .TexCoords = frame.TexRect})
		End Sub

		Public Sub EndRadixRender()
			For Each atlas As TextureAtlas In m_AtlasList
				atlas.Texture.Bind()

				GL.Begin(BeginMode.Quads)
				For Each task As TaskEntry In atlas.Tasks
					task.Handler(task.TexCoords)
				Next task
				GL.End()
			Next atlas
		End Sub

	End Class
End Namespace
