using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

using math = Unity.Mathematics.math;
using random = Unity.Mathematics.Random;

namespace Jobs
{
    [BurstCompile]
    public struct PositionUpdateJob : IJobParallelForTransform
    {
        public NativeArray<Vector3> objectVelocities;

        public Vector3 bounds;
        public Vector3 center;

        public float jobDeltaTime;
        public float time;
        public float swimSpeed;
        public float turnSpeed;
        public int swimChangeFrequency;
        public float sphereRadius;

        public float seed;
        
        public void Execute(int index, TransformAccess transform)
        {
            Vector3 currentVelocity = objectVelocities[index];
            
            random randomGen = new random((uint)(index * time + 1 + seed));
            
            transform.position += 
                transform.localToWorldMatrix.MultiplyVector(new Vector3(0, 0, 1)) * 
                swimSpeed * 
                jobDeltaTime * 
                randomGen.NextFloat(0.3f, 1.0f);
            
            if (currentVelocity != Vector3.zero)
            {
                transform.rotation = 
                    Quaternion.Lerp(transform.rotation, 
                        Quaternion.LookRotation(currentVelocity), turnSpeed * jobDeltaTime);
            }
            
            Vector3 currentPosition = transform.position;
            
            if (/*CubeBoundsCheck*/SphereBoundsCheck(currentPosition))
            {
                objectVelocities[index] = /*ChangeBoxVelocity*/ChangeSphereVelocity(currentPosition, randomGen);
                transform.rotation = RotationChange(transform, objectVelocities[index]);
            }
            
            if (randomGen.NextInt(0, swimChangeFrequency) <= 2)
            {
                objectVelocities[index] = RandomDirection(randomGen);
            }
        }

        private Vector3 RandomDirection(random randomGen)
        {
            return new Vector3(
                randomGen.NextFloat(-1f, 1f), 
                randomGen.NextFloat(-1f, 1f), 
                randomGen.NextFloat(-1f, 1f));
        }
        
        private bool CubeBoundsCheck(Vector3 currentPosition)
        {
            return (currentPosition.x > center.x + bounds.x / 2 ||
                    currentPosition.x < center.x - bounds.x / 2 ||
                    currentPosition.z > center.z + bounds.z / 2 ||
                    currentPosition.z < center.z - bounds.z / 2 ||
                    currentPosition.y > center.y + bounds.y / 2 ||
                    currentPosition.y < center.y - bounds.y / 2);
        }

        private bool SphereBoundsCheck(Vector3 currentPosition)
        {
            return math.pow(currentPosition.x - center.x, 2) + 
                   math.pow(currentPosition.y - center.y, 2) +
                   math.pow(currentPosition.z - center.z , 2) > 
                   math.pow(sphereRadius, 2);
        }

        private Vector3 ChangeBoxVelocity(Vector3 currentPosition, random randomGen)
        {
            Vector3 internalPosition = new Vector3(
                center.x + randomGen.NextFloat(-bounds.x / 2, bounds.x / 2) / 1.3f,
                center.y + randomGen.NextFloat(-bounds.y / 2, bounds.y / 2) / 1.3f,
                center.z + randomGen.NextFloat(-bounds.z / 2, bounds.z / 2) / 1.3f);

            return (internalPosition- currentPosition).normalized;
        }

        private Vector3 ChangeSphereVelocity(Vector3 currentPosition, random randomGen)
        {
            Vector3 internalPosition = new Vector3(
                center.x + randomGen.NextFloat(-sphereRadius, sphereRadius) / 1.3f,
                center.y + randomGen.NextFloat(-sphereRadius, sphereRadius) / 1.3f,
                center.z + randomGen.NextFloat(-sphereRadius, sphereRadius) / 1.3f);

            return (internalPosition- currentPosition).normalized;
        }
        
        private Quaternion RotationChange(TransformAccess transform, Vector3 currentVelocity)
        {
            return Quaternion.Lerp(transform.rotation, 
                Quaternion.LookRotation(currentVelocity), 
                turnSpeed * jobDeltaTime * 2);
        }
    }
}