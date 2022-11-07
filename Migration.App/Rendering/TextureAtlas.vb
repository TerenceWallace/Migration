Imports System.Runtime.Serialization
Imports Migration.Common
Imports Migration.Core

#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering
	<Serializable()> _
	Friend Class TextureAtlas

		<NonSerialized()> _
		Private m_Tasks As New List(Of TaskEntry)()
		Public ReadOnly Property Tasks() As List(Of TaskEntry)
			Get
				Return m_Tasks
			End Get
		End Property

		Private m_Image As Bitmap
		<NonSerialized()> _
		Private m_Texture As NativeTexture

		<OnDeserialized()> _
		Private Sub OnDeserialized(ByVal ctx As StreamingContext)
			m_Tasks = New List(Of TaskEntry)()
		End Sub

		Public ReadOnly Property Texture() As NativeTexture
			Get
				If m_Texture Is Nothing Then
					m_Texture = New NativeTexture(TextureOptions.None, m_Image)
				End If

				Return m_Texture
			End Get
		End Property

		Public Sub New(ByVal inAtlasTree As AtlasTree)
			m_Image = New Bitmap(inAtlasTree.Rect.Width, inAtlasTree.Rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
			Dim g As Graphics = Graphics.FromImage(m_Image)

			Using g
				ProcessNode(g, inAtlasTree)
			End Using

			m_Texture = New NativeTexture(TextureOptions.None, m_Image)
		End Sub

		Private Sub ProcessNode(ByVal g As Graphics, ByVal node As AtlasTree)
			If node Is Nothing Then
				Return
			End If

			If node.Entry IsNot Nothing Then
				g.DrawImageUnscaled(node.Entry.Image, node.Rect.Left, node.Rect.Top)

				node.Entry.Atlas = Me
				node.Entry.Image = Nothing
			End If

			ProcessNode(g, node.Left)
			ProcessNode(g, node.Right)
		End Sub
	End Class
End Namespace
