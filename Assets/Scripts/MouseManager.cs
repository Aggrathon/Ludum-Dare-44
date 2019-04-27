using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

[RequireComponent(typeof(Renderer))]
public class MouseManager : MonoBehaviour
{
    public float range = 1.5f;
    public float strength = 0.75f;
    public VectorField vectorField;

    Renderer rend;
    Camera camera;
    EntityManager mgr;
    Entity ent;
    GravityWell gw;

    void Start()
    {
        rend = GetComponent<Renderer>();
        camera = Camera.main;
        mgr = World.Active.EntityManager;
        ent = mgr.CreateEntity(typeof(Translation), typeof(GravityWell));
        gw = new GravityWell(range, vectorField.spacing, false, strength);
        mgr.SetComponentData(ent, gw);
    }

    void Update()
    {
        int newIndex = -1;
        Vector3 pos = Vector3.zero;
        if (Input.GetMouseButton(0)) {
            pos = camera.ScreenToWorldPoint(Input.mousePosition);
            newIndex = vectorField.CoordToIndex(pos);
            pos.z = 1f;
            transform.position = pos;
            mgr.SetComponentData(ent, new Translation() { Value = pos });
            if (!gw.enabled) {
                rend.enabled = true;
                gw.enabled = true;
                mgr.SetComponentData(ent, gw);
            }
        } else if (gw.enabled) {
            gw = mgr.GetComponentData<GravityWell>(ent);
            gw.enabled = false;
            mgr.SetComponentData(ent, gw);
            rend.enabled = false;
        }
    }

    private void OnDrawGizmosSelected() {
        if (camera) {
            Gizmos.color = Color.yellow;
            var pos = camera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }
}
