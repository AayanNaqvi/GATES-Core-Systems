using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Inventory inventoryManager;

    private RectTransform myRectTransform;

    void Start()
    {
        myRectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryManager != null)
        {
            inventoryManager.SetHoverTarget(myRectTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryManager != null)
        {
            inventoryManager.ClearHoverTarget(myRectTransform);
        }
    }
}
