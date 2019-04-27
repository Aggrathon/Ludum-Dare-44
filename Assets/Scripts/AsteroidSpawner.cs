using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{

    public Transform planet;
    public float atmosphere = 2f;
    public float width = 10;
    public float height = 5;
    public float density = 1;
    public float rnd = 0.8f;
    public GameObject[] prefabs;

    [ContextMenu("Spawn Asteroids")]
    public void SpawnAsteroids() {
        while(true) {
            var go = GameObject.Find("Asteroid (Spawned)");
            if (go) DestroyImmediate(go);
            else break;
        }
        for (float j = density/2; j < height; j += density)
        {
            float offset = Random.Range(-density, density) * rnd * 2;
            for (float i = density/2; i < width; i += density)
            {
                for (float k = -1; k <= 1; k +=2)
                {
                    for (float l = -1; l <= 1; l +=2)
                    {
                        var vec =  new Vector3(i * k + Random.Range(-density, density) * rnd + offset, j * l + Random.Range(-density, density) * rnd);
                        if ((vec - planet.position).sqrMagnitude > planet.localScale.x + atmosphere) {
                            var p = prefabs[Random.Range(0, prefabs.Length)];
                            var go = Instantiate(p, vec, Quaternion.identity);
                            go.name = "Asteroid (Spawned)";
                        }
                    }
                }
            }
        }
    }
}
