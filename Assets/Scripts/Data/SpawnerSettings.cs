using StaticMeshes;
using System;
using UnityEngine;

public class SpawnerSettings : ScriptableObject
{
    public Mesh Mesh;
    public Material Material;
    public float SphereRadius = 20f;
    public int ObjectsCount = 1000;
    public SphericalGridData SphericalGridData;

    public event Action OnAnyValueChanged;

    public void OnValidate()
    {
        OnAnyValueChanged?.Invoke();

        if(SpawnSystem.Instance != null)
        {
            SpawnSystem.Instance.UpdateSettings(this);
        }
    }
}
