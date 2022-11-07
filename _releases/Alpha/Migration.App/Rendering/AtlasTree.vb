Imports Migration.Core

#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering

	Friend Class AtlasTree
		Public Left As AtlasTree
		Public Right As AtlasTree
		Public Rect As Rectangle
		Public Entry As TextureAtlasEntry

		Private Sub New()
		End Sub

		Public Sub New(ByVal inWidth As Integer, ByVal inHeight As Integer)
			Rect = New Rectangle(0, 0, inWidth, inHeight)
		End Sub

		Public Function Insert(ByVal inNewEntry As TextureAtlasEntry) As AtlasTree

			If (Left IsNot Nothing) OrElse (Right IsNot Nothing) Then
				Dim newNode As AtlasTree = Nothing

				If Left IsNot Nothing Then
					newNode = Left.Insert(inNewEntry)
				End If

				If newNode IsNot Nothing Then
					Return newNode
				End If

				Return Right.Insert(inNewEntry)
			Else
				If Entry IsNot Nothing Then
					Return Nothing
				End If

				If (Rect.Width < inNewEntry.PixRect.Width) OrElse (Rect.Height < inNewEntry.PixRect.Height) Then
					Return Nothing
				End If

				If (Rect.Width = inNewEntry.PixRect.Width) AndAlso (Rect.Height = inNewEntry.PixRect.Height) Then
					Entry = inNewEntry

					Return Me
				End If

				Left = New AtlasTree()
				Right = New AtlasTree()

				Dim dw As Integer = Rect.Width - inNewEntry.PixRect.Width
				Dim dh As Integer = Rect.Height - inNewEntry.PixRect.Height

				If dw > dh Then
					Left.Rect = New Rectangle(Rect.Left, Rect.Top, inNewEntry.PixRect.Width, Rect.Height)
					Right.Rect = New Rectangle(Rect.Left + inNewEntry.PixRect.Width, Rect.Top, dw, Rect.Height)
				Else
					Left.Rect = New Rectangle(Rect.Left, Rect.Top, Rect.Width, inNewEntry.PixRect.Height)
					Right.Rect = New Rectangle(Rect.Left, Rect.Top + inNewEntry.PixRect.Height, Rect.Width, dh)
				End If

				Return Left.Insert(inNewEntry)
			End If
		End Function
	End Class
End Namespace