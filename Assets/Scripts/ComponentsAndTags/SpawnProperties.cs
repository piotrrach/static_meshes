using Unity.Entities;
namespace StaticMeshes
{
    public struct SpawnProperties : IComponentData
    {
        public Entity SpawnablePrefab;
        public float Radius;
        public int Quantity;
        public int MaxQuantity;
        public float XYSpread;
        public float ZSpread;
    }
}
