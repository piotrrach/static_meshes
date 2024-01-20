using Unity.Entities;

namespace StaticMeshes
{
    /// <summary>
    /// All of the entities with the SpawnedTag will be managed (destroyed mostly) by the SpawnSystem.
    /// </summary>
    public struct SpawnedTag : IComponentData { }
}
