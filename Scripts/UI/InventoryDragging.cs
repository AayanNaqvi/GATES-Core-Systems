using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDragging : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Inventory inventoryManager;
    public int slotIndex;
    public void OnBeginDrag(PointerEventData eventData)
    {
        inventoryManager.BeginDrag(slotIndex);
    }

    
    public void OnDrag(PointerEventData eventData)
    {
        
        inventoryManager.UpdateDragPosition(eventData.position);
    }

    
    public void OnEndDrag(PointerEventData eventData)
    {
        inventoryManager.StopDrag(eventData);
    }

    
    public void OnDrop(PointerEventData eventData)
    {
        InventoryDragging draggedSlot = eventData.pointerDrag.GetComponent<InventoryDragging>();

        if (draggedSlot != null)
        {
            inventoryManager.SwapItems(draggedSlot.slotIndex, slotIndex);
        }
    }
}
