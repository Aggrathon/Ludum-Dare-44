using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Rendering;

[UpdateAfter(typeof(BuildPhysicsWorld))]
public class EnemySystem : JobComponentSystem
{
    BuildPhysicsWorld physicsWorldSystem;
    Entity lazerPrefab;
    Entity explosionPrefab;
    EnemySettings settings;

    struct Spawn {
        public float3 pos;
        public float rot;
        public bool lazer;
    }

    [BurstCompile]
    struct EnemySystemJob : IJobForEachWithEntity<Translation, Enemy>
    {
        public float deltaTime;
        public uint mask;
        public NativeQueue<Entity>.Concurrent destroy;
        public NativeQueue<Spawn>.Concurrent spawn;
        [ReadOnly] public CollisionWorld world;

        public void Execute(Entity ent, int index, [ReadOnly] ref Translation translation, ref Enemy enemy)
        {
            enemy.timer -= deltaTime;
            var input = new PointDistanceInput()
            {
                MaxDistance = enemy.rangeDie,
                Filter = new CollisionFilter() { CategoryBits = mask, MaskBits = mask, GroupIndex = 0 },
                Position = translation.Value
            };
            DistanceHit hit;
            if (world.CalculateDistance(input, out hit)) {
                destroy.Enqueue(ent);
                destroy.Enqueue(world.Bodies[hit.RigidBodyIndex].Entity);
                spawn.Enqueue(new Spawn() { pos = hit.Position, lazer = false });
            } else if (enemy.timer < 0) {
                input.MaxDistance = enemy.rangeShoot;
                if (world.CalculateDistance(input, out hit)) {
                    var dir = hit.Position - translation.Value;
                    dir *= enemy.rangeShoot / math.length(dir) * 0.5f;
                    destroy.Enqueue(world.Bodies[hit.RigidBodyIndex].Entity);
                    spawn.Enqueue(new Spawn() { pos = translation.Value + dir, rot = math.atan2(dir.y, dir.x), lazer = true });
                    enemy.timer = enemy.cooldown;
                }
            }
        }
    }

    protected override void OnCreateManager() {
        physicsWorldSystem = EntityManager.World.GetOrCreateSystem<BuildPhysicsWorld>();
        settings = UnityEngine.GameObject.FindObjectOfType<EnemySettings>();
        lazerPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(settings.lazerPrefab, World);
        explosionPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(settings.explosionPrefab, World);
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var destroy = new NativeQueue<Entity>(Allocator.TempJob);
        var spawn = new NativeQueue<Spawn>(Allocator.TempJob);
        var job = new EnemySystemJob() {
            deltaTime = UnityEngine.Time.deltaTime,
            world = physicsWorldSystem.PhysicsWorld.CollisionWorld,
            mask = (uint)settings.blobShape.BelongsTo,
            destroy = destroy.ToConcurrent(),
            spawn = spawn.ToConcurrent()
        };
        var jh = JobHandle.CombineDependencies(inputDependencies, physicsWorldSystem.FinalJobHandle);
        jh = job.Schedule(this, jh);
        jh.Complete();
        Entity ent;
        while(destroy.TryDequeue(out ent)) EntityManager.DestroyEntity(ent);
        Spawn sp;
        while(spawn.TryDequeue(out sp)) {
            if (sp.lazer) {
                var e = EntityManager.Instantiate(lazerPrefab);
                EntityManager.SetComponentData(e, new Translation() { Value = sp.pos });
                EntityManager.SetComponentData(e, new Rotation() { Value = quaternion.RotateZ(sp.rot) });
            } else {
                var e = EntityManager.Instantiate(explosionPrefab);
                EntityManager.SetComponentData(e, new Translation() { Value = sp.pos });
            }
        }
        destroy.Dispose();
        spawn.Dispose();
        return jh;
    }
}