using StaticMeshes;
using System;
using Unity.Entities;
using UnityEngine;

namespace StaticMeshes
{
    public class SpawnMono : MonoBehaviour
    {
        public GameObject SpawnablePrefab;
        public SpawnerSettings SpawnerSettings;
    }

    public class SpawnBaker : Baker<SpawnMono>
    {
        public override void Bake(SpawnMono authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new SpawnProperties()
            {
                SpawnablePrefab = GetEntity(authoring.SpawnablePrefab, TransformUsageFlags.Dynamic),
                Quantity = authoring.SpawnerSettings.ObjectsCount,
                Radius = authoring.SpawnerSettings.SphereRadius,
            });
            //AddComponent(entity, new SpawnRandom
            //{
            //    Value = Unity.Mathematics.Random.CreateFromIndex(authoring.SpawnerSettings.Seed)
            //});
        }
    }
}