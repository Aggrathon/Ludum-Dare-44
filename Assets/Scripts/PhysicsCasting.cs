﻿using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;

public static class PhysicsCasting
{
     [BurstCompile]
    public struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public CollisionWorld world;
        [ReadOnly] public NativeArray<RaycastInput> inputs;
        public NativeArray<RaycastHit> results;

        public unsafe void Execute(int index)
        {
            RaycastHit hit = new RaycastHit() { RigidBodyIndex = -1 };
            world.CastRay(inputs[index], out hit);
            results[index] = hit;
        }
    }

    public static JobHandle ScheduleBatchRayCast(CollisionWorld world, NativeArray<RaycastInput> inputs, NativeArray<RaycastHit> results)
    {
        JobHandle rcj = new RaycastJob
        {
            inputs = inputs,
            results = results,
            world = world

        }.Schedule(inputs.Length, 5);
        return rcj;
    }

    [BurstCompile]
    public struct RaycastJobSingle : IJob
    {
        [ReadOnly] public CollisionWorld world;
        [ReadOnly] public RaycastInput input;
        public RaycastHit result;
        public bool hit;

        public unsafe void Execute()
        {
            hit = world.CastRay(input, out result);
        }
    }

    public static bool SingleRayCast(CollisionWorld world, RaycastInput input, out RaycastHit result)
    {
        var job = new RaycastJobSingle
        {
            input = input,
            world = world

        };
        job.Schedule().Complete();
        result = job.result;
        return job.hit;
    }

    public static void SingleRayCast2(CollisionWorld world, RaycastInput input, out RaycastHit result)
    {
        var rayCommands = new NativeArray<RaycastInput>(1, Allocator.TempJob);
        var rayResults = new NativeArray<RaycastHit>(1, Allocator.TempJob);
        rayCommands[0] = input;
        var handle = ScheduleBatchRayCast(world, rayCommands, rayResults);
        handle.Complete();
        result = rayResults[0];
        rayCommands.Dispose();
        rayResults.Dispose();
    }


    public unsafe static void SphereCastAll(CollisionWorld world, float radius, uint mask, float3 origin, float3 direction, NativeList<ColliderCastHit> results) {
        var sphereCollider = Unity.Physics.SphereCollider.Create(float3.zero, radius,
            new CollisionFilter() { CategoryBits = mask, MaskBits = mask, GroupIndex = (int)mask});
        ColliderCastInput input = new ColliderCastInput()
        {
            Position  = origin,
            Orientation = quaternion.identity,
            Direction = direction,
            Collider = (Collider*)sphereCollider.GetUnsafePtr()
        };
        world.CastCollider(input, ref results);
    }

    public unsafe static ColliderCastHit SphereCast(CollisionWorld world, float radius, uint mask, float3 origin, float3 direction) {
        var sphereCollider = Unity.Physics.SphereCollider.Create(float3.zero, radius,
            new CollisionFilter() { CategoryBits = mask, MaskBits = mask, GroupIndex = (int)mask});
        ColliderCastInput input = new ColliderCastInput()
        {
            Position  = origin,
            Orientation = quaternion.identity,
            Direction = direction,
            Collider = (Collider*)sphereCollider.GetUnsafePtr()
        };
        ColliderCastHit hit = new ColliderCastHit() { RigidBodyIndex = -1 };
        world.CastCollider(input, out hit);
        return hit;
    }
}
