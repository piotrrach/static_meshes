using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace StaticMeshes
{
    [BurstCompile]
    public partial struct RotateSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var elapsedTime = (float)SystemAPI.Time.ElapsedTime;

            new RotateJob
            {
                ElapsedTime = elapsedTime,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct RotateJob : IJobEntity
    {
        public float ElapsedTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        [BurstCompile]
        private void Execute(RefRW<LocalTransform> localTransform, in RotationTag _)
        {
            localTransform.ValueRW.Rotation = quaternion.Euler(new float3(0, 0, ElapsedTime));
        }
    }
}

