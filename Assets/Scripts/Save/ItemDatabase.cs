
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;
    public List<GameObject> itemPrefabs; // Assign all item prefabs in the Inspector

    private Dictionary<string, GameObject> itemDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        instance = this;
        foreach (var prefab in itemPrefabs)
        {
            var item = prefab.GetComponent<Item>();
            if (item != null && !string.IsNullOrEmpty(item.id))
            {
                itemDict[item.id] = prefab;
            }
        }
    }

    public GameObject GetItemById(string id)
    {
        if (itemDict.TryGetValue(id, out var prefab))
            return prefab;
        Debug.LogWarning("Item with id " + id + " not found in database.");
        return null;
    }
}