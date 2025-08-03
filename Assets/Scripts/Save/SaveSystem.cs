
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath => Application.persistentDataPath + "/save.json";

    public static void SaveGame(Fox player, InventorySystem inventory)
    {
        SaveData data = new SaveData();
        data.playerPosition = new float[] { player.transform.position.x, player.transform.position.y, player.transform.position.z };
        data.playerHealth = player.GetCurrentHealth();
        data.inventoryItems = inventory.GetItemList();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved: " + savePath);
    }

    public static void LoadGame(Fox player, InventorySystem inventory)
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found!");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Restore position
        player.transform.position = new Vector3(data.playerPosition[0], data.playerPosition[1], data.playerPosition[2]);
        // Restore health
        player.SetCurrentHealth(data.playerHealth);
        // Restore inventory
        inventory.SetItemList(data.inventoryItems);

        Debug.Log("Game Loaded");
    }
}