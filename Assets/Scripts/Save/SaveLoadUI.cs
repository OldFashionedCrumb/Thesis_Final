using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveLoadUI : MonoBehaviour
{
    public Button[] saveButtons;
    public Button[] loadButtons;
    public Button[] deleteButtons;
    public Text[] slotTexts;
    [SerializeField] private GameObject saveLoadPanel;

    public Fox player;
    public InventorySystem inventory;

    private string GetSlotPath(int slot) => Application.persistentDataPath + $"/save_slot{slot + 1}.json";

    void Start()
    {
        // Auto-assign player and inventory if not set in Inspector
        if (player == null)
            player = FindObjectOfType<Fox>();
        if (inventory == null)
            inventory = FindObjectOfType<InventorySystem>();

        UpdateSlotTexts();
        for (int i = 0; i < 3; i++)
        {
            int slot = i;
            saveButtons[i].onClick.AddListener(() => Save(slot));
            loadButtons[i].onClick.AddListener(() => Load(slot));
            deleteButtons[i].onClick.AddListener(() => Delete(slot));
        }
    }

    void UpdateSlotTexts()
    {
        for (int i = 0; i < 3; i++)
        {
            string path = GetSlotPath(i);
            slotTexts[i].text = File.Exists(path) ? $"Slot {i + 1}: Saved" : $"Slot {i + 1}: Empty";
        }
    }

    void Save(int slot)
    {
        string path = GetSlotPath(slot);
        SaveData data = new SaveData
        {
            playerPosition = new float[] {
                player.transform.position.x,
                player.transform.position.y,
                player.transform.position.z
            },
            playerHealth = player.GetCurrentHealth(),
            inventoryItems = inventory.GetItemList()
        };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        UpdateSlotTexts();
        Debug.Log($"Saved to slot {slot + 1}");
    }

    void Load(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file in this slot!");
            return;
        }
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        player.transform.position = new Vector3(data.playerPosition[0], data.playerPosition[1], data.playerPosition[2]);
        player.SetCurrentHealth(data.playerHealth);
        inventory.SetItemList(data.inventoryItems);
        Debug.Log($"Loaded from slot {slot + 1}");
    }

    void Delete(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
            File.Delete(path);
        UpdateSlotTexts();
        Debug.Log($"Deleted slot {slot + 1}");
    }
    
    public void ShowSaveLoadPanel()
    {
        saveLoadPanel.SetActive(true);
        UpdateSlotTexts();
    }
}