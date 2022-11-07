Imports System.IO
Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core

Namespace Migration.Game

	Friend Class MapFile
		Implements IDisposable

		Friend Shared Function OpenWrite(ByVal inFileName As String, ByVal inDeriveFrom As MapFile) As MapFile
			Return New MapFile(inFileName, False)
		End Function

		Friend Shared Function OpenRead(ByVal inFileName As String) As MapFile
			Return New MapFile(inFileName, True)
		End Function

		Private m_Stream As FileStream
		Private m_Reader As BinaryReader
		Private m_Writer As BinaryWriter
		Private m_IsReadMode As Boolean
		Private ReadOnly m_Lock As New Object()

		Private privateNextCycle As Int64
		Friend Property NextCycle() As Int64
			Get
				Return privateNextCycle
			End Get
			Private Set(ByVal value As Int64)
				privateNextCycle = value
			End Set
		End Property

		Private privateFileName As String
		Friend Property FileName() As String
			Get
				Return privateFileName
			End Get
			Private Set(ByVal value As String)
				privateFileName = value
			End Set
		End Property

		Public Sub Dispose() Implements IDisposable.Dispose
			If m_Stream IsNot Nothing Then
				m_Stream.Dispose()
			End If

			NextCycle = -1
			FileName = Nothing
			m_Stream = Nothing
			m_Reader = Nothing
			m_Writer = Nothing
		End Sub

		Private Sub New(ByVal inFileName As String, ByVal inIsReadMode As Boolean)
			m_IsReadMode = inIsReadMode
			FileName = Path.GetFullPath(inFileName)

			If m_IsReadMode Then
				m_Stream = New FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan)
				m_Reader = New BinaryReader(m_Stream)

				If m_Stream.Length > 0 Then
					NextCycle = m_Reader.ReadInt64()
				Else
					NextCycle = -1
				End If
			Else
				m_Stream = New FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Read, 1024 * 1024, FileOptions.WriteThrough)
				m_Writer = New BinaryWriter(m_Stream)
			End If
		End Sub

		Private Sub ForceWriteMode()
			If m_IsReadMode Then
				Throw New InvalidOperationException("Map file is not in write mode!")
			End If
		End Sub

		Friend Sub AddBuilding(ByVal inCycleTime As Int64, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			SyncLock m_Lock
				ForceWriteMode()

				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.AddBuilding))
				m_Writer.Write(Convert.ToInt16((inConfig.TypeIndex)))
				m_Writer.Write(Convert.ToInt16((inPosition.X)))
				m_Writer.Write(Convert.ToInt16((inPosition.Y)))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub DropResource(ByVal inCycleTime As Int64, ByVal inAround As Point, ByVal inResource As Resource, ByVal inCount As Integer)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.DropResource))
				m_Writer.Write(Convert.ToInt16((inAround.X)))
				m_Writer.Write(Convert.ToInt16((inAround.Y)))
				m_Writer.Write(Convert.ToByte(inResource))
				m_Writer.Write(Convert.ToByte(inCount))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub AddMovable(ByVal inCycleTime As Int64, ByVal inPosition As CyclePoint, ByVal inMovable As MovableType)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.AddMovable))
				m_Writer.Write(Convert.ToInt64((inPosition.XCycles)))
				m_Writer.Write(Convert.ToInt64((inPosition.YCycles)))
				m_Writer.Write(Convert.ToInt16((inMovable)))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub ChangeToolSetting(ByVal inCycleTime As Int64, ByVal inTool As Resource, ByVal inNewTodo As Integer, ByVal inNewPercentage As Double)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.ChangeToolSetting))
				m_Writer.Write(Convert.ToByte(inTool))
				m_Writer.Write(Convert.ToInt32(CInt(inNewTodo)))
				m_Writer.Write(Convert.ToDouble(inNewPercentage))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub ChangeDistributionSetting(ByVal inCycleTime As Int64, ByVal inBuildingClass As String, ByVal inResource As Resource, ByVal inIsEnabled As Boolean)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.ChangeDistributionSetting))
				m_Writer.Write(Convert.ToString(inBuildingClass))
				m_Writer.Write(Convert.ToByte(inResource))
				m_Writer.Write(Convert.ToBoolean(inIsEnabled))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub ChangeProfession(ByVal inCycleTime As Int64, ByVal inProfession As MigrantProfessions, ByVal inDelta As Integer)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.ChangeProfession))
				m_Writer.Write(Convert.ToByte(inProfession))
				m_Writer.Write(Convert.ToSByte(inDelta))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub AddMarketTransport(ByVal inCycleTime As Int64, ByVal inBuildingUniqueID As Long, ByVal inResource As Resource)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.AddMarketTransport))
				m_Writer.Write(Convert.ToInt64((inBuildingUniqueID)))
				m_Writer.Write(Convert.ToByte(inResource))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub RemoveMarketTransport(ByVal inCycleTime As Int64, ByVal inBuildingUniqueID As Long, ByVal inResource As Resource)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.RemoveMarketTransport))
				m_Writer.Write(Convert.ToInt64((inBuildingUniqueID)))
				m_Writer.Write(Convert.ToByte(inResource))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub SetTaskSuspended(ByVal inCycleTime As Int64, ByVal inTaskID As Long, ByVal isSuspended As Boolean)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.SetTaskSuspended))
				m_Writer.Write(Convert.ToInt64((inTaskID)))
				m_Writer.Write(Convert.ToBoolean(isSuspended))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub RemoveBuildTask(ByVal inCycleTime As Int64, ByVal inTaskID As Long)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.RemoveBuildTask))
				m_Writer.Write(Convert.ToInt64((inTaskID)))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub RaiseTaskPriority(ByVal inCycleTime As Int64, ByVal inTaskID As Long)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.RaiseTaskPriority))
				m_Writer.Write(Convert.ToInt64((inTaskID)))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub RemoveBuilding(ByVal inCycleTime As Int64, ByVal inBuildingID As Long)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.RemoveBuilding))
				m_Writer.Write(Convert.ToInt64((inBuildingID)))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub RaiseBuildingPriority(ByVal inCycleTime As Int64, ByVal inBuildingID As Long)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.RaiseBuildingPriority))
				m_Writer.Write(Convert.ToInt64((inBuildingID)))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub LowerBuildingPriority(ByVal inCycleTime As Int64, ByVal inBuildingID As Long)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.LowerBuildingPriority))
				m_Writer.Write(Convert.ToInt64((inBuildingID)))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub SetBuildingSuspended(ByVal inCycleTime As Int64, ByVal inBuildingID As Long, ByVal isSuspended As Boolean)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.SetBuildingSuspended))
				m_Writer.Write(Convert.ToInt64((inBuildingID)))
				m_Writer.Write(Convert.ToBoolean(isSuspended))
				m_Writer.Flush()
			End SyncLock
		End Sub

		Friend Sub SetWorkingArea(ByVal inCycleTime As Int64, ByVal inBuildingID As Long, ByVal inWorkingCenter As Point)
			SyncLock m_Lock
				m_Writer.Write(Convert.ToInt64((inCycleTime)))
				m_Writer.Write(Convert.ToByte(GameMapCommand.SetWorkingArea))
				m_Writer.Write(Convert.ToInt64((inBuildingID)))
				m_Writer.Write(Convert.ToInt32(CInt(inWorkingCenter.X)))
				m_Writer.Write(Convert.ToInt32(CInt(inWorkingCenter.Y)))
				m_Writer.Flush()
			End SyncLock
		End Sub

		''' <summary>
		''' Read the next stored command, executes it for the given game map and returns
		''' the cycle time for the next command. This method should be used in a while loop
		''' until the return value is higher to the current cycle time...
		''' </summary>
		''' <param name="inTarget"></param>
		Friend Sub ReadNext(ByVal inTarget As Map)
			SyncLock m_Lock
				If Not m_IsReadMode Then
					Throw New InvalidOperationException("Map file is not in read mode!")
				End If

				If m_Stream.Length = m_Stream.Position Then
					NextCycle = -1
				End If

				If NextCycle = -1 Then
					Return
				End If

				' read next command
				Dim mCommand As GameMapCommand = CType(m_Reader.ReadByte(), GameMapCommand)
				Select Case mCommand
					Case GameMapCommand.AddBuilding
							Dim buildingID As Integer = m_Reader.ReadInt16()
							Dim posX As Integer = m_Reader.ReadInt16()
							Dim posY As Integer = m_Reader.ReadInt16()

							inTarget.AddBuildingInternal(inTarget.ResolveBuildingTypeIndex(buildingID), New Point(posX, posY))

							Exit Select
					Case GameMapCommand.DropResource
								Dim posX As Integer = m_Reader.ReadInt16()
								Dim posY As Integer = m_Reader.ReadInt16()
								Dim res As Resource = CType(m_Reader.ReadByte(), Resource)
								Dim count As Integer = m_Reader.ReadByte()

								inTarget.DropResourceInternal(New Point(posX, posY), res, count)

								Exit Select
					Case GameMapCommand.AddMovable
									Dim xCycles As Int64 = m_Reader.ReadInt64()
									Dim yCycles As Int64 = m_Reader.ReadInt64()

									inTarget.AddMovableInternal(New CyclePoint(xCycles, yCycles), CType(m_Reader.ReadInt16(), MovableType))

									Exit Select
					Case GameMapCommand.ChangeDistributionSetting
										Dim inBuildingClass As String = m_Reader.ReadString()
										Dim inResource As Resource = CType(m_Reader.ReadByte(), Resource)
										Dim inIsEnabled As Boolean = m_Reader.ReadBoolean()

										inTarget.ChangeDistributionSettingInternal(inBuildingClass, inResource, inIsEnabled)

										Exit Select
					Case GameMapCommand.ChangeProfession
											Dim inProfession As MigrantProfessions = CType(m_Reader.ReadByte(), MigrantProfessions)
											Dim inDelta As Integer = m_Reader.ReadSByte()

											inTarget.ChangeProfessionInternal(inProfession, inDelta)

											Exit Select
					Case GameMapCommand.ChangeToolSetting
												Dim inTool As Resource = CType(m_Reader.ReadByte(), Resource)
												Dim inNewTodo As Int32 = m_Reader.ReadInt32()
												Dim inNewPercentage As Double = m_Reader.ReadDouble()

												inTarget.ChangeToolSettingInternal(inTool, inNewTodo, inNewPercentage)

												Exit Select
					Case GameMapCommand.AddMarketTransport
													Dim inBuildingUniqueID As Int64 = m_Reader.ReadInt64()
													Dim inResource As Resource = CType(m_Reader.ReadByte(), Resource)

													inTarget.AddMarketTransportInternal(inBuildingUniqueID, inResource)

													Exit Select
					Case GameMapCommand.RemoveMarketTransport
														Dim inBuildingUniqueID As Int64 = m_Reader.ReadInt64()
														Dim inResource As Resource = CType(m_Reader.ReadByte(), Resource)

														inTarget.RemoveMarketTransportInternal(inBuildingUniqueID, inResource)

														Exit Select
					Case GameMapCommand.RaiseBuildingPriority
															Dim inBuildingUniqueID As Int64 = m_Reader.ReadInt64()

															inTarget.RaiseBuildingPriorityInternal(inBuildingUniqueID)

															Exit Select
					Case GameMapCommand.LowerBuildingPriority
																Dim inBuildingUniqueID As Int64 = m_Reader.ReadInt64()

																inTarget.LowerBuildingPriorityInternal(inBuildingUniqueID)

																Exit Select
					Case GameMapCommand.RemoveBuilding
																	Dim inBuildingUniqueID As Int64 = m_Reader.ReadInt64()

																	inTarget.RemoveBuildingInternal(inBuildingUniqueID)

																	Exit Select
					Case GameMapCommand.SetBuildingSuspended
																		Dim inBuildingUniqueID As Int64 = m_Reader.ReadInt64()
																		Dim isSuspended As Boolean = m_Reader.ReadBoolean()

																		inTarget.SetBuildingSuspendedInternal(inBuildingUniqueID, isSuspended)

																		Exit Select
					Case GameMapCommand.RaiseTaskPriority
																			Dim inTaskUniqueID As Int64 = m_Reader.ReadInt64()

																			inTarget.RaiseTaskPriorityInternal(inTaskUniqueID)

																			Exit Select
					Case GameMapCommand.RemoveBuildTask
																				Dim inTaskUniqueID As Int64 = m_Reader.ReadInt64()

																				inTarget.RemoveBuildTaskInternal(inTaskUniqueID)

																				Exit Select
					Case GameMapCommand.SetTaskSuspended
																					Dim inTaskUniqueID As Int64 = m_Reader.ReadInt64()
																					Dim isSuspended As Boolean = m_Reader.ReadBoolean()

																					inTarget.SetTaskSuspendedInternal(inTaskUniqueID, isSuspended)

																					Exit Select
					Case GameMapCommand.SetWorkingArea
																						Dim inBuildingID As Int64 = m_Reader.ReadInt64()
																						Dim posX As Int32 = m_Reader.ReadInt32()
																						Dim posY As Int32 = m_Reader.ReadInt32()

																						inTarget.SetWorkingAreaInternal(inBuildingID, New Point(posX, posY))

																						Exit Select
					Case GameMapCommand.Idle
																							' just for time synchronization...

																							Exit Select
					Case Else
																								Throw New ArgumentException("Given game file """ & FileName & """ contains unknown commands.")
				End Select

				If m_Stream.Length = m_Stream.Position Then
					NextCycle = -1

					Return
				End If

				Dim nextTime As Long = m_Reader.ReadInt64()

				If nextTime < NextCycle Then
					Throw New InvalidDataException()
				End If

				NextCycle = nextTime
			End SyncLock
		End Sub

		Friend Sub Clear()
			SyncLock m_Lock
				m_Stream.SetLength(0)
				m_Stream.Flush()
			End SyncLock
		End Sub

		Friend Sub Fork(ByVal inCycleTime As Int64, ByVal inFileName As String)
			SyncLock m_Lock
				m_Stream.Flush()

				File.Delete(inFileName)
				File.Copy(FileName, inFileName)
			End SyncLock


			Using stream As Stream = File.OpenWrite(inFileName)
				stream.Position = stream.Length

				Dim writer As New BinaryWriter(stream)

				writer.Write(Convert.ToInt64((inCycleTime)))
				writer.Write(Convert.ToByte(GameMapCommand.Idle))
				writer.Flush()
			End Using
		End Sub
	End Class
End Namespace
