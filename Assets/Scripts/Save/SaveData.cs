// Assets/Scripts/SaveData.cs

using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public float[] playerPosition;
    public int playerHealth;
    public List<string> inventoryItems;
    // Add more fields as needed
}