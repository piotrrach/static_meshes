//using JetBrains.Annotations;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;

//namespace StaticMeshes.Assets.Scripts.Systems
//{
//    [BurstCompile]
//    public partial struct CreateSpawnPointsAssetsSystem : ISystem
//    {
//        private int _maxNumberOfSpawnPoints;

//        [BurstCompile]
//        public void OnCreate(ref SystemState state)
//        {
//            _maxNumberOfSpawnPoints = 25000;
//            state.RequireForUpdate<SpawnProperties>();
//        }

//        [BurstCompile]
//        public void OnUpdate(ref SystemState state)
//        {
//            state.Enabled = false;

//            var fieldEntity = SystemAPI.GetSingletonEntity<SpawnRandom>();

//            var ecb = new EntityCommandBuffer(Allocator.Temp);

//            var builder = new BlobBuilder(Allocator.Temp);
//            ref var spawnPoints = ref builder.ConstructRoot<SpawnPointsBlob>();
//            var arrayBuilder = builder.Allocate(ref spawnPoints.Value, _maxNumberOfSpawnPoints);

//            float golden_angle = math.PI * (3 - math.sqrt(5));
//            float spreadDistanceFactor = 80;

//            float maxNumberOfSpawnPointsSQ = math.sqrt(_maxNumberOfSpawnPoints);
//            for (int i = 0; i <= _maxNumberOfSpawnPoints; i++)
//            {
//                if(i == 0)
//                {
//                    continue;
//                }

//                float theta = i * golden_angle;
//                float r = math.sqrt(i) / maxNumberOfSpawnPointsSQ;
//                float3 spawnPoint = new float3()
//                {
//                    x = r * math.cos(theta),
//                    y = r * math.sin(theta),
//                    z = r * math.cos(theta * 1.37f) 
//                };
//                //float3 secondSpawnPoint = new float3()
//                //{
//                //    x = r * math.cos(theta),
//                //    y = r * math.sin(theta),
//                //    z = 0
//                //};
//                arrayBuilder[i-1] = (spawnPoint) * spreadDistanceFactor;
//            }

//            var blobAsset = builder.CreateBlobAssetReference<SpawnPointsBlob>(Allocator.Persistent);
//            ecb.SetComponent(fieldEntity, new SpawnPoints { Value = blobAsset });
//            builder.Dispose();

//            ecb.Playback(state.EntityManager);
//        }
//    }

//}
