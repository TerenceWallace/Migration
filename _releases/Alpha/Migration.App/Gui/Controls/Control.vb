Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Common
Imports Migration.Rendering

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True), Serializable()> _
	Public Class Control

		Public Const InvalidMouseButton As Integer = -1
		Public Const LeftMouseButton As Integer = 0
		Public Const MiddleMouseButton As Integer = 1
		Public Const RightMouseButton As Integer = 2

		<XmlIgnore()> _
		Private ReadOnly m_Children As ControlCollection

		Private privateRootElement As RootControl
		<XmlIgnore()> _
		Public Property RootElement() As RootControl
			Get
				Return privateRootElement
			End Get
			Private Set(ByVal value As RootControl)
				privateRootElement = value
			End Set
		End Property

		<XmlElement(GetType(Control)), XmlElement(GetType(Image)), XmlElement(GetType(Frame)), XmlElement(GetType(Button)), XmlElement(GetType(Content)), XmlElement(GetType(ListBox)), XmlElement(GetType(TabButton)), XmlElement(GetType(RadioButton)), XmlElement(GetType(Label))> _
		Public Property XMLChildren() As List(Of Control)

		Private privateTemplateString As String
		<XmlAttribute("Template")> _
		Public Property TemplateString() As String
			Get
				Return privateTemplateString
			End Get
			Set(ByVal value As String)
				privateTemplateString = value
			End Set
		End Property

		<XmlIgnore()> _
		Public ReadOnly Property Children() As ICollection(Of Control)
			Get
				Return m_Children
			End Get
		End Property

		Private privateParent As Control
		<XmlIgnore()> _
		Public Property Parent() As Control
			Get
				Return privateParent
			End Get
			Friend Set(ByVal value As Control)
				privateParent = value
			End Set
		End Property

		Private privateOpacity As Double
		<XmlAttribute()> _
		Public Property Opacity() As Double
			Get
				Return privateOpacity
			End Get
			Set(ByVal value As Double)
				privateOpacity = value
			End Set
		End Property

		<XmlIgnore()> _
		Public ReadOnly Property Renderer() As Renderer
			Get
				Return Gui.Loader.GUIConfig.Renderer
			End Get
		End Property

		Private privateDataContextString As String
		<XmlAttribute("DataContext")> _
		Public Property DataContextString() As String
			Get
				Return privateDataContextString
			End Get
			Set(ByVal value As String)
				privateDataContextString = value
			End Set
		End Property

		Private privateWidth As Int32
		<XmlAttribute()> _
		Public Property Width() As Int32
			Get
				Return privateWidth
			End Get
			Set(ByVal value As Int32)
				privateWidth = value
			End Set
		End Property

		Private privateId As String
		<XmlAttribute()> _
		Public Property Id() As String
			Get
				Return privateId
			End Get
			Set(ByVal value As String)
				privateId = value
			End Set
		End Property

		Private privateHeight As Int32
		<XmlAttribute()> _
		Public Property Height() As Int32
			Get
				Return privateHeight
			End Get
			Set(ByVal value As Int32)
				privateHeight = value
			End Set
		End Property

		Private privateLeft As Int32
		<XmlAttribute()> _
		Public Property Left() As Int32
			Get
				Return privateLeft
			End Get
			Set(ByVal value As Int32)
				privateLeft = value
			End Set
		End Property

		Private privateTop As Int32
		<XmlAttribute()> _
		Public Property Top() As Int32
			Get
				Return privateTop
			End Get
			Set(ByVal value As Int32)
				privateTop = value
			End Set
		End Property

		Private privateIsVisible As Boolean
		<XmlAttribute()> _
		Public Property IsVisible() As Boolean
			Get
				Return privateIsVisible
			End Get
			Set(ByVal value As Boolean)
				privateIsVisible = value
			End Set
		End Property

		Private Shared privateHeightScale As Double
		<XmlIgnore()> _
		Friend Shared Property HeightScale() As Double
			Get
				Return privateHeightScale
			End Get
			Set(ByVal value As Double)
				privateHeightScale = value
			End Set
		End Property

		Private Shared privateWidthScale As Double
		<XmlIgnore()> _
		Friend Shared Property WidthScale() As Double
			Get
				Return privateWidthScale
			End Get
			Set(ByVal value As Double)
				privateWidthScale = value
			End Set
		End Property

		Private privateIsMouseOver As Boolean
		<XmlIgnore()> _
		Public Property IsMouseOver() As Boolean
			Get
				Return privateIsMouseOver
			End Get
			Private Set(ByVal value As Boolean)
				privateIsMouseOver = value
			End Set
		End Property

		Private privateIsMouseDown As Boolean
		<XmlIgnore()> _
		Public Property IsMouseDown() As Boolean
			Get
				Return privateIsMouseDown
			End Get
			Private Set(ByVal value As Boolean)
				privateIsMouseDown = value
			End Set
		End Property

		Private privateIsEnabled As Boolean
		<XmlAttribute()> _
		Public Property IsEnabled() As Boolean
			Get
				Return privateIsEnabled
			End Get
			Set(ByVal value As Boolean)
				privateIsEnabled = value
			End Set
		End Property

		Private privateMouseState As List(Of Integer)
		<XmlIgnore()> _
		Public Property MouseState() As List(Of Integer)
			Get
				Return privateMouseState
			End Get
			Private Set(ByVal value As List(Of Integer))
				privateMouseState = value
			End Set
		End Property

		Public Event OnMouseButtonUp As DNotifyHandler(Of Control, Integer, Integer, Integer)
		Public Event OnMouseButtonDown As DNotifyHandler(Of Control, Integer, Integer, Integer)
		Public Event OnMouseMove As DNotifyHandler(Of Control, Integer, Integer)
		Public Event OnMouseEnter As DNotifyHandler(Of Control)
		Public Event OnMouseLeave As DNotifyHandler(Of Control)

		Public Sub New()
			MouseState = New List(Of Integer)(0)
			m_Children = New ControlCollection(Me)

			Opacity = 1
			IsVisible = True
			IsEnabled = True
		End Sub

		Public Sub New(ParamArray ByVal inControls() As Control)
			Me.New()
			For Each child As Control In inControls
				Children.Add(child)
			Next child
		End Sub

		Public Overridable Sub XMLPostProcess(ByVal inLayout As XMLGUILayout)
			If inLayout IsNot Nothing Then
				RootElement = inLayout.RootElement
			End If

			' append children to templates
			If Not(String.IsNullOrEmpty(TemplateString)) Then
				Dim tempChilds As List(Of Control) = Nothing
				Dim tempPresenters As List(Of Control) = Nothing

				Dim  m_children As ICollection(Of Control) = Me.Children

				inLayout.GetTemplate(Me.TemplateString).Instantiate(tempChilds, tempPresenters, New Object(){})

				For Each child As Control In tempChilds
					child.RootElement = Me.RootElement
					Me.m_Children.Add(child)
				Next child

				' process children
				If XMLChildren IsNot Nothing Then
					'                    
					'                     * There might be multiple content presenters in a template and all of them
					'                     * need to be filled with all the control childs!
					'                     
					For Each contentPresenter As Control In tempPresenters
						For Each xmlChild As Control In XMLChildren
							Dim child As Control = CType(xmlChild, Control)

							child.XMLPostProcess(inLayout)

							contentPresenter.Children.Add(child)
						Next xmlChild
					Next contentPresenter

					XMLChildren = Nothing
				End If
			Else
				' process children
				If XMLChildren IsNot Nothing Then
					For Each xmlChild As Control In XMLChildren
						Dim child As Control = CType(xmlChild, Control)

						child.XMLPostProcess(inLayout)

						Children.Add(child)
					Next xmlChild

					XMLChildren = Nothing
				End If
			End If
		End Sub

		Public Function FindControl(ByVal inId As String) As Control
			If Id = inId Then
				Return Me
			End If

			If m_Children IsNot Nothing Then
				For Each child As Control In m_Children
					Dim ctrl As Control = child.FindControl(inId)

					If ctrl IsNot Nothing Then
						Return ctrl
					End If
				Next child
			Else
				Id = Nothing
			End If

			If XMLChildren IsNot Nothing Then
				'                
				'                 * This method is also being used during load time, so we need to
				'                 * search XML children if available...
				'                 
				For Each child As Control In XMLChildren
					Dim ctrl As Control = child.FindControl(inId)

					If ctrl IsNot Nothing Then
						Return ctrl
					End If
				Next child
			End If

			Return Nothing
		End Function

		Protected Sub EnumVisibleChildren(ByVal inEnumHandler As Procedure(Of Control))
			For Each child As Control In m_Children
				If child.IsVisible Then
					'                    
					'                     * If no height or width is specified, the child inherits its size
					'                     * from parent. This is important for templates or at least makes
					'                     * them easier.
					'                     
					Dim widthBackup As Integer = child.Width
					Dim heightBackup As Integer = child.Height

					If widthBackup = 0 Then
						child.Width = Width
					End If
					If heightBackup = 0 Then
						child.Height = Height
					End If

					Try
						inEnumHandler(child)
					Finally
						child.Width = widthBackup
						child.Height = heightBackup
					End Try
				End If
			Next child
		End Sub

		Friend Function ProcessMouseEventInternal(ByVal inMouseX As Integer, ByVal inMouseY As Integer, ByVal inButtonDown As Integer, ByVal inButtonUp As Integer) As Boolean
			Dim Result As Boolean = False

			' is within component?
			' notify child
			' propagate event to subchilds
			EnumVisibleChildren(Sub(child As Control)
				If Not child.IsEnabled Then
					Return
				End If
				Dim childMouseX As Integer = inMouseX - child.Left
				Dim childMouseY As Integer = inMouseY - child.Top
				If (childMouseX > child.Width) OrElse (childMouseY > child.Height) OrElse (childMouseX < 0) OrElse (childMouseY < 0) Then
					child.ResetMouseEvents()
					Return
				End If
				Result = True
				child.RaiseMouseEvents(childMouseX, childMouseY, inButtonDown, inButtonUp)
				child.ProcessMouseEventInternal(childMouseX, childMouseY, inButtonDown, inButtonUp)
			End Sub)

			Return Result
		End Function

		Private Sub ResetMouseEvents()
			If IsMouseOver Then
				IsMouseOver = False

				DoMouseLeave()

				RaiseEvent OnMouseLeave(Me)
			End If

			' no button up event will be raised if mouse left control during click...
			MouseState.Clear()
			IsMouseDown = False

			For Each child As Control In Children
				child.ResetMouseEvents()
			Next child
		End Sub

		Private Sub RaiseMouseEvents(ByVal inMouseX As Integer, ByVal inMouseY As Integer, ByVal inButtonDown As Integer, ByVal inButtonUp As Integer)
			' mouse is guaranteed to be within bounds...

			If Not IsMouseOver Then
				IsMouseOver = True

				DoMouseEnter()

				RaiseEvent OnMouseEnter(Me)
			End If

			If inButtonUp <> InvalidMouseButton Then
				If MouseState.Contains(inButtonUp) Then
					MouseState.Remove(inButtonUp)

					IsMouseDown = MouseState.Contains(LeftMouseButton)

					DoMouseButtonUp(inMouseX, inMouseY, inButtonUp)

					RaiseEvent OnMouseButtonUp(Me, inMouseX, inMouseY, inButtonUp)
				End If
			End If

			If inButtonDown <> InvalidMouseButton Then
				If Not(MouseState.Contains(inButtonDown)) Then
					MouseState.Add(inButtonDown)

					IsMouseDown = MouseState.Contains(LeftMouseButton)

					DoMouseButtonDown(inMouseX, inMouseY, inButtonUp)

					RaiseEvent OnMouseButtonDown(Me, inMouseX, inMouseY, inButtonDown)
				End If
			End If

			DoMouseMove(inMouseX, inMouseY)

			RaiseEvent OnMouseMove(Me, inMouseX, inMouseY)
		End Sub

		Protected Overridable Sub DoMouseButtonUp(ByVal inMouseX As Integer, ByVal inMouseY As Integer, ByVal inButton As Integer)
		End Sub
		Protected Overridable Sub DoMouseButtonDown(ByVal inMouseX As Integer, ByVal inMouseY As Integer, ByVal inButton As Integer)
		End Sub
		Protected Overridable Sub DoMouseMove(ByVal inMouseX As Integer, ByVal inMouseY As Integer)
		End Sub
		Protected Overridable Sub DoMouseEnter()
		End Sub
		Protected Overridable Sub DoMouseLeave()
		End Sub

		Protected Overridable Sub Render(ByVal inChainLeft As Integer, ByVal inChainTop As Integer)
            EnumVisibleChildren(Sub(child As Control) child.Render(inChainLeft + Left, inChainTop + Top))
		End Sub

	End Class
End Namespace
