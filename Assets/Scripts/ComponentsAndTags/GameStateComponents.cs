using Unity.Entities;

namespace StaticMeshes
{
    public struct GameStateTag : IComponentData { }

    public struct GameStartedTag : IComponentData {}

    public struct GameEndedTag : IComponentData { }
}
