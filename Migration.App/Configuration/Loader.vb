Imports System.IO
Imports System.Text
Imports System.Xml.Serialization
Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Core

Namespace Migration.Configuration
	Public NotInheritable Class Loader
		Private Shared m_BuildingClasses() As Type

		Private Sub New()
		End Sub

		Shared Sub New()
			Dim classes As New List(Of Type)()

			' collect types derived from BaseBuilding
			For Each mType As Type In GetType(Migration.Game.Map).Assembly.GetTypes()
				If mType.Namespace <> My.Resources.NamespaceBuildings Then
					Continue For
				End If

				If Not(GetType(BaseBuilding).IsAssignableFrom(mType)) Then
					Continue For
				End If

				classes.Add(mType)
			Next mType

			m_BuildingClasses = classes.ToArray()
		End Sub

		Public Shared Function Open(ByVal inXMLPath As String) As RaceConfiguration
			Dim reader As New XmlSerializer(GetType(RaceConfiguration))
			Using stream As FileStream = System.IO.File.OpenRead(inXMLPath)
				Dim result As RaceConfiguration = Nothing
				Dim tabIndices(3) As Integer
				Using buffer As New MemoryStream()
					Using writer As New BinaryWriter(buffer)

						Using stream
							result = CType(reader.Deserialize(stream), RaceConfiguration)
						End Using

						writer.Write(CByte(result.Buildables.Count))

						For Each buildable As BuildingConfiguration In result.Buildables
							Dim mChar As Character = Nothing

							Try
								mChar = AnimationLibrary.Instance.FindClass(buildable.Character)


								'ORIGINAL LINE: if (++tabIndices[buildable.TabIndex] > 10)
								If tabIndices(buildable.TabIndex) > 10 Then
									Throw New ArgumentException("Too many items in building tab """ & buildable.TabIndex & """.")
								End If

								If Not(String.IsNullOrEmpty(buildable.ClassName)) Then
									For Each mType As Type In m_BuildingClasses
										If mType.Name = buildable.ClassName Then
											buildable.ClassType = mType

											Exit For
										End If
									Next mType

									If buildable.ClassType Is Nothing Then
										Throw New ArgumentException("Unknown building class """ & buildable.ClassName & """.")
									End If
								Else
									buildable.ClassType = GetType(Workshop)
								End If

								buildable.BuildingClass = CType(System.Enum.Parse(GetType(BuildingClass), buildable.ClassType.Name), BuildingClass)

								If buildable.ClassType Is GetType(Home) Then
									If (buildable.MigrantCount <= 0) OrElse (buildable.ResourceStacks.Count <> 1) OrElse (buildable.ResourceStacks(0).Direction <> StackType.Provider) Then
										Throw New ArgumentException("Given configuration does not describe a house.")
									End If
								End If

								If GetType(Business).IsAssignableFrom(buildable.ClassType) Then
									' check for spawn point
									If Not(buildable.ResourceStacks.Any(Function(e) e.Type = Resource.Max)) Then
										Throw New ArgumentException("Buildable """ & buildable.Name & """ derives from ""Worker"" but has no spawn point!")
									End If
								End If

								'                    
								'                     * We need to compute the ZMargin, which indicates the value subtracted from
								'                     * the usual ZValue of the rendered visual. This is on of the required steps for 3D
								'                     * emulation. 
								'                     
								buildable.GroundPlane = mChar.GroundPlane
								buildable.ReservedPlane = mChar.ReservedPlane

								buildable.GridWidth = Math.Max(mChar.GroundPlane.Max(Function(e) e.X + e.Width), mChar.ReservedPlane.Max(Function(e) e.X + e.Width))
								buildable.GridHeight = Math.Max(mChar.GroundPlane.Max(Function(e) e.Y + e.Height), mChar.ReservedPlane.Max(Function(e) e.Y + e.Height))

								If buildable.ResourceStacks.Count <> mChar.ResourceStacks.Count Then
									Throw New ArgumentException("Configured resource stack count differs from count in animation library.")
								End If


								' postprocess resource stacks
								Dim availOffsets(CInt(Resource.Max)) As Integer

								For i As Integer = 0 To buildable.ResourceStacks.Count - 1
									Dim stackCfg As ResourceStack = buildable.ResourceStacks(i)
									Dim availDefs() As ResourceStackEntry = ( _
									    From e In mChar.ResourceStacks _
									    Where e.Resource = stackCfg.Type _
									    Select e).ToArray()

									Dim stackEntry As ResourceStackEntry = availDefs(availOffsets(Convert.ToInt32(CInt(stackCfg.Type))))
									availOffsets(CInt(stackCfg.Type)) += 1

									stackCfg.MaxStackSize = If(stackCfg.MaxStackSize = 0, GenericResourceStack.DEFAULT_STACK_SIZE, stackCfg.MaxStackSize)

									' convert pixel offsets to grid cell space
									stackCfg.Position = stackEntry.Position

									If stackCfg.TimeOffset = 0 Then
									End If

									' make sure resource spot is in reserved area
									buildable.ReservedPlane.Add(New Rectangle(stackCfg.Position.X, stackCfg.Position.Y, 1, 1))

									'                        
									'                         * Configuration time is in millis, but we need cycle time as well as boundary
									'                         * alignment. See BuildingManager.MillisToCycleBoundary()
									'                         
									stackCfg.TimeOffset = SynchronizedManager.MillisToAlignedCycle(stackCfg.TimeOffset)

									If stackCfg.Direction = StackType.Query Then
										buildable.ProductionAnimTime = Math.Max(buildable.ProductionAnimTime, stackCfg.TimeOffset)
									End If
								Next i

								' save spots for building resources
								Dim mid As Integer = buildable.GridWidth \ 2

								buildable.StoneSpot = New Point(mid - 1, buildable.GridHeight)
								buildable.TimberSpot = New Point(mid + 1, buildable.GridHeight)

								buildable.ReservedPlane.Add(New Rectangle(buildable.StoneSpot.X, buildable.StoneSpot.Y, 1, 1))
								buildable.ReservedPlane.Add(New Rectangle(buildable.TimberSpot.X, buildable.TimberSpot.Y, 1, 1))

								' same as for "stackCfg.TimeOffset"
								buildable.ProductionTimeMillis = SynchronizedManager.MillisToAlignedCycle(buildable.ProductionTimeMillis)

								' compute ground grid
								buildable.GroundGrid = New Boolean(mChar.GroundPlane.Max(Function(e) e.X + e.Width) - 1)(){}

								Dim x As Integer = 0
								Dim yCount As Integer = mChar.GroundPlane.Max(Function(e) e.Y + e.Height)
								Do While x < buildable.GroundGrid.Length
									buildable.GroundGrid(x) = New Boolean(yCount - 1){}
									x += 1
								Loop

								For Each rect As Rectangle In buildable.GroundPlane
									For x1 As Integer = rect.Left To rect.Right - 1
										For y As Integer = rect.Top To rect.Bottom - 1
											If (x1 < 0) OrElse (y < 0) Then
												Continue For
											End If

											buildable.GroundGrid(x1)(y) = True
										Next y
									Next x1
								Next rect

								' there must be at least one line with all bits set to "true"
								Dim yGroundLine As Integer = -1

								For y As Integer = 0 To buildable.GroundGrid(0).Length - 1
									Dim yLoop As Integer = y
									If buildable.GroundGrid.All(Function(e) e(yLoop)) Then
										yGroundLine = y

										Exit For
									End If
								Next y

								If yGroundLine < 0 Then
									Throw New ArgumentException("Building """ & buildable.Name & """ has no ground line.")
								End If

								' compute reserved grid
								buildable.ReservedGrid = New Boolean(mChar.ReservedPlane.Max(Function(e) e.X + e.Width) - 1)(){}

								x = 0
								yCount = mChar.ReservedPlane.Max(Function(e) e.Y + e.Height)
								Do While x < buildable.ReservedGrid.Length
									buildable.ReservedGrid(x) = New Boolean(yCount - 1){}
									x += 1
								Loop

								For Each rect As Rectangle In buildable.ReservedPlane
									For x1 As Integer = rect.Left To rect.Right - 1
										For y As Integer = rect.Top To rect.Bottom - 1
											If (x1 < 0) OrElse (y < 0) Then
												Continue For
											End If

											buildable.ReservedGrid(x1)(y) = True
										Next y
									Next x1
								Next rect

								' compute constructor spots
								buildable.ConstructorSpots = New List(Of Point)()

								x = 0
								Dim height As Integer = buildable.GroundGrid(0).Length
								Do While x < buildable.GroundGrid.Length
									' start from ground line and search downwards for first non blocked spot
									Dim y As Integer = yGroundLine

									Do While y < height
										If Not(buildable.GroundGrid(x)(y)) Then
											' we found a reserved cell for constructor
											Exit Do
										End If
										y += 1
									Loop

									If y >= buildable.GridHeight Then
										Throw New ArgumentException("Building """ & buildable.Name & """ is not fully surrounded by reserved cells!")
									End If

									buildable.ConstructorSpots.Add(New Point(x, y))
									x += 1
								Loop

								'///////////////////////////////////////////////////////////////////////////////////////////////
								'///////////////////////// Binary Serialization...
								'///////////////////////////////////////////////////////////////////////////////////////////////
								writer.Write(CByte(buildable.BuildingClass))

								If buildable.ClassParameter Is Nothing Then
									writer.Write(CByte(0))
								Else
									writer.Write(CByte(Encoding.ASCII.GetByteCount(buildable.ClassParameter)))
									writer.Write(Encoding.ASCII.GetBytes(buildable.ClassParameter))
								End If

								writer.Write(CByte(buildable.ConstructorSpots.Count))

								For Each p As Point In buildable.ConstructorSpots
									writer.Write(CByte(p.X))
									writer.Write(CByte(p.Y))
								Next p

								writer.Write(CByte(buildable.DamageResistance))
								writer.Write(CByte(buildable.GridHeight))
								writer.Write(CByte(buildable.GridWidth))

								writer.Write(CByte(buildable.GroundGrid.Length))
								writer.Write(CByte(buildable.GroundGrid(0).Length))

								For x1 As Integer = 0 To buildable.GroundGrid.Length - 1
									For y As Integer = 0 To buildable.GroundGrid(0).Length - 1
										writer.Write(CByte((If(buildable.GroundGrid(x1)(y), 1, 0))))
									Next y
								Next x1

								writer.Write(CByte(Encoding.ASCII.GetByteCount(buildable.Name)))
								writer.Write(Encoding.ASCII.GetBytes(buildable.Name))
								writer.Write(CByte(buildable.ProductionAnimTime))
								writer.Write(CByte(buildable.ProductionTimeMillis))

								writer.Write(CByte(buildable.ReservedGrid.Length))
								writer.Write(CByte(buildable.ReservedGrid(0).Length))

								For x1 As Integer = 0 To buildable.ReservedGrid.Length - 1
									For y As Integer = 0 To buildable.ReservedGrid(0).Length - 1
										writer.Write(CByte((If(buildable.ReservedGrid(x1)(y), 1, 0))))
									Next y
								Next x1

								writer.Write(CByte(buildable.ResourceStacks.Count))
								For Each Stack As ResourceStack In buildable.ResourceStacks
									writer.Write(CByte(Stack.CycleCount))
									writer.Write(CByte(Stack.Direction))
									writer.Write(CByte(Stack.MaxStackSize))
									writer.Write(CByte(Stack.Position.X))
									writer.Write(CByte(Stack.Position.Y))
									writer.Write(CByte(Stack.QualityIndex))
									writer.Write(CByte(Stack.TimeOffset))
									writer.Write(CByte(Stack.Type))
								Next Stack

								writer.Write(CByte(buildable.MigrantCount))
								writer.Write(CByte(buildable.StoneCount))
								writer.Write(CByte(buildable.StoneSpot.X))
								writer.Write(CByte(buildable.StoneSpot.Y))
								writer.Write(CByte(buildable.TabIndex))
								writer.Write(CByte(buildable.TimberSpot.X))
								writer.Write(CByte(buildable.TimberSpot.Y))
								writer.Write(CByte(buildable.TypeIndex))
								writer.Write(CByte(buildable.WoodCount))
								writer.Write(CByte(buildable.Worker))

							Catch e As Exception
								Throw New ArgumentException("Building """ & buildable.Name & """ could not be imported (see inner exception for details).", e)
							End Try
						Next buildable
					End Using

					File.WriteAllBytes(inXMLPath & ".compiled", buffer.ToArray())
				End Using

				Return result
			End Using
		End Function

		Private Shared Function FindNearestFreeSpot(ByVal reservedArg(,) As Boolean, ByVal posX As Integer, ByVal posY As Integer) As Point
			result = New Point(0, 0)
			reserved = reservedArg

			If WalkResult.NotFound = GridSearch.GridWalkAround(New Point(posX, posY), reserved.GetLength(0), reserved.GetLength(1), AddressOf GetWalkResult) Then

				Throw New ArgumentException()
			End If

			Return result
		End Function

		Private Shared result As Point
		Private Shared reserved(,) As Boolean

		''' <summary>
		''' Answers the question is movable allowed to move to a certain point
		''' </summary>
		''' <param name="entry"></param>
		''' <returns></returns>
		''' <remarks></remarks>
		Private Shared Function GetWalkResult(ByVal entry As Point) As WalkResult
			If reserved(entry.X, entry.Y) Then
				Return WalkResult.NotFound
			End If
			result = entry
			Return WalkResult.Success
		End Function
	End Class
End Namespace
