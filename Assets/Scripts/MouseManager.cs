using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;

[RequireComponent(typeof(Renderer))]
public class MouseManager : MonoBehaviour
{
    public float range = 1.5f;
    public float strength = 0.75f;
    public VectorField vectorField;

    Renderer rend;
    Camera camera;
    int oldIndex;
    int tiles;

    void Start()
    {
        rend = GetComponent<Renderer>();
        camera = Camera.main;
        oldIndex = -1;
        tiles = (int)(range / vectorField.spacing);
    }

    void Update()
    {
        int newIndex = -1;
        Vector3 pos = Vector3.zero;
        if (Input.GetMouseButton(0)) {
            pos = camera.ScreenToWorldPoint(Input.mousePosition);
            newIndex = vectorField.CoordToIndex(pos);
        }
        if (oldIndex == newIndex)
            return;
        else {
            pos.z = 1f;
            transform.position = pos;
            rend.enabled = newIndex > -1;
        }
        if (oldIndex > -1) {
            for (int i = max(0, oldIndex-tiles); i < min(vectorField.vectors.Length, oldIndex+tiles+1); i++)
            {
                for (int j = -tiles; j < tiles + 1; j++)
                {
                    var index = i + j * vectorField.size;
                    if (index >= vectorField.vectors.Length)
                        break;
                    else if (index >= 0)
                        vectorField.vectors[index] = vectorField.reset[index];
                }
            }
        }
        if (newIndex > -1) {
            for (int i = max(0, newIndex-tiles); i < min(vectorField.vectors.Length, newIndex+tiles+1); i++)
            {
                int i2 = i - newIndex;
                for (int j = -tiles; j < tiles + 1; j++)
                {
                    var index = i + j * vectorField.size;
                    if (index >= vectorField.vectors.Length)
                        break;
                    else if (index >= 0 && i2*i2 + j*j <= tiles*tiles) {
                        vectorField.vectors[index] = new float3(-i2, -j, 0) * strength;
                    }
                }
            }
        }
        oldIndex = newIndex;
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
