using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Physics.Extensions;
using Unity.Physics;

public class PeriodicSpawner : MonoBehaviour
{

    public GameObject prefab;

    public float rnd = 2f;
    public float delay = 1f;

    EntityManager mgr;
    Entity epref;

    private void OnEnable() {
        mgr = World.Active.EntityManager;
        epref = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, mgr.World);
        StartCoroutine(Spawn());
    }
    
    IEnumerator Spawn() {
        var wfs = new WaitForSeconds(delay);
        while(true) {
            yield return wfs;
            mgr.SetComponentData(mgr.Instantiate(epref), new Translation() { 
                Value = transform.position + new Vector3(Random.Range(-rnd, rnd), Random.Range(-rnd, rnd), 0)
            });
        }
    }
}
