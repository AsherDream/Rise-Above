using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        // Singleton Pattern: Ensures only one SaveManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // FILE NAMING HELPER
    // -------------------------------------------------------------------------
    // We use this single function to make sure we never mistype the file name again.
    // It guarantees every function looks for "save_slot_X.json"
    private string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    // -------------------------------------------------------------------------
    // SAVE, LOAD & DELETE LOGIC
    // -------------------------------------------------------------------------

    public bool IsSlotEmpty(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        return !File.Exists(path);
    }

    public void SaveGame(int slotIndex)
    {
        // 1. Create the data container
        GameData data = new GameData();

        // 2. Fill it with current game state
        // (Add your game variables here later, e.g., data.score = 100;)

        // 3. Convert to JSON
        string json = JsonUtility.ToJson(data, true);

        // 4. Write to file using the helper path
        string path = GetSavePath(slotIndex);
        File.WriteAllText(path, json);

        Debug.Log($"Game Saved to Slot {slotIndex}");
    }

    public void LoadGame(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // 5. Apply data to player here...
            // (e.g., player.transform.position = data.position;)

            Debug.Log($"Loaded Slot {slotIndex}");
        }
        else
        {
            Debug.LogError($"Save file not found in Slot {slotIndex}");
        }
    }

    public void DeleteSaveFile(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted Save File {slotIndex}");
        }
        else
        {
            Debug.LogWarning($"Could not delete. File not found at: {path}");
        }
    }
}