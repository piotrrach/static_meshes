using Unity.Entities;
using Unity.Mathematics;

namespace StaticMeshes
{
    public struct SpawnRandom : IComponentData
    {
        public Random Value;
    }
}
