using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace StaticMeshes
{
    [BurstCompile]
    public partial class CameraControllerSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<SpawnProperties>();
        }

        protected override void OnUpdate()
        {
            //var fieldEntity = SystemAPI.GetSingletonEntity<SpawnProperties>();
            //var fieldAspect = SystemAPI.GetAspect<SpawnAspect>(fieldEntity);

            //var farthestAgentPosition = fieldAspect.GetFarthestAgentPosition();
            //var distanceToFarthest = math.distance(float3.zero, farthestAgentPosition) + 1f;
            //Camera.main.orthographicSize = distanceToFarthest;
        }
    }
}