using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace StaticMeshes
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class SpawnSystem : SystemBase
    {
        public static SpawnSystem Instance;

        private Mesh _mesh;
        private Material _material;
        private int _objectsCountToSpawn;
        private float _spawningSphereRadius;

        private Mesh _oldMesh;
        private Material _oldMaterial;

        private float3[] _grid;
        private Entity _prototype;

        protected override void OnCreate()
        {
            //Debug.Log("Spawn System Created");
            //DefaultWorldInitialization.DefaultLazyEditModeInitialize();
            Instance = this;
            //_shouldSpawnAgents = true;
        }

        protected override void OnDestroy()
        {

        }

        public void UpdateSettings(SpawnerSettings spawnerSettings)
        {
            _objectsCountToSpawn = spawnerSettings.ObjectsCount;
            _spawningSphereRadius = spawnerSettings.SphereRadius;
            _mesh = spawnerSettings.Mesh;
            _material = spawnerSettings.Material;
            _grid = spawnerSettings.SphericalGridData.Grid;

            if (_material == null || _mesh == null) return;

            DestroyLeftOvers();
            SpawnEntities();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {

        }

        [BurstCompile]
        private void DestroyLeftOvers()
        {
            var spawnablesToClear = SystemAPI.QueryBuilder().WithAll<SpawnedTag>().Build();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            ecb.DestroyEntity(spawnablesToClear, EntityQueryCaptureMode.AtPlayback);
            ecb.Playback(World.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        private void SpawnEntities()
        {
            if (_objectsCountToSpawn <= 0)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            //Creating entity with RenderMeshUtility.AddComponents is huge structural change so it must happen ONLY when user changes mat or mesh.
            //Otherwise enjoy unity crash everytime while messing in editor:) 
            if (_prototype == null || _mesh != _oldMesh || _material != _oldMaterial)
            {
                _prototype = CreatePrototypeEntity();
                _oldMesh = _mesh;
                _oldMaterial = _material;
            }

            if (_objectsCountToSpawn >= _grid.Length)
            {
                Debug.LogError("Tried to spawn more elements that are positions for them in the spherical grid data asset");
                return;
            }


            NativeArray<float3> gridPositions =
                CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(_objectsCountToSpawn, ref World.UpdateAllocator);

            for (int i = 0; i < _objectsCountToSpawn; i++)
            {
                gridPositions[i] = _grid[i];
            }

            float farthestDistance = math.length(_grid[_objectsCountToSpawn - 1]);
            float distanceMod = _spawningSphereRadius / farthestDistance;

            var spawnJob = new SpawnJob
            {
                ECB = ecb.AsParallelWriter(),
                Prototype = _prototype,
                DistanceMod = distanceMod,
                Grid = gridPositions
            };

            var jobHandle = spawnJob.Schedule(_objectsCountToSpawn, 128);
            jobHandle.Complete();

            ecb.Playback(World.EntityManager);
            ecb.Dispose();
        }

        private Entity CreatePrototypeEntity()
        {
            var desc = new RenderMeshDescription(UnityEngine.Rendering.ShadowCastingMode.On, receiveShadows: true);

            var renderMeshArray = new RenderMeshArray(new Material[] { _material }, new Mesh[] { _mesh });

            var entity = World.EntityManager.CreateEntity();

            RenderMeshUtility.AddComponents(
                entity,
                World.EntityManager,
                desc,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
            World.EntityManager.AddComponentData(entity, new LocalTransform());

            return entity;
        }
    }

    [BurstCompile]
    public partial struct SpawnJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public Entity Prototype;
        public float DistanceMod;
        [ReadOnly] public NativeArray<float3> Grid;

        [BurstCompile]
        public void Execute(int index)
        {
            // Clone the Prototype entity to create a new entity.
            var newEntity = ECB.Instantiate(index, Prototype);
            // Prototype has all correct components up front, can use SetComponent to
            // set values unique to the newly created entity, such as the transform.
            ECB.SetComponent(index, newEntity, new LocalTransform()
            {
                Position = Grid[index] * DistanceMod,//GetSpawnPoint(Index),//aspect.GetSpawnPoint(Index),
                Rotation = Unity.Mathematics.quaternion.identity,
                Scale = 1
            });

            ECB.AddComponent(index, newEntity, new SpawnedTag());
        }
    }
}

