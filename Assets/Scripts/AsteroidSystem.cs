using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;
using TMPro;

public class AsteroidSystem : JobComponentSystem
{
    float timeout = 0f;
    float timer;
    NativeQueue<int2> changes;
    TextMeshPro[] texts;
    string[] cache;

    override protected void OnCreateManager() {
        timeout = 0f;
        changes = new NativeQueue<int2>(Allocator.Persistent);
        cache = new string[101];
        for (int i = 1; i < 100; i++)
        {
            cache[i + 1] = i.ToString();
        }
        cache[0] = "E";
        cache[1] = "";
        texts = null;
    }

    void CreateTexts() {
        var sp = GameObject.FindObjectOfType<AsteroidSettings>();
        timer = sp.tickTime;
        var job = new AsteroidPosJob() { pos = new NativeList<float3>(Allocator.TempJob) };
        var jh = job.ScheduleSingle(this);
        jh.Complete();
        texts = new TextMeshPro[job.pos.Length];
        for (int i = 0; i < job.pos.Length; i++)
        {
            var go = GameObject.Instantiate(sp.textPrefab, job.pos[i], Quaternion.identity, sp.transform);
            texts[i] = go.GetComponent<TextMeshPro>();
        }
        job.pos.Dispose();
    }
    
    [BurstCompile]
    struct AsteroidPosJob : IJobForEach<Translation, Asteroid>
    {
        public NativeList<float3> pos;
        public void Execute([ReadOnly] ref Translation translation, ref Asteroid ast)
        {
            ast.index = pos.Length;
            pos.Add(translation.Value);
        }
    }

    [BurstCompile]
    struct AsteroidSystemJob : IJobForEach<Asteroid>
    {
        public NativeQueue<int2>.Concurrent changes;
        
        public void Execute(ref Asteroid ast)
        {
            if (ast.mines > 0 && ast.stock < 99) {
                if (ast.minerals > 0) {
                    ast.minerals -= ast.mines;
                    ast.stock += ast.mines;
                    changes.Enqueue(new int2(ast.index, ast.stock));
                } else if (ast.stock == 0) {
                    ast.stock = -1;
                    changes.Enqueue(new int2(ast.index, ast.stock));
                }
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        timeout -= Time.deltaTime;
        if (timeout < 0) {
            if (texts == null) CreateTexts();
            timeout += 1f;
            var job = new AsteroidSystemJob() { changes = changes.ToConcurrent() };
            int2 todo;
            while(changes.TryDequeue(out todo)) {
                texts[todo.x].text = cache[todo.y + 1];
            }
            return job.Schedule(this, inputDependencies);
        } else 
        return inputDependencies;
    }

    override protected void OnDestroyManager() {
        changes.Dispose();
    }
}