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
    /// <summary>
    /// System responsible for entity creation.
    /// Even though it is not necessary to derive from the SystemBase or ISystem,
    /// it's just easier to implement if it's required to work both in the play mode and in the edit mode.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class SpawnSystem : SystemBase
    {
        private bool _rotationEnabled;
        private Mesh _mesh;
        private Material _material;
        private int _objectsCount;
        private float _sphereRadius;
        private float3[] _grid;

        private bool _oldRotationEnabled;
        private Mesh _oldMesh;
        private Material _oldMaterial;
        private int _oldObjectsCount;
        private float _oldSphereRadius;

        public void UpdateSettingsAndView(SpawnerSettings spawnerSettings)
        {
            if(spawnerSettings.SphericalGridData == null)
            {
                Debug.LogError("The Spherical Grid Data field in Spawner Settings must be set!");
                return;
            }

            _objectsCount = spawnerSettings.ObjectsCount;
            _sphereRadius = spawnerSettings.SphereRadius;
            _mesh = spawnerSettings.Mesh;
            _material = spawnerSettings.Material;
            _grid = spawnerSettings.SphericalGridData.Grid;
            _rotationEnabled = spawnerSettings.AnimateObjects;

            UpdateView();
        }

        [BurstCompile]
        protected override void OnCreate()
        {
            _oldRotationEnabled = false;
            _oldObjectsCount = -1;
            _oldSphereRadius = -math.INFINITY;
            _oldMaterial = null;
            _oldMesh = null;
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            //Left empty on purpose
        }

        [BurstCompile]
        private void UpdateView()
        {
            if (_objectsCount < 0)
            {
                return;
            }

            if (_objectsCount > _grid.Length)
            {
                Debug.LogError("Tried to spawn more elements that are positions for them in the spherical grid data asset");
                return;
            }

            if(_objectsCount == 0)
            {
                DestroyAlreadySpawned();
                return;
            }

            // Create native array and populate it with values from spawner settings asset.
            // It's required by a jobs system to operate only on value types;
            NativeArray<float3> nativeGrid = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(_objectsCount, ref World.UpdateAllocator);

            for (int i = 0; i < _objectsCount; i++)
            {
                nativeGrid[i] = _grid[i];
            }

            //Calculate distance modifier relative to the farthest entity. 
            //It ensures proper size of the sphere.
            float farthestDistance = math.length(_grid[_objectsCount-1]);
            float distanceMod = _sphereRadius / farthestDistance;


            // If almost any settings has changed there is need to recreate all entities.
            bool isRecreationNeeded = _objectsCount != _oldObjectsCount 
                || _oldMesh != _mesh 
                || _oldMaterial != _material 
                || _oldRotationEnabled != _rotationEnabled;
            if (isRecreationNeeded)
            {
                RecreateEntities(nativeGrid, distanceMod);
                _oldRotationEnabled = _rotationEnabled;
                _oldMesh = _mesh;
                _oldMaterial = _material;
                _oldObjectsCount = _objectsCount;
                return;
            }

            // If only the radius of the spawning sphere has changed,
            // it's enough to just change positions of previously spawned entities
            bool isRelocationNeeded = _oldSphereRadius != _sphereRadius;
            if (isRelocationNeeded)
            {
                RelocateEntities(nativeGrid, distanceMod);
                _oldSphereRadius = _sphereRadius;
            }
        }

        /// <summary>
        /// Destroy all spawned entities. It's cheap and is not creating a sync point thanks to destroying whole chunks of allocated memory.
        /// </summary>
        [BurstCompile]
        private void DestroyAlreadySpawned()
        {
            var spawnablesToClear = SystemAPI.QueryBuilder().WithAll<SpawnedTag>().Build();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            ecb.DestroyEntity(spawnablesToClear, EntityQueryCaptureMode.AtPlayback);
            ecb.Playback(World.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        private void RecreateEntities(NativeArray<float3> nativeGrid, float distanceMod)
        {
            // Destroy all of the entities that were already spawned.
            // As long it can seem to be costly to destroy all entities and spawn all again even with slightest change in settings,
            // it's not that bad, thanks to a way of entities are destroyed. 
            DestroyAlreadySpawned();

            //Create prototype entity, this entity will serve as a template for later generation of entities.
            Entity prototype = CreatePrototypeEntity();

            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            var spawnJob = new SpawnJob
            {
                ECB = ecb.AsParallelWriter(),
                Prototype = prototype,
                DistanceMod = distanceMod,
                Grid = nativeGrid
            };

            var spawnJobHandle = spawnJob.Schedule(_objectsCount, 128);
            spawnJobHandle.Complete();
            ecb.Playback(World.EntityManager);
            ecb.Dispose();
            World.EntityManager.DestroyEntity(prototype);
        }

        /// <summary>
        /// Creates entity destined to be used as the entity template for spawning jobs.
        /// </summary>
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
            World.EntityManager.AddComponent<SpawnedTag>(entity);
            if (_rotationEnabled)
            {
                World.EntityManager.AddComponent<RotationTag>(entity);
            }

            return entity;
        }

        /// <summary>
        /// Changes LocalTransform component of all of the spawned entities according to grid data and distance modifier.
        /// </summary>
        /// <param name="nativeGrid"></param>
        private void RelocateEntities(NativeArray<float3> nativeGrid, float distanceMod)
        {
            var spawnablesToClear = SystemAPI.QueryBuilder().WithAll<SpawnedTag>().Build();

            Entities.ForEach((Entity entity, int entityInQueryIndex, ref LocalTransform localToWorld, in SpawnedTag _) =>
            {
                localToWorld.Position = nativeGrid[entityInQueryIndex] * distanceMod;
            }).Schedule();
            this.CompleteDependency();
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
                Position = Grid[index] * DistanceMod,
                Rotation = Unity.Mathematics.quaternion.identity,
                Scale = 1
            });
        }
    }
}

