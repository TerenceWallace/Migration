Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Common
Imports Migration.Rendering

Namespace Migration

	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class Frame
		Inherits Control

		Private m_Config As FrameEntry

		<XmlAttribute("HiddenBorders")> _
		Public Property HiddenBordersString() As String
			Get
				Return HiddenBorders.ToString()
			End Get
			Set(ByVal value As String)
				HiddenBorders = CType(System.Enum.Parse(GetType(FrameBorders), value), FrameBorders)
			End Set
		End Property

		<XmlAttribute> _
		Public Property Config() As String
			Get
				If m_Config IsNot Nothing Then
					Return m_Config.Name
				Else
					Return Nothing
				End If
			End Get
			Set(ByVal value As String)
				m_Config = Gui.Loader.GUIConfig.GetFrame(value)
			End Set
		End Property

		Private privateHiddenBorders As FrameBorders
		<XmlIgnore> _
		Public Property HiddenBorders() As FrameBorders
			Get
				Return privateHiddenBorders
			End Get
			Set(ByVal value As FrameBorders)
				privateHiddenBorders = value
			End Set
		End Property

		Public Sub New()
			MyBase.New()
			HiddenBorders = FrameBorders.None
		End Sub

		Public Sub New(ByVal inParent As Control, ByVal inFrameConfig As String)
			MyBase.New(inParent)
			Config = inFrameConfig
		End Sub

		Private Sub RenderTile(ByVal inChainLeft As Integer, ByVal inChainTop As Integer, ByVal tile As TileEntry, ByVal deltaX As Double, ByVal deltaY As Double, ByVal deltaWidth As Double, ByVal deltaHeight As Double)
			Renderer.DrawSpriteAtlas((inChainLeft + Left + tile.X + deltaX) * WidthScale, (inChainTop + Top + tile.Y + deltaY) * HeightScale, (tile.Width + deltaWidth) * WidthScale, (tile.Height + deltaHeight) * HeightScale, m_Config.ImageID, Opacity, tile.X, tile.Y, tile.Width, tile.Height)
		End Sub

		Protected Overrides Sub Render(ByVal inChainLeft As Integer, ByVal inChainTop As Integer)
			Dim tiles() As TileEntry = m_Config.Atlas.Tiles
			Dim midWidth As Integer = Width - tiles(0).Width - tiles(1).Width - tiles(2).Width
			Dim midHeight As Integer = Height - tiles(0).Height - tiles(1).Height - tiles(2).Height
			Dim hideTop As Boolean = (HiddenBorders And FrameBorders.Top) <> 0
			Dim hideLeft As Boolean = (HiddenBorders And FrameBorders.Left) <> 0
			Dim hideBottom As Boolean = (HiddenBorders And FrameBorders.Bottom) <> 0
			Dim hideRight As Boolean = (HiddenBorders And FrameBorders.Right) <> 0
			Dim hideMiddle As Boolean = (HiddenBorders And FrameBorders.Middle) <> 0

			If ((Not hideTop)) AndAlso ((Not hideLeft)) Then
				RenderTile(inChainLeft, inChainTop, tiles(0), 0, 0, 0, 0)
			End If
			If Not hideTop Then
				RenderTile(inChainLeft, inChainTop, tiles(1), 0, 0, midWidth, 0)
			End If
			If ((Not hideTop)) AndAlso ((Not hideRight)) Then
				RenderTile(inChainLeft, inChainTop, tiles(2), midWidth, 0, 0, 0)
			End If
			If Not hideRight Then
				RenderTile(inChainLeft, inChainTop, tiles(3), midWidth, 0, 0, midHeight)
			End If
			If ((Not hideRight)) AndAlso ((Not hideBottom)) Then
				RenderTile(inChainLeft, inChainTop, tiles(4), midWidth, midHeight, 0, 0)
			End If
			If Not hideBottom Then
				RenderTile(inChainLeft, inChainTop, tiles(5), 0, midHeight, midWidth, 0)
			End If
			If ((Not hideBottom)) AndAlso ((Not hideLeft)) Then
				RenderTile(inChainLeft, inChainTop, tiles(6), 0, midHeight, 0, 0)
			End If
			If Not hideLeft Then
				RenderTile(inChainLeft, inChainTop, tiles(7), 0, 0, 0, midHeight)
			End If
			If Not hideMiddle Then
				RenderTile(inChainLeft, inChainTop, tiles(8), 0, 0, midWidth, midHeight)
			End If

			MyBase.Render(inChainLeft, inChainTop)
		End Sub
	End Class
End Namespace
