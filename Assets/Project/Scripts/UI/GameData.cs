using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // --- THIS IS WHERE YOU LIST WHAT TO SAVE ---
    public int playerHealth;
    public Vector3 playerPosition;
    public string currentSceneName;

    // You can add more later (like coins, inventory, etc.)

    // --- CONSTRUCTOR (Sets default values for a New Game) ---
    public GameData()
    {
        this.playerHealth = 100; // Default health
        this.playerPosition = new Vector3(0, 0, 0); // Default start position
        this.currentSceneName = "Level1"; // Default level
    }
}