using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct ResourceColor {
        public Asteroid.Resource resource;
        public Color color;
    }

    public Transform planet;
    public float atmosphere = 2f;
    public float width = 10;
    public float height = 5;
    public float density = 1;
    public float rnd = 0.8f;
    public GameObject prefab;

    public ResourceColor[] colors;

    [ContextMenu("Spawn Asteroids")]
    public void SpawnAsteroids() {
        while(true) {
            var go = GameObject.Find("Asteroid(Clone)");
            if (go) DestroyImmediate(go);
            else break;
        }
        for (float i = density/2; i < width; i += density)
        {
            for (float j = density/2; j < height; j += density)
            {
                for (float k = -1; k <= 1; k +=2)
                {
                    for (float l = -1; l <= 1; l +=2)
                    {
                        var vec =  new Vector3(i * k + Random.Range(-density, density) * rnd, j * l + Random.Range(-density, density) * rnd);
                        if ((vec - planet.position).sqrMagnitude > planet.localScale.x + atmosphere) {
                            var go = Instantiate(prefab, vec, Quaternion.identity);
                            var col = colors[Random.Range(0, colors.Length)];
                            go.GetComponent<AsteroidProxy>().Value = new Asteroid() { resource = col.resource };
                            go.GetComponent<SpriteRenderer>().color = col.color;
                        }
                    }
                }
            }
        }
    }
}
