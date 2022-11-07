Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Common

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class ListBox
		Inherits Control

		Private m_ItemWidth As Int32
		Private m_ItemHeight As Int32
		Private ReadOnly m_Items As New List(Of ListBoxItem)()
		Private m_SelectionIndex As Integer = -1

		Private privateItemTemplateString As String
		<XmlAttribute("ItemTemplate")> _
		Public Property ItemTemplateString() As String
			Get
				Return privateItemTemplateString
			End Get
			Set(ByVal value As String)
				privateItemTemplateString = value
			End Set
		End Property

		Private privateItemTemplate As Template
		<XmlIgnore()> _
		Public Property ItemTemplate() As Template
			Get
				Return privateItemTemplate
			End Get
			Set(ByVal value As Template)
				privateItemTemplate = value
			End Set
		End Property

		<XmlIgnore()> _
		Public ReadOnly Property Count() As Integer
			Get
				Return Children.Count
			End Get
		End Property

		<XmlIgnore()> _
		Public Property SelectionIndex() As Integer
			Get
				Return m_SelectionIndex
			End Get
			Set(ByVal value As Integer)
				If value = m_SelectionIndex Then
					Return
				End If

				For Each child As Control In Children
					Dim item As Button = CType(child, Button)

					item.IsDown = False
				Next child

				If value >= 0 Then
					If value >= Children.Count Then
						Throw New ArgumentOutOfRangeException()
					End If

					m_SelectionIndex = value
					CType(Children.ElementAt(value), Button).IsDown = True
				Else
					m_SelectionIndex = -1
				End If
			End Set
		End Property

		Public Event OnSelectionChanged As DNotifyHandler(Of ListBox, ListBoxItem, ListBoxItem)

		Public Sub New()
			MyBase.New()
		End Sub

		Public Overrides Sub XMLPostProcess(ByVal inLayout As XMLGUILayout)
			MyBase.XMLPostProcess(inLayout)

			If String.IsNullOrEmpty(ItemTemplateString) Then
				Throw New ArgumentException("A ListBox must specify an item template.")
			End If

			ItemTemplate = inLayout.GetTemplate(ItemTemplateString)

			ItemTemplate.XMLPostProcess(inLayout)

			If Width <= 0 Then
				Throw New ArgumentException("A ListBox must have a positive, fixed width.")
			End If

			Dim ctrl As Control = CreateItem(Nothing)

			m_ItemWidth = ctrl.Width
			m_ItemHeight = ctrl.Height
			If (m_ItemWidth <= 0) OrElse (m_ItemHeight <= 0) Then
				Throw New ArgumentException("A ListBox item must have a positive, fixed width and height.")
			End If

			If Not(TypeOf ctrl Is Button) Then
				Throw New ArgumentException("A ListBoxItem shall be a subclass of ""Button"".")
			End If

			If m_ItemWidth > Width Then
				Throw New ArgumentException("A ListBox must be at least large enough to hold one entry.")
			End If
		End Sub

		Private Function CreateItem(ByVal inNewItem As ListBoxItem) As Control
			Dim childs As List(Of Control) = Nothing
			Dim presenters As List(Of Control) = Nothing
			Dim iLeft As Integer = Children.Count

			If inNewItem IsNot Nothing Then
				ItemTemplate.Instantiate(childs, presenters, inNewItem.Params)
			Else
				ItemTemplate.Instantiate(childs, presenters)
			End If

			If childs.Count <> 1 Then
				Throw New ArgumentException("A ListBox item template must have exactly one control on its first level.")
			End If

			Return childs.First()
		End Function

		Public Sub AddItem(ByVal inNewItem As ListBoxItem)
			Dim item As Button = CType(CreateItem(inNewItem), Button)

			Dim iLeft As Integer = Width \ m_ItemWidth
			Dim iTop As Integer = Children.Count \ iLeft

			item.Left = m_ItemWidth * (Children.Count Mod iLeft)
			item.Top = m_ItemHeight * iTop

			AddHandler item.OnClick, AddressOf OnItemClick

			Children.Add(item)
			m_Items.Add(inNewItem)
		End Sub

		Private Sub OnItemClick(ByVal inSender As Button)
			Dim backup As Integer = SelectionIndex
			m_SelectionIndex = Children.IndexOf(inSender)

			RaiseEvent OnSelectionChanged(Me, (If(backup >= 0, m_Items(backup), Nothing)), (If(SelectionIndex >= 0, m_Items(SelectionIndex), Nothing)))
		End Sub

		Public Sub RemoveAt(ByVal inIndex As Integer)
			Children.Remove(Children.ElementAt(inIndex))
			m_Items.RemoveAt(inIndex)
		End Sub

		Public Sub Clear()
			Children.Clear()
			m_Items.Clear()
		End Sub
	End Class
End Namespace
