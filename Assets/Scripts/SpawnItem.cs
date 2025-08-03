using UnityEngine;
using System.Collections.Generic;

public class SpawnItem : MonoBehaviour
{
    public GameObject item; // The item to spawn (e.g., cherry)
    public List<Transform> spawnPoints = new List<Transform>();
    private Dictionary<Transform, GameObject> occupiedSpawnPoints = new Dictionary<Transform, GameObject>();
    private int maxItems;

    void Start()
    {
        // Find all child objects named Item (1), Item (2), etc.
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Item"))
            {
                spawnPoints.Add(child);
            }
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points found as children of " + gameObject.name);
            return;
        }

        // Set maxItems to a random value between 1 and the number of spawn points
        maxItems = Random.Range(1, spawnPoints.Count + 1);
        Debug.Log($"Max items set to: {maxItems}");

        // Spawn items immediately
        SpawnItems();
    }

    private void SpawnItems()
    {
        int itemsToSpawn = Random.Range(1, Mathf.Min(maxItems + 1, spawnPoints.Count + 1));

        for (int i = 0; i < itemsToSpawn; i++)
        {
            List<Transform> availableSpawnPoints = spawnPoints.FindAll(sp => !occupiedSpawnPoints.ContainsKey(sp));
            if (availableSpawnPoints.Count > 0)
            {
                Transform spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

                // Adjust the position to bring the item in front of the point
                Vector3 spawnPosition = spawnPoint.position;
                spawnPosition.z -= 1f; // Move the item forward on the z-axis

                GameObject spawnedItem = Instantiate(item, spawnPosition, Quaternion.identity);
                occupiedSpawnPoints[spawnPoint] = spawnedItem;

                Debug.Log($"Item spawned at {spawnPoint.name}. Current count: {occupiedSpawnPoints.Count}");
            }
        }
    }
}