Imports Migration.Common
Imports Migration.Core


#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering


	''' <summary>
	''' This class provides the finest level of control about visual rendering.
	''' It will render the given native texture according to all other given
	''' parameters.
	''' </summary>
	''' <remarks>
	''' At the beginning it might be confusing what exactly a width or height value,
	''' for example, means. The thing is that it depends on configuration.  
	''' Just thread virtual pixels as real pixel size, only scaled by internal parameters.
	''' 
	''' With OpenGL always keep in mind that the we have to call ALL GL commands
	''' withtin the same thread, usually the internal render loop thread. So any
	''' methods and properties exposed to the public MUSTN'T call any GL commands,
	''' instead the calls shall be deferred.
	''' </remarks>
	Public MustInherit Class RenderableVisual
		Inherits PositionTracker

		'Private Shared m_RenderShiftRandom As New CrossRandom(0)
		Private m_Alpha As Byte = 255

		''' <summary>
		''' The ratio between width and height initially passed to the constructor.
		''' </summary>
		Private privateAspectRatio As Double
		Public Property AspectRatio() As Double
			Get
				Return privateAspectRatio
			End Get
			Set(ByVal value As Double)
				privateAspectRatio = value
			End Set
		End Property
		''' <summary>
		''' Internally allows to set a custom texture at any time, may also be null.
		''' </summary>
		Private privateTexture As NativeTexture
		Friend Property Texture() As NativeTexture
			Get
				Return privateTexture
			End Get
			Set(ByVal value As NativeTexture)
				privateTexture = value
			End Set
		End Property
		''' <summary>
		''' The parent renderer.
		''' </summary>
		Private privateRenderer As TerrainRenderer
		Public Property Renderer() As TerrainRenderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateRenderer = value
			End Set
		End Property
		''' <summary>
		''' Default is one, and denotes the scaling factor for the whole rendered object.
		''' </summary>
		Private privateScale As Double
		Public Property Scale() As Double
			Get
				Return privateScale
			End Get
			Friend Set(ByVal value As Double)
				privateScale = value
			End Set
		End Property
		''' <summary>
		''' Should be rendered at all?
		''' </summary>
		Private privateIsVisible As Boolean
		Public Property IsVisible() As Boolean
			Get
				Return privateIsVisible
			End Get
			Set(ByVal value As Boolean)
				privateIsVisible = value
			End Set
		End Property
		''' <summary>
		''' Usuall set to 1.0, which also provides the fastest rendering using only one quad.
		''' For building animations, this will use a special subdivided, incomplete quad to
		''' so that by incrementally adding parts, the building gets more and more visible.
		''' </summary>
		Private privateBuildingProgress As Double
		Public Property BuildingProgress() As Double
			Get
				Return privateBuildingProgress
			End Get
			Set(ByVal value As Double)
				privateBuildingProgress = value
			End Set
		End Property

		Private privatePositionShiftX As SByte
		Public Property PositionShiftX() As SByte
			Get
				Return privatePositionShiftX
			End Get
			Protected Set(ByVal value As SByte)
				privatePositionShiftX = value
			End Set
		End Property
		Private privatePositionShiftY As SByte
		Public Property PositionShiftY() As SByte
			Get
				Return privatePositionShiftY
			End Get
			Protected Set(ByVal value As SByte)
				privatePositionShiftY = value
			End Set
		End Property

		''' <summary>
		''' By default, the Z-Shift is set to the terrain height at the position trackable's 
		''' coordinates. In case of resource stacks or animated movables this isn't sufficient.
		''' So if this field has a value, it will be used as Z-Shift instead.
		''' </summary>
		Private privateZShiftOverride? As Double
		Public Property ZShiftOverride() As Double?
			Get
				Return privateZShiftOverride
			End Get
			Set(ByVal value? As Double)
				privateZShiftOverride = value
			End Set
		End Property
		Private privateIsCentered As Boolean
		Public Property IsCentered() As Boolean
			Get
				Return privateIsCentered
			End Get
			Set(ByVal value As Boolean)
				privateIsCentered = value
			End Set
		End Property

		Public Property Opacity() As Double
			Get
				Return m_Alpha / 255.0
			End Get
			Set(ByVal value As Double)
				m_Alpha = Convert.ToByte(255 * Math.Max(0, Math.Min(1.0, value)))
			End Set
		End Property

		''' <summary>
		''' 
		''' </summary>
		''' <param name="inParent">The parent renderer.</param>
		Friend Sub New(ByVal inParent As TerrainRenderer, ByVal inInitialPosition As CyclePoint)
			If inParent Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Renderer = inParent
			Scale = 1
			IsVisible = True
			BuildingProgress = 1.0
			Position = inInitialPosition
		End Sub

		Public Overridable Sub SetAnimationTime(ByVal inTime As Int64)
		End Sub

		''' <summary>
		''' Renders the object with default parameters.
		''' </summary>
		Friend MustOverride Sub Render(ByVal inPass As RenderPass)

		''' <summary>
		''' Is intended for derived classes and provides a way to fine tune the final
		''' size of the rendered object according to additional frame size and offsets.
		''' </summary>
		''' <param name="inOffsetX">Virtual pixel offset in X direction.</param>
		''' <param name="inOffsetY">Virtual pixel offset in Y direction.</param>
		''' <param name="inWidth">Virtual pixel width; overrides the instance width for this call.</param>
		''' <param name="inHeight">Virtual pixel height; overrides the instance height for this call.</param>
		Friend Sub Render(ByVal inPass As RenderPass, ByVal inFrameIndex As Integer, ByVal inOffsetX As Double, ByVal inOffsetY As Double, ByVal inWidth As Double, ByVal inHeight As Double)
			If Not IsVisible Then
				Return
			End If

			Dim ortho As Double = 1.41421356
			Dim x As Double = Position.X + PositionShiftX + ((If(IsCentered, (inOffsetX - inWidth / 2), inOffsetX))) * Scale
			Dim y As Double = Position.Y + PositionShiftY + ((If(IsCentered, (inOffsetY - inHeight / 2), inOffsetY))) * Scale * ortho

			If (Renderer.SpriteEngine Is Nothing) OrElse (inFrameIndex < 0) Then
				Texture.Bind()

				Renderer.DrawAtlas(Me, Convert.ToSingle(x), Convert.ToSingle(y), GetZShift(), Convert.ToSingle(inWidth * Scale), Convert.ToSingle(inHeight * Scale * ortho), 255, 255, 255, m_Alpha, 0F, 0F, 1F, 1F)
			Else
				' Correct texture and QUAD mode is already activated by sprite engine.
				' ATTENTION: Keep in mind that this method is executed asynchronously!
				Renderer.SpriteEngine.RadixRenderSchedule(inFrameIndex, Sub(texCoords As RectangleDouble) Renderer.DrawAtlasStripe(Me, Convert.ToSingle(x), Convert.ToSingle(y) + GetZShift(), -Convert.ToSingle((Position.Y + PositionShiftY) / 2000.0), Convert.ToSingle(inWidth * Scale), Convert.ToSingle(inHeight * Scale * ortho), 255, 255, 255, m_Alpha, Convert.ToSingle(texCoords.Left), Convert.ToSingle(texCoords.Top), Convert.ToSingle(texCoords.Right), Convert.ToSingle(texCoords.Bottom)))
			End If
		End Sub

		Public Overridable Function GetZShift() As Single
			If (ZShiftOverride IsNot Nothing) AndAlso (ZShiftOverride.HasValue) Then
				Return Convert.ToSingle(ZShiftOverride.Value)
			End If

			Return Renderer.Terrain.GetZShiftAt(Position.XGrid, Position.YGrid)
		End Function
	End Class
End Namespace
