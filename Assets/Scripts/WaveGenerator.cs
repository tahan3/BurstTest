using System.Collections;
using System.Collections.Generic;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class WaveGenerator : MonoBehaviour
{
    [Header("Wave Parameters")]
    public float waveScale;
    public float waveOffsetSpeed;
    public float waveHeight;
    public float minWaveHeight;
    public float maxWaveHeight;

    [Header("References and Prefabs")]
    public MeshFilter waterMeshFilter;
    
    private Mesh _waterMesh;

    private NativeArray<Vector3> _waterVertices;
    private NativeArray<Vector3> _waterNormals;
    
    private JobHandle _meshModificationJobHandle;
    private UpdateMeshJob _meshModificationJob;
    
    private void Start()
    {
        _waterMesh = waterMeshFilter.mesh; 

        _waterMesh.MarkDynamic(); 
        
        _waterVertices = 
            new NativeArray<Vector3>(_waterMesh.vertices, Allocator.Persistent); 
        _waterNormals = 
            new NativeArray<Vector3>(_waterMesh.normals, Allocator.Persistent);
    }

    private void Update()
    {
        _meshModificationJob = new UpdateMeshJob()
        {
            vertices = _waterVertices,
            normals = _waterNormals,
            offsetSpeed = waveOffsetSpeed,
            time = Time.time,
            scale = waveScale,
            height = waveHeight,
            minHeight = minWaveHeight,
            maxHeight = maxWaveHeight
        };
        
        _meshModificationJobHandle = 
            _meshModificationJob.Schedule(_waterVertices.Length, 64);
    }

    private void LateUpdate()
    {
        _meshModificationJobHandle.Complete();
        
        _waterMesh.SetVertices(_meshModificationJob.vertices);
        
        _waterMesh.RecalculateNormals();
    }

    private void OnDestroy()
    {
        _waterVertices.Dispose();
        _waterNormals.Dispose();
    }
}