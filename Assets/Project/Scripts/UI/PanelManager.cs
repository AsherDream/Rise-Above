using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [Header("Fallback Panel")]
    public GameObject pausePanel; // fallback if no previous panel exists

    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    /// <summary>
    /// Call this to open a new panel and optionally hide the current one.
    /// Automatically remembers the current panel in history.
    /// </summary>
    public void OpenPanel(GameObject newPanel, GameObject currentPanel = null)
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            panelHistory.Push(currentPanel); // remember where we came from
        }

        newPanel.SetActive(true);
    }

    /// <summary>
    /// Call this on any Back button.
    /// Hides the current panel and shows the previous one from history.
    /// If history is empty, shows the fallback panel (usually pause menu).
    /// </summary>
    public void OnBackButton(GameObject currentPanel)
    {
        currentPanel.SetActive(false);

        if (panelHistory.Count > 0)
        {
            GameObject previousPanel = panelHistory.Pop();
            previousPanel.SetActive(true);
        }
        else
        {
            // fallback
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Optional: call this to clear panel history
    /// Useful when loading a new scene or resetting UI.
    /// </summary>
    public void ClearHistory()
    {
        panelHistory.Clear();
    }
}
