using Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;

using math = Unity.Mathematics.math;
using random = Unity.Mathematics.Random;

public class FishGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform waterObject;
    public Transform fishPrefab;

    [Header("Spawn Settings")]
    public int amountOfFish;
    public Vector3 spawnBounds;
    public Vector3 spawnBoundsOffset;
    public float radius;
    public int swimChangeFrequency;

    [Header("Settings")]
    public float swimSpeed;
    public float turnSpeed;

    private NativeArray<Vector3> _velocities;
    private TransformAccessArray _transformAccessArray;
    
    private PositionUpdateJob _positionUpdateJob;
    private JobHandle _positionUpdateJobHandle;
    
    private void Start()
    {
        _velocities = new NativeArray<Vector3>(amountOfFish, Allocator.Persistent);
        
        _transformAccessArray = new TransformAccessArray(amountOfFish);

        for (int i = 0; i < amountOfFish; i++)
        {
            var parentTransform = transform;
            
            /*
            float distanceX = 
                Random.Range(-spawnBounds.x / 2f, spawnBounds.x / 2f) 
                + spawnBoundsOffset.x;

            float distanceZ =
                Random.Range(-spawnBounds.z / 2f, spawnBounds.z / 2f) 
                + spawnBoundsOffset.z;

            float spawnHeight = Random.Range(-spawnBounds.y / 2f, spawnBounds.y / 2f) 
                                + spawnBoundsOffset.y;

            Vector3 spawnPoint = 
                (parentTransform.position + Vector3.up * spawnHeight) 
                + new Vector3(distanceX, 0, distanceZ);
                */

            
            Transform fishTransform = Instantiate(fishPrefab, /*spawnPoint*/parentTransform.position, 
                    Quaternion.identity, parentTransform);
            
            _transformAccessArray.Add(fishTransform);
        }
    }

    private void Update()
    {
        _positionUpdateJob = new PositionUpdateJob()
        {
            objectVelocities = _velocities,
            jobDeltaTime = Time.deltaTime,
            swimSpeed = this.swimSpeed,
            turnSpeed = this.turnSpeed,
            time = Time.time,
            swimChangeFrequency = this.swimChangeFrequency,
            center = waterObject.position,
            bounds = spawnBounds,
            seed = System.DateTimeOffset.Now.Millisecond,
            sphereRadius = radius
        };
        
        _positionUpdateJobHandle = _positionUpdateJob.Schedule(_transformAccessArray);
    }

    private void LateUpdate()
    {
        _positionUpdateJobHandle.Complete(); 
    }

    private void OnDestroy()
    {
        _velocities.Dispose();
        _transformAccessArray.Dispose();
    }
}