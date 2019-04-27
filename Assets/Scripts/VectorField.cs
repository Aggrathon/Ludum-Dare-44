using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

public class VectorField : MonoBehaviour
{
    public float radius = 17f;
    public float spacing = 0.25f;
    public float planetGravity = 10f;

    public NativeArray<float3> vectors;
    
    CalcVector job;
    JobHandle jobH;


    [BurstCompile]
    struct CalcVector : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Translation> asteroids;
        [WriteOnly] public NativeArray<float3> vectors;

        public float radius;
        public float spacing;
        public float planet;

        public int width;

        public void Execute(int index)
        {
            float3 pos = new float3(
                -radius + (int)(index % width) * spacing,
                -radius + (int)(index / width) * spacing,
                0);
            float3 vec = - pos  / math.pow(math.length(pos), 3) * planet;
            for (int i = 0; i < asteroids.Length; i++)
            {
                float3 dir = asteroids[i].Value - pos;
                float magn = math.length(dir);
                if (magn == 0) magn = 1;
                vec += dir / math.pow(magn, 3);
            }
            vectors[index] = vec;
        }
    }

    private void Awake() {
        int size = (int) (radius * 2 / spacing);
        vectors = new NativeArray<float3>(size * size, Allocator.Persistent);
        var query = World.Active.EntityManager.CreateEntityQuery(typeof(Translation), typeof(Asteroid));
        JobHandle handle;
        job.vectors = vectors;
        job.radius = radius;
        job.width = size;
        job.spacing = spacing;
        job.planet = planetGravity;
        job.asteroids = query.ToComponentDataArray<Translation>(Allocator.TempJob, out handle);
        jobH = job.Schedule(vectors.Length, 32, handle);
        
    }

    void Start()
    {
        jobH.Complete();
        job.asteroids.Dispose();
        enabled = false;
    }

    private void OnDestroy() {
        vectors.Dispose();
    }

    private void OnDrawGizmosSelected() {
        int size = (int) (radius * 2 / spacing);
        Gizmos.color = Color.green;
        for (int i = 0; i < vectors.Length; i++)
        {
            float3 pos = new float3(-radius + (int)(i % size) * spacing, -radius + (int)(i / size) * spacing, 0);
            Gizmos.DrawLine(pos, pos + vectors[i] / math.length(vectors[i]) * spacing);
        }
    }
}
