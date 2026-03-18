using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class RiseAboveDevTools
{
    // ==========================================
    // 💾 SECTION 1: SAVE DATA & MEMORY
    // These buttons work ANYTIME (Even when stopped)
    // ==========================================

    [MenuItem("Rise Above Dev/1. Save Data/💥 WIPE ALL DATA (Nuke it)")]
    public static void WipeAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.LogWarning("🧹 [Dev Tools] NUKE INITIATED: All achievements, tutorial memory, and settings have been completely wiped.");
    }

    [MenuItem("Rise Above Dev/1. Save Data/🎓 Reset Tutorial Only")]
    public static void ResetTutorialOnly()
    {
        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();
        Debug.Log("🔄 [Dev Tools] Tutorial memory wiped. You will play the tutorial on your next run.");
    }

    [MenuItem("Rise Above Dev/1. Save Data/🏆 Unlock ALL Achievements")]
    public static void UnlockAllAchievements()
    {
        string[] allAchievements = new string[]
        {
            "panic_shopper", "valedictorian", "barely_passing", "over_prepared",
            "buddy_system", "cleanup_aisle_3", "absolute_menace", "caught_in_storm",
            "we_have_enough", "vegan_run", "nerves_of_steel"
        };

        foreach (string id in allAchievements)
        {
            PlayerPrefs.SetInt("Ach_" + id, 1);
        }

        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        Debug.Log("🏆 [Dev Tools] HACKER MODE: All achievements unlocked and tutorial skipped!");
    }


    // ==========================================
    // 🎬 SECTION 2: SCENE NAVIGATION
    // These use your custom SceneTransitionManager!
    // ==========================================

    [MenuItem("Rise Above Dev/2. Navigation/⏩ Fast Travel: Main Menu")]
    public static void LoadMainMenu()
    {
        if (!Application.isPlaying) { Debug.LogWarning("⚠️ You must be in Play Mode to use Fast Travel!"); return; }

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }

    [MenuItem("Rise Above Dev/2. Navigation/⏩ Fast Travel: SuperMarket")]
    public static void LoadSupermarket()
    {
        if (!Application.isPlaying) { Debug.LogWarning("⚠️ You must be in Play Mode to use Fast Travel!"); return; }

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("SuperMarket_Scene");
        else
            SceneManager.LoadScene("SuperMarket_Scene");
    }


    // ==========================================
    // 🛠️ SECTION 3: GAMEPLAY TRIGGERS (Live Testing)
    // Completely wired up to your actual game logic!
    // ==========================================

    [MenuItem("Rise Above Dev/3. Gameplay/⏰ Force Timer to Zero (Test Game Over)")]
    public static void ForceTimerZero()
    {
        if (!Application.isPlaying) { Debug.LogWarning("⚠️ You must be in Play Mode!"); return; }

        if (Scene1Manager.Instance != null)
        {
            // Set it to 0.1 so the Update loop catches it naturally and triggers the game over sequence
            Scene1Manager.Instance.currentTime = 0.1f;
            Debug.Log("⏱️ [Dev Tools] Timer forced to zero!");
        }
        else
        {
            Debug.LogWarning("⚠️ Scene1Manager not found in this scene!");
        }
    }

    [MenuItem("Rise Above Dev/3. Gameplay/🌩️ Trigger Thunder Strike")]
    public static void TriggerThunder()
    {
        if (!Application.isPlaying) { Debug.LogWarning("⚠️ You must be in Play Mode!"); return; }

        if (StormController.Instance != null)
        {
            StormController.Instance.TriggerThunder();
            Debug.Log("🌩️ [Dev Tools] Thunder triggered manually!");
        }
        else
        {
            Debug.LogWarning("⚠️ StormController not found in this scene!");
        }
    }

    [MenuItem("Rise Above Dev/3. Gameplay/🫨 Trigger Max Screen Shake")]
    public static void TriggerScreenShake()
    {
        if (!Application.isPlaying) { Debug.LogWarning("⚠️ You must be in Play Mode!"); return; }

        if (UIShake.Instance != null)
        {
            UIShake.Instance.ShakeGameOver(); // Using your biggest shake!
            Debug.Log("🫨 [Dev Tools] Massive screen shake triggered!");
        }
        else
        {
            Debug.LogWarning("⚠️ UIShake not found in this scene!");
        }
    }

    [MenuItem("Rise Above Dev/3. Gameplay/💉 Force Max Survival HP")]
    public static void ForceMaxHP()
    {
        if (!Application.isPlaying) { Debug.LogWarning("⚠️ You must be in Play Mode!"); return; }

        if (SurvivalMeter.Instance != null)
        {
            // Pumping 100 points in ensures it maxes out the meter perfectly
            SurvivalMeter.Instance.HandleEssentialItem(100);
            Debug.Log("💉 [Dev Tools] Max HP restored!");
        }
        else
        {
            Debug.LogWarning("⚠️ SurvivalMeter not found in this scene!");
        }
    }
}