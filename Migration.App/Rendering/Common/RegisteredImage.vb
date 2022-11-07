#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering
	Friend Class RegisteredImage

		Private privateImage As System.Drawing.Bitmap
		Public Property Image() As System.Drawing.Bitmap
			Get
				Return privateImage
			End Get
			Set(ByVal value As System.Drawing.Bitmap)
				privateImage = value
			End Set
		End Property

		Private privateTexture As NativeTexture
		Public Property Texture() As NativeTexture
			Get
				Return privateTexture
			End Get
			Set(ByVal value As NativeTexture)
				privateTexture = value
			End Set
		End Property
	End Class

End Namespace
