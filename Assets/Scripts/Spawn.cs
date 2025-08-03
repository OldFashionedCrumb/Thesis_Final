using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawn : MonoBehaviour
{
    public List<GameObject> enemyTypes; // List of enemy prefabs
    public float spawnRate = 2f;
    private float nextSpawn = 0.0f;

    private List<Transform> spawnPoints = new List<Transform>();
    private Dictionary<Transform, GameObject> occupiedSpawnPoints = new Dictionary<Transform, GameObject>();

    private int maxEnemies;

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Enemy"))
            {
                spawnPoints.Add(child);
            }
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points found as children of " + gameObject.name);
        }

        maxEnemies = spawnPoints.Count > 0 ? Random.Range(1, spawnPoints.Count + 1) : 0;
        Debug.Log($"Max enemies set to: {maxEnemies}");

        ClearExistingEnemies();
    }

    void Update()
    {
        CleanupEnemyList();

        if (Time.time > nextSpawn && occupiedSpawnPoints.Count < maxEnemies && spawnPoints.Count > 0)
        {
            nextSpawn = Time.time + spawnRate;

            int enemiesToSpawn = Random.Range(1, Mathf.Min(maxEnemies + 1, spawnPoints.Count + 1));

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                List<Transform> availableSpawnPoints = spawnPoints.FindAll(sp => !occupiedSpawnPoints.ContainsKey(sp));
                if (availableSpawnPoints.Count > 0)
                {
                    Transform spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

                    // Randomly select an enemy type
                    GameObject selectedEnemy = enemyTypes[Random.Range(0, enemyTypes.Count)];

                    GameObject spawnedEnemy = Instantiate(selectedEnemy, spawnPoint.position, Quaternion.identity);
                    occupiedSpawnPoints[spawnPoint] = spawnedEnemy;

                    Transform waypoints = spawnedEnemy.transform.Find("Waypoints");
                    if (waypoints != null)
                    {
                        Transform point1 = waypoints.Find("Point1");
                        Transform point2 = waypoints.Find("Point2");

                        if (point1 != null && point2 != null)
                        {
                            point1.position = new Vector3(spawnedEnemy.transform.position.x + 5f,
                                spawnedEnemy.transform.position.y,
                                spawnedEnemy.transform.position.z);

                            point2.position = new Vector3(spawnedEnemy.transform.position.x - 5f,
                                spawnedEnemy.transform.position.y,
                                spawnedEnemy.transform.position.z);
                        }
                    }

                    Debug.Log($"Enemy spawned at {spawnPoint.name}. Current count: {occupiedSpawnPoints.Count}");
                }
            }
        }
    }

    private void CleanupEnemyList()
    {
        List<Transform> keysToRemove = new List<Transform>();
        foreach (var entry in occupiedSpawnPoints)
        {
            if (entry.Value == null)
            {
                keysToRemove.Add(entry.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            occupiedSpawnPoints.Remove(key);
        }
    }

    private void ClearExistingEnemies()
    {
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj != null && enemyTypes.Exists(e => obj.name.StartsWith(e.name + "(Clone)")))
            {
                Destroy(obj);
            }
        }

        occupiedSpawnPoints.Clear();
    }
}