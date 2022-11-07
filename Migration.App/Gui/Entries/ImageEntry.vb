Imports System.Reflection

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class ImageEntry
		Private Shared s_ImageID As Integer = 0

		Private privateName As String
		Public Property Name() As String
			Get
				Return privateName
			End Get
			Private Set(ByVal value As String)
				privateName = value
			End Set
		End Property

		Private privatePath As String
		Public Property Path() As String
			Get
				Return privatePath
			End Get
			Private Set(ByVal value As String)
				privatePath = value
			End Set
		End Property

		Private privateImage As System.Drawing.Bitmap
		Public Property Image() As System.Drawing.Bitmap
			Get
				Return privateImage
			End Get
			Private Set(ByVal value As System.Drawing.Bitmap)
				privateImage = value
			End Set
		End Property

		Private privateImageID As Integer
		Public Property ImageID() As Integer
			Get
				Return privateImageID
			End Get
			Private Set(ByVal value As Integer)
				privateImageID = value
			End Set
		End Property

		Friend Sub New(ByVal inParent As XMLGUIConfig, ByVal inResourceName As String)
			Name = inResourceName
			Path = System.IO.Path.GetFullPath(inParent.ConfigDirectory & Name)
			ImageID = System.Threading.Interlocked.Increment(s_ImageID)
			Image = CType(System.Drawing.Bitmap.FromFile(Path), System.Drawing.Bitmap)
			inParent.Renderer.RegisterImage(ImageID, Image)
		End Sub

		Friend Sub New(ByVal inParent As XMLGUIConfig, ByVal inBitmap As System.Drawing.Bitmap)
			ImageID = System.Threading.Interlocked.Increment(s_ImageID)
			Image = inBitmap
			inParent.Renderer.RegisterImage(ImageID, Image)
		End Sub
	End Class
End Namespace
