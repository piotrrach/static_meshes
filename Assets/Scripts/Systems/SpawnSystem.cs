using System;
using System.Linq;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

namespace StaticMeshes
{
    [BurstCompile]
    public partial class SpawnSystem : SystemBase
    {
        public static SpawnSystem Instance;

        private Mesh _mesh;
        private Material _material;

        private Mesh _oldMesh;
        private Material _oldMaterial;

        private bool _shouldSpawnAgents;

        private float3[] _grid;
        private Entity _prototype;

        protected override void OnCreate()
        {
            RequireForUpdate<SpawnProperties>();
            Instance = this;
            _shouldSpawnAgents = true;
        }

        protected override void OnDestroy()
        {

        }

        public void UpdateSettings(SpawnerSettings spawnerSettings)
        {
            _shouldSpawnAgents = true;
            var spawnEntity = SystemAPI.GetSingletonEntity<SpawnProperties>();
            var spawnableEntity = SystemAPI.GetSingleton<SpawnProperties>().SpawnablePrefab;
            SystemAPI.SetComponent(spawnEntity, new SpawnProperties
            {
                SpawnablePrefab = spawnableEntity,
                Quantity = spawnerSettings.ObjectsCount,
                Radius = spawnerSettings.SphereRadius
            });
            _mesh = spawnerSettings.Mesh;
            _material = spawnerSettings.Material;
            _grid = spawnerSettings.SphericalGridData.Grid;

        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_material == null || _mesh == null) return;

            if (!_shouldSpawnAgents)
            {
                return;
            }
            _shouldSpawnAgents = false;

            var spawnEntity = SystemAPI.GetSingletonEntity<SpawnProperties>();
            var spawnAspect = SystemAPI.GetAspect<SpawnAspect>(spawnEntity);

            DestroyLeftOvers();
            SpawnEntities(ref spawnAspect);
        }

        [BurstCompile]
        private void DestroyLeftOvers()
        {
            var spawnablesToClear = SystemAPI.QueryBuilder().WithAll<SpawnedTag>().Build();

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);

            ecb.DestroyEntity(spawnablesToClear, EntityQueryCaptureMode.AtRecord);
        }

        [BurstCompile]
        private void SpawnEntities(ref SpawnAspect spawnAspect)
        {
            float sphereRadius = spawnAspect.SphereRadius;
            int meshesToSpawn = spawnAspect.MeshesToSpawn;
            if (meshesToSpawn <= 0)
            {
                return;
            }

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            //Creating entity with RenderMeshUtility.AddComponents is huge structural change so it must happen ONLY when user changes mat or mesh.
            //Otherwise enjoy unity crash everytime while messing in editor:) 
            if (_mesh != _oldMesh || _material != _oldMaterial)
            {
                _prototype = CreatePrototypeEntity();
                _oldMesh = _mesh;
                _oldMaterial = _material;
            }

            if (meshesToSpawn >= _grid.Length)
            {
                Debug.LogError("Tried to spawn more elements that are positions for them in the spherical grid data asset");
                return;
            }

            float farthestDistance = math.length(_grid[meshesToSpawn-1]);
            float distanceMod = sphereRadius / farthestDistance;

            for(int i = 0; i < meshesToSpawn; i++)
            {
                new SpawnJob
                {
                    ECB = ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter(),
                    Prototype = _prototype,
                    Position = _grid[i] * distanceMod
                }.ScheduleParallel();
            }

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
    public partial struct SpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public Entity Prototype;
        public float3 Position;

        [BurstCompile]
        private void Execute(SpawnAspect spawnAspect, [ChunkIndexInQuery] int sortKey)
        {
            // Clone the Prototype entity to create a new entity.
            var newEntity = ECB.Instantiate(sortKey, Prototype);
            // Prototype has all correct components up front, can use SetComponent to
            // set values unique to the newly created entity, such as the transform.
            //ECB.SetComponent(sortKey, newEntity, new LocalToWorld { Value = float4x4.Translate(Position) });
            ECB.SetComponent(sortKey, newEntity, new LocalTransform()
            {
                Position = Position,//GetSpawnPoint(Index),//aspect.GetSpawnPoint(Index),
                Rotation = Unity.Mathematics.quaternion.identity,
                Scale = 1f
            });

            ECB.AddComponent(sortKey, newEntity, new SpawnedTag());
        }
    }
}

