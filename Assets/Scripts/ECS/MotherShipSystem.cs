﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MotherShipSystem : JobComponentSystem
{
    [BurstCompile]
    struct MotherShipSystemJob : IJobForEach<Translation, MotherShip, Rotation>
    {
        public float deltaTime;
        
        public void Execute(ref Translation translation, ref MotherShip ship, ref Rotation rotation)
        {
            if (ship.lerp < 0) return;
            ship.lerp += deltaTime * ship.speed;
            if (ship.lerp > 1) {
                ship.lerp = -1f;
                ship.current = (ship.current + 1) % 4;
                translation.Value = ship.GetPos(ship.current);
                return;
            }
            float3 a = ship.GetPos(ship.current);
            float3 b = ship.GetPos(ship.current + 1);
            float3 c = (a + b) * 0.8f;
            float l = ship.lerp;
            float p = 1 - l;
            float3 bezier = p*p*a + 2*l*p*c + l*l*b;
            translation.Value = bezier;
            float3 grad = 2*p*(c-a) + 2*l*(b-c);
            rotation.Value = quaternion.Euler(0, 0, math.atan2(grad.y, grad.x));
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MotherShipSystemJob() { deltaTime = Time.deltaTime };
        return job.Schedule(this, inputDependencies);
    }
}