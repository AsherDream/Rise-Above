using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class EndScreenSlot : MonoBehaviour, IPointerClickHandler
{
    [ReadOnly] public InspectableItemData myData;
    private EndScreenManager manager;

    public void Setup(InspectableItemData data, EndScreenManager screenManager)
    {
        myData = data;
        manager = screenManager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // When clicked, tell the manager to show THIS item's details
        if (manager != null && myData != null)
        {
            manager.ShowItemDetails(myData);
        }
    }
}