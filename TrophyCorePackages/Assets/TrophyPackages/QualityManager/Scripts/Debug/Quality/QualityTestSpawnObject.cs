using UnityEngine;
using System.Collections.Generic;

public class QualityTestSpawnObject : MonoBehaviour
{
    public GameObject prefab;
    public float minForce = 5f;
    public float maxForce = 15f;

    private List<Rigidbody> spawnedObjects = new List<Rigidbody>();

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
            SpawnObject();
        else if (Input.GetKeyDown(KeyCode.Escape))
            DeleteAllObjects();
    }

    void SpawnObject()
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject newObj = Instantiate(prefab, transform.position, Quaternion.identity);
            Rigidbody rb = newObj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                float randomForce = Random.Range(minForce, maxForce);
                rb.AddForce(Random.insideUnitSphere * randomForce, ForceMode.Impulse);
                spawnedObjects.Add(rb);
            }
        }
    }

    void DeleteAllObjects()
    {
        foreach (Rigidbody rb in spawnedObjects)
        {
            Destroy(rb.gameObject);
        }

        spawnedObjects.Clear();
    }
}
