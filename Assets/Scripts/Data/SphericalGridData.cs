using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

namespace StaticMeshes
{
    /// <summary>
    /// Represents a collection of points that were evenly distributed within the sphere;
    /// Points are sorted by distance to center of the sphere.
    /// </summary>
    [CreateAssetMenu]
    [BurstCompile]
    public class SphericalGridData : ScriptableObject
    {
        //a stands for radius or half of the side lenght of cube that was "sphericaled" or "inflated"
        private static readonly int _a = 20;

        [SerializeField, HideInInspector]
        private float3[] _grid;
        public float3[] Grid => _grid;
        public int Length => _grid.Length;

        [BurstCompile]
        public void BakeData()
        {
            int gridLenght = (int)math.pow(_a * 2 + 1, 3);
            _grid = new float3[gridLenght];
            int index = 0;
            for (int x = -_a; x <= _a; x++)
            {
                for (int y = -_a; y <= _a; y++)
                {
                    for (int z = -_a; z <= _a; z++)
                    {
                        _grid[index] = GetInSpherePosition(_a, x, y, z);
                        index++;
                    }
                }
            }
            _grid = _grid.OrderBy(v => math.lengthsq(v)).ToArray();
        }

        [BurstCompile]
        private float3 GetInSpherePosition(int a, int x, int y, int z)
        {
            float3 result = new float3
            {
                x = x * a * (math.sqrt((a * a) - (y * y) / (2) - (z * z) / (2) + (y * y * z * z) / (3 * a * a))),
                y = y * a * (math.sqrt((a * a) - (z * z) / (2) - (x * x) / (2) + (x * x * z * z) / (3 * a * a))),
                z = z * a * (math.sqrt((a * a) - (x * x) / (2) - (y * y) / (2) + (y * y * x * x) / (3 * a * a)))
            };
            return result;
        }
    }
}