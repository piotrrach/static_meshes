using Unity.Entities;

namespace StaticMeshes
{
    public readonly partial struct SpawnAspect : IAspect
    {
        public readonly Entity Entity;
        private readonly RefRO<SpawnProperties> _properties;
        //private readonly RefRW<SpawnRandom> _random;

        public Entity MeshPrefab => _properties.ValueRO.SpawnablePrefab;
        public int MeshesToSpawn => _properties.ValueRO.Quantity;
        public int MaxMeshesToSpawn => _properties.ValueRO.MaxQuantity;
        public float SphereRadius => _properties.ValueRO.Radius;
    }
}

