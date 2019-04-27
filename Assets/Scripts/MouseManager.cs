﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Authoring;

[RequireComponent(typeof(VectorField))]
public class MouseManager : MonoBehaviour
{
    [Header("Gravity Well")]
    public float range = 1.5f;
    public float strength = 0.75f;
    public Renderer marker;

    [Header("Mines")]
    public float blobRange = 1f;
    public int blobCost = 10;
    public PhysicsShape asteroidPhysicsShape;
    public PhysicsShape blobPhysicsShape;
    public GameObject factoryPrefab;

    VectorField vectorField;
    Camera camera;
    EntityManager mgr;
    Entity ent;
    GravityWell gw;
    Entity fp;

    void Start()
    {
        vectorField = GetComponent<VectorField>();
        camera = Camera.main;
        mgr = World.Active.EntityManager;
        ent = mgr.CreateEntity(typeof(Translation), typeof(GravityWell));
        gw = new GravityWell(range, vectorField.spacing, false, strength);
        mgr.SetComponentData(ent, gw);
        marker.gameObject.SetActive(false);
        fp = GameObjectConversionUtility.ConvertGameObjectHierarchy(factoryPrefab, mgr.World);
    }

    void Update()
    {
        // Gravity Well
        if (Input.GetMouseButton(0)) {
            var pos = camera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 1f;
            marker.transform.position = pos;
            mgr.SetComponentData(ent, new Translation() { Value = pos });
            if (!gw.enabled) {
                marker.gameObject.SetActive(true);
                gw.enabled = true;
                mgr.SetComponentData(ent, gw);
            }
        } else if (gw.enabled) {
            gw = mgr.GetComponentData<GravityWell>(ent);
            gw.enabled = false;
            mgr.SetComponentData(ent, gw);
            marker.gameObject.SetActive(false);
        }

        // Build Mines
        if (Input.GetMouseButtonUp(1)) {
            var pos = camera.ScreenToWorldPoint(Input.mousePosition);
            var physicsWorldSystem = mgr.World.GetOrCreateSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
            physicsWorldSystem.FinalJobHandle.Complete();
            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
            var hit = new Unity.Physics.RaycastHit();
            uint mask = (uint)asteroidPhysicsShape.BelongsTo;
            RaycastInput input = new RaycastInput()
            {
                Ray = new Unity.Physics.Ray() { Origin = pos, Direction = new float3(0, 0, 100) },
                Filter = new CollisionFilter() { CategoryBits =  mask, MaskBits = mask, GroupIndex = (int)mask }
            };
            PhysicsCasting.SingleRayCast2(collisionWorld, input, out hit);
            if (hit.RigidBodyIndex >= 0) {
                var e = collisionWorld.Bodies[hit.RigidBodyIndex].Entity;
                if (mgr.HasComponent(e, typeof(Asteroid))) {
                    NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.TempJob);
                    PhysicsCasting.SphereCast(collisionWorld, blobRange, (uint)blobPhysicsShape.BelongsTo, pos, new float3(0, 0, 100), hits);
                    if (hits.Length >= blobCost) {
                        NativeArray<Entity> torm = new NativeArray<Entity>(10, Allocator.TempJob);
                        int j = 0;
                        for (int i = 0; i < hits.Length && j < 10; i++)
                        {
                            var e2 = collisionWorld.Bodies[hits[i].RigidBodyIndex].Entity;
                            if (mgr.HasComponent(e2, typeof(Blob))) {
                                torm[j++] = e2;
                            }
                        }
                        if (j == 10) { 
                            mgr.DestroyEntity(torm);
                            var f = mgr.Instantiate(fp);
                            var tr = mgr.GetComponentData<Translation>(e);
                            tr.Value.z -= 0.01f;
                            mgr.SetComponentData(f, tr);
                            var ast = mgr.GetComponentData<Asteroid>(e);
                            ast.mines++;
                            mgr.SetComponentData(e, ast);
                        }
                        torm.Dispose();
                    }
                    hits.Dispose();
                }
            }
        }
    }

}
