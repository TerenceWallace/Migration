Namespace Migration
	Friend MustInherit Class AbstractTerrainGenerator
		Protected m_Random As CrossRandom

		Public ReadOnly Property Terrain() As TerrainDefinition
			Get
				Return Map.Terrain
			End Get
		End Property

		Public ReadOnly Property ResMgr() As ResourceManager
			Get
				Return Map.ResourceManager
			End Get
		End Property

		Private privateMap As Migration.Game.Map
		Public Property Map() As Migration.Game.Map
			Get
				Return privateMap
			End Get
			Private Set(ByVal value As Migration.Game.Map)
				privateMap = value
			End Set
		End Property

		Public ReadOnly Property Size() As Int32
			Get
				Return Map.Size
			End Get
		End Property

		Private privateSeed As Int64
		Public Property Seed() As Int64
			Get
				Return privateSeed
			End Get
			Private Set(ByVal value As Int64)
				privateSeed = value
			End Set
		End Property

		Protected Sub New(ByVal inMap As Migration.Game.Map, ByVal inSeed As Long)
			If inMap Is Nothing Then
				Throw New ArgumentNullException()
			End If

			m_Random = New CrossRandom(Convert.ToInt32(CInt(inSeed)))
			Map = inMap
			Seed = inSeed
		End Sub
	End Class

End Namespace
