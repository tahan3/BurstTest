using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Jobs
{
    [BurstCompile]
    public struct UpdateMeshJob : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;
        
        [ReadOnly]
        public NativeArray<Vector3> normals;
        
        public float offsetSpeed;
        public float scale;
        public float height;
        public float minHeight;
        public float maxHeight;
        
        public float time;

        public void Execute(int index)
        {
            var vertex = vertices[index];
            
            if (normals[index].z/*vertex.z*/ > 0f)
            {
                var noiseValue = 
                    Noise(vertex.x * scale + offsetSpeed * time,
                        vertex.y * scale + offsetSpeed * time);

                var waveHeight = math.clamp(noiseValue * height, minHeight, maxHeight);
                
                vertices[index] = 
                    new Vector3(vertex.x , vertex.y, waveHeight);
            }
        }
        
        private float Noise(float x, float y)
        {
            float2 pos = math.float2(x, y);
            return noise.snoise(pos);
        }
    }
}