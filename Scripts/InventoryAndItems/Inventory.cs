using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
public class Inventory : MonoBehaviour
{
    [Header("References")]
    private PlayerInput playerInput;
    private InputAction openAction;
    private InputAction closeAction;
    private InputAction useQuickAccess;
    private InputAction useQuickAccess2;
    private InputAction dropAction;
    private InputAction useAction;


    private int currentHoveredIndex = -1;

    [Header("Inventory")]
    public Item[] items = new Item[7];
    public int[] itemAmounts = new int[7];
    public GameObject droppedItem;


    [Header("Action Menu")]
    public GameObject actionMenu;
    public Button useButton;      
    public Button dropButton;
    public Button cancelButton;

    private int selectedSlotIndex = -1;

    [Header("Menu Animation")]
    public CanvasGroup actionMenuCanvasGroup; 
    public float menuAnimDuration = 0.2f;     
    public Vector3 menuSlideOffset = new Vector3(-120f, 0f, 0f);
    public float iconPopScale = 1.2f;
    private Coroutine menuAnimCoroutine;

    [Header("Hover Selector")]
    public RectTransform hoverSelector;   
    public float selectorSpeed = 15f;     
    private RectTransform currentTarget;

    [Header("Item Swapping")]
    public RectTransform dragGhostIcon; 
    private Image dragGhostImage;       
    private int draggingSlotIndex = -1;

    [Header("UI")]
    public Image quickAccessImage;
    public TextMeshProUGUI quickAccessText;
    public Image quickAccessImage2;
    public TextMeshProUGUI quickAccessText2;
    public RectTransform inventoryPanel;
    public Vector2 offScreenPosition = new Vector2(1200f, 0f);
    public Vector2 onScreenPosition = new Vector2(0f, 0f);
    public float slideDuration = 0.35f;
    private Coroutine slideCoroutine;
    public Image[] inventorySprites;
    public TextMeshProUGUI[] itemAmountTexts;

    public Sprite backpackClose;
    public Sprite backpackOpen;
    public Image backpack;


    [Header("Item Description")]
    public GameObject itemBox;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemRarityText;
    public Image itemIcon;


    private bool currentlySliding = false;

    public float quickAccessCooldown = 0.5f;
    private float nextQuickAccessTime = 0f;

     

    public bool inventoryOpen = false;

    public GameObject promptHolder;


    [Header("Audio")]
    public AudioClip inventOpen;
    public AudioClip inventClose;
    public AudioClip dropItem;
    public float soundVol = 0.3f;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        openAction = playerInput.actions.FindAction("Player/OpenInventory");
        closeAction = playerInput.actions.FindAction("UI/CloseInventory");
        useQuickAccess = playerInput.actions.FindAction("Player/UseQuickAccess");
        useQuickAccess2 = playerInput.actions.FindAction("Player/UseQuickAccess2");
        dropAction = playerInput.actions.FindAction("UI/InventoryDrop");
        useAction = playerInput.actions.FindAction("UI/InventoryUse");

        itemBox.SetActive(false);
        RefreshUI();

        if (inventoryPanel != null)
        {
            inventoryPanel.anchoredPosition = offScreenPosition;
        }

        if (actionMenu != null) actionMenu.SetActive(false);
        if (dragGhostIcon != null) dragGhostImage = dragGhostIcon.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        MyInput();

        promptHolder.SetActive(!inventoryOpen);

        
        if (!inventoryOpen || currentlySliding)
        {
            if (hoverSelector.gameObject.activeSelf) hoverSelector.gameObject.SetActive(false);
            if (itemBox.activeSelf) itemBox.SetActive(false);
            if (actionMenu.activeSelf) actionMenu.SetActive(false);
            return;
        }

        
        if (IsGamepad())
        {
            GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
            if (selectedObj != null)
            {
                bool isSlot = false;
                for (int i = 0; i < inventorySprites.Length; i++)
                {
                    if (inventorySprites[i].transform.parent.gameObject == selectedObj)
                    {
                        SetHoverTarget(inventorySprites[i].rectTransform);
                        isSlot = true;
                        break;
                    }
                }

                if (!isSlot)
                {
                    if (hoverSelector.gameObject.activeSelf) hoverSelector.gameObject.SetActive(false);
                    if (itemBox.activeSelf) itemBox.SetActive(false);
                    if (actionMenu.activeSelf) actionMenu.SetActive(false);
                }
            }
        }

        
        if (currentTarget != null)
        {
            if (!hoverSelector.gameObject.activeSelf)
            {
                hoverSelector.gameObject.SetActive(true);
            }

            hoverSelector.position = Vector3.Lerp(hoverSelector.position, currentTarget.position, Time.unscaledDeltaTime * selectorSpeed);

            if (IsGamepad())
            {
                
                if (dragGhostIcon.gameObject.activeSelf)
                {
                    dragGhostIcon.position = currentTarget.position;
                }
                if (draggingSlotIndex == -1 && currentHoveredIndex != -1 && items[currentHoveredIndex] != null)
                {
                    if (!actionMenu.activeSelf)
                    {
                        actionMenu.SetActive(true);
                        if (actionMenuCanvasGroup != null) actionMenuCanvasGroup.alpha = 1f;
                    }

                    RectTransform actionRect = actionMenu.GetComponent<RectTransform>();
                    Vector3 scaledOffset = new Vector3(menuSlideOffset.x * currentTarget.lossyScale.x, menuSlideOffset.y * currentTarget.lossyScale.y, 0f);
                    actionRect.position = currentTarget.position + scaledOffset;
                }
                else
                {
                    
                    actionMenu.SetActive(false);
                }
            }
        }
        else
        {
            if (hoverSelector.gameObject.activeSelf)
            {
                hoverSelector.gameObject.SetActive(false);
            }

            if (itemBox.activeSelf)
            {
                itemBox.SetActive(false);
            }

            if (IsGamepad() && actionMenu.activeSelf)
            {
                actionMenu.SetActive(false);
            }
        }
    }

    public void MyInput()
    {
        if (GetComponent<UpgradeStuff>().menuOpen) return;
        
        
        if(!currentlySliding)
        {
            if (!inventoryOpen && openAction != null && openAction.WasPressedThisFrame())
            {
                ToggleInventory(true);
                AudioManager.Instance.Play2D(inventOpen, soundVol);
                backpack.sprite = backpackOpen;
            }
            else if (inventoryOpen && closeAction != null && closeAction.WasPressedThisFrame())
            {

                

                if (IsGamepad())
                {
                    if (draggingSlotIndex != -1)
                    {
                        dragGhostIcon.gameObject.SetActive(false);
                        draggingSlotIndex = -1;
                        RefreshUI();
                    }
                    else
                    {
                        AudioManager.Instance.Play2D(inventClose, soundVol);
                        ToggleInventory(false);
                        backpack.sprite = backpackClose;
                    }
                }
                else
                {
                    if (actionMenu.activeSelf)
                    {
                        CloseActionMenu();

                        if (selectedSlotIndex != -1)
                        {
                            GameObject slot = inventorySprites[selectedSlotIndex].transform.parent.gameObject;
                            EventSystem.current.SetSelectedGameObject(slot);
                        }
                    }
                    else
                    {
                        AudioManager.Instance.Play2D(inventClose, soundVol);
                        ToggleInventory(false);
                        backpack.sprite = backpackClose;
                    }
                }
            }

            if (inventoryOpen && currentHoveredIndex != -1 && IsGamepad())
            {
                if (dropAction != null && dropAction.WasPressedThisFrame())
                {
                    selectedSlotIndex = currentHoveredIndex;
                    
                    DropItem();
                }
                if (useAction != null && useAction.WasPressedThisFrame())
                {
                    selectedSlotIndex = currentHoveredIndex;
                    UseItemInSlot();
                }
            }
        }
    


        if (useQuickAccess == null) return;
        if(useQuickAccess.WasPressedThisFrame())
        {
            UseFirstItem();
        }
        if (useQuickAccess2 == null) return;
        if (useQuickAccess2.WasPressedThisFrame())
        {
            UseSecondItem();
        }
    }

    public void UseFirstItem()
    {
        if (items[0] == null) return;

        if (Time.time < nextQuickAccessTime)
        {
            return;
        }

        items[0].Use(this.gameObject);

        itemAmounts[0]--; 
        if (itemAmounts[0] <= 0)
        {
            items[0] = null; 
        }
        RefreshUI();
        nextQuickAccessTime = Time.time + quickAccessCooldown;
    }

    public void UseSecondItem()
    {
        if (items[1] == null) return;

        if (Time.time < nextQuickAccessTime)
        {
            return;
        }

        items[1].Use(this.gameObject);

        itemAmounts[1]--;
        if (itemAmounts[1] <= 0)
        {
            items[1] = null;
        }
        RefreshUI();
        nextQuickAccessTime = Time.time + quickAccessCooldown;
    }


    public void ToggleInventory(bool open)
    {
        inventoryOpen = open;
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);

        if (inventoryOpen)
        {
            playerInput.SwitchCurrentActionMap("UI");
            slideCoroutine = StartCoroutine(SlidePanel(onScreenPosition));

            if (IsGamepad() && inventorySprites.Length > 0 && inventorySprites[0] != null)
            {
                GameObject firstSlot = inventorySprites[0].transform.parent.gameObject;
                EventSystem.current.SetSelectedGameObject(firstSlot);
            }
        }
        else
        {
            CloseActionMenu();

            if (draggingSlotIndex != -1)
            {
                dragGhostIcon.gameObject.SetActive(false);
                draggingSlotIndex = -1;
                RefreshUI();
            }

            EventSystem.current.SetSelectedGameObject(null);
            currentTarget = null;

            playerInput.SwitchCurrentActionMap("Player");
            slideCoroutine = StartCoroutine(SlidePanel(offScreenPosition));
        }
    }

    

    public bool AddItem(Item itemToAdd)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == itemToAdd && itemAmounts[i] < itemToAdd.maxStackSize)
            {
                itemAmounts[i]++;
                RefreshUI();
                return true;
            }
        }
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = itemToAdd;
                itemAmounts[i] = 1;

                RefreshUI();
                return true;
            }
        }

        return false;
    }

    public void RefreshUI()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                inventorySprites[i].sprite = items[i].icon;
                inventorySprites[i].color = new Color(1f, 1f, 1f, 1f);
                inventorySprites[i].preserveAspect = true;
                inventorySprites[i].GetComponentInParent<Button>().interactable = true;

                if (itemAmountTexts.Length > i && itemAmountTexts[i] != null)
                {
                    itemAmountTexts[i].text = itemAmounts[i] > 1 ? itemAmounts[i].ToString() : "";
                }
            }
            else
            {
                inventorySprites[i].sprite = null;
                inventorySprites[i].color = new Color(1f, 1f, 1f, 0f);
                inventorySprites[i].GetComponentInParent<Button>().interactable = true;

                if (itemAmountTexts.Length > i && itemAmountTexts[i] != null)
                {
                    itemAmountTexts[i].text = "";
                }
            }
        }

        SetFirstItemToQuickAccess();
        SetSecondItemToQuickAccess();
    }

    private void SetFirstItemToQuickAccess()
    {
        if (items[0] == null)
        {
            quickAccessImage.sprite = null;
            quickAccessImage.color = new Color(1f, 1f, 1f, 0f);
            quickAccessText.text = "";

        }
        else
        {
            quickAccessImage.sprite = items[0].icon;
            quickAccessImage.color = new Color(1f, 1f, 1f, 1f);
            quickAccessImage.preserveAspect = true;

            quickAccessText.text = itemAmounts[0] > 1 ? itemAmounts[0].ToString() : "";
        }
    }

    private void SetSecondItemToQuickAccess()
    {
        if (items[1] == null)
        {
            quickAccessImage2.sprite = null;
            quickAccessImage2.color = new Color(1f, 1f, 1f, 0f);
            quickAccessText2.text = "";

        }
        else
        {
            quickAccessImage2.sprite = items[1].icon;
            quickAccessImage2.color = new Color(1f, 1f, 1f, 1f);
            quickAccessImage2.preserveAspect = true;

            quickAccessText2.text = itemAmounts[1] > 1 ? itemAmounts[1].ToString() : "";
        }
    }

    public void DropItem()
    {
        if (selectedSlotIndex == -1 || items[selectedSlotIndex] == null) return;

        AudioManager.Instance.Play2D(dropItem, soundVol);

        if (draggingSlotIndex == selectedSlotIndex)
        {
            dragGhostIcon.gameObject.SetActive(false);
            draggingSlotIndex = -1;
        }

        Item itemToDrop = items[selectedSlotIndex];

        int amountToDrop = itemAmounts[selectedSlotIndex];

        for (int i = 0; i < amountToDrop; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
            Vector3 spawnPos = transform.position;
            spawnPos.z = 0f;

            GameObject droppedObj = Instantiate(droppedItem, spawnPos, Quaternion.identity);
            droppedObj.GetComponent<Rigidbody2D>().AddForce(randomOffset * 7.5f, ForceMode2D.Impulse);

            PickUpItem pickup = droppedObj.GetComponent<PickUpItem>();
            if (pickup != null)
            {
                pickup.Setup(itemToDrop);
            }
        }

        itemAmounts[selectedSlotIndex] = 0;
        items[selectedSlotIndex] = null;

        RefreshUI();
        if (IsGamepad()) actionMenu.SetActive(false);
        else CloseActionMenu();

    }

    public void BeginDrag(int startIndex)
    {
        
        if (items[startIndex] == null) return;

        draggingSlotIndex = startIndex;

        
        dragGhostImage.sprite = items[startIndex].icon;
        dragGhostImage.preserveAspect = true;
        dragGhostIcon.gameObject.SetActive(true);

        
        inventorySprites[startIndex].color = new Color(1f, 1f, 1f, 0.5f);


        actionMenu.SetActive(false);

        if (!IsGamepad()) CloseActionMenu();
    }

    public void UpdateDragPosition(Vector2 mousePosition)
    {
        if (dragGhostIcon.gameObject.activeSelf)
        {
            dragGhostIcon.position = mousePosition;
        }
    }

    public void StopDrag(PointerEventData eventData)
    {
        dragGhostIcon.gameObject.SetActive(false);

        
        GameObject hitUIObject = eventData.pointerCurrentRaycast.gameObject;
        bool droppedOutsideInventory = hitUIObject == null || !hitUIObject.transform.IsChildOf(inventoryPanel);

        if (droppedOutsideInventory)
        {
            selectedSlotIndex = draggingSlotIndex;

            DropItem();
        }
        draggingSlotIndex = -1;
        RefreshUI();
    }

    public void SwapItems(int startIndex, int endIndex)
    {

        if (startIndex == endIndex) return;

        if (items[startIndex] != null && items[startIndex] == items[endIndex])
        {
            int maxStack = items[startIndex].maxStackSize;
            int spaceInTarget = maxStack - itemAmounts[endIndex];

            if (spaceInTarget > 0)
            {
                int amountToMove = Mathf.Min(spaceInTarget, itemAmounts[startIndex]);

                itemAmounts[endIndex] += amountToMove;
                itemAmounts[startIndex] -= amountToMove;

                if (itemAmounts[startIndex] <= 0) items[startIndex] = null;

                RefreshUI();
                return;
            }
        }

        Item tempItem = items[startIndex];
        int tempAmount = itemAmounts[startIndex];

        items[startIndex] = items[endIndex];
        itemAmounts[startIndex] = itemAmounts[endIndex];

        items[endIndex] = tempItem;
        itemAmounts[endIndex] = tempAmount;

        RefreshUI();
    }


    public void UseItemInSlot()
    {
        if (selectedSlotIndex == -1 || items[selectedSlotIndex] == null) return;


        items[selectedSlotIndex].Use(this.gameObject);

        itemAmounts[selectedSlotIndex]--;
        if (itemAmounts[selectedSlotIndex] <= 0)
        {
            items[selectedSlotIndex] = null;

            if (draggingSlotIndex == selectedSlotIndex)
            {
                dragGhostIcon.gameObject.SetActive(false);
                draggingSlotIndex = -1;
            }
        }

        RefreshUI();
        if (IsGamepad() && items[selectedSlotIndex] == null) actionMenu.SetActive(false);
        else CloseActionMenu();
    }

    public void ClearAllSlots()
    {
        for(int i = 0; i < items.Length; i++)
        {
            items[i] = null;
            itemAmounts[i] = 0;
        }
        RefreshUI();    
    }


    #region UIEffects
    private IEnumerator SlidePanel(Vector2 targetPosition)
    {
        currentlySliding = true;
        Vector2 startPosition = inventoryPanel.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / slideDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            
            inventoryPanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedT);

            yield return null; 
        }

        inventoryPanel.anchoredPosition = targetPosition;
        

        currentlySliding = false;
    }



    public void OnSlotClicked(int slotIndex)
    {
        if (!inventoryOpen || currentlySliding) return;

        if (IsGamepad())
        {
            if (draggingSlotIndex == -1)
            {
                if (items[slotIndex] != null) BeginDrag(slotIndex);
            }
            else
            {
                SwapItems(draggingSlotIndex, slotIndex);
                dragGhostIcon.gameObject.SetActive(false);
                draggingSlotIndex = -1;
                RefreshUI();
            }
        }
        else
        {
            if (items[slotIndex] == null) return; 

            if (slotIndex == selectedSlotIndex)
            {
                CloseActionMenu();
                return;
            }

            if (selectedSlotIndex != -1)
            {
                inventorySprites[selectedSlotIndex].rectTransform.localScale = Vector3.one;
            }

            selectedSlotIndex = slotIndex;
            actionMenu.SetActive(true);

            if (menuAnimCoroutine != null) StopCoroutine(menuAnimCoroutine);

            RectTransform slotRect = inventorySprites[slotIndex].rectTransform;
            menuAnimCoroutine = StartCoroutine(AnimateMenuIn(slotRect));

            EventSystem.current.SetSelectedGameObject(useButton.gameObject);
        }
    }

    
    public void CloseActionMenu()
    {
        if (menuAnimCoroutine != null) StopCoroutine(menuAnimCoroutine);

        if (actionMenu.activeSelf && selectedSlotIndex != -1 && !IsGamepad())
        {
            menuAnimCoroutine = StartCoroutine(AnimateMenuOut());
        }
        else
        {
            actionMenu.SetActive(false);
            selectedSlotIndex = -1;
        }
    }


    private IEnumerator AnimateMenuIn(RectTransform slotRect)
    {
        RectTransform actionRect = actionMenu.GetComponent<RectTransform>();
        Vector3 startPos = slotRect.position;
        
        Vector3 scaledOffset = new Vector3(menuSlideOffset.x * slotRect.lossyScale.x, menuSlideOffset.y * slotRect.lossyScale.y, 0f);
        Vector3 endPos = slotRect.position + scaledOffset;

        Vector3 startScale = slotRect.localScale;
        Vector3 endScale = new Vector3(iconPopScale, iconPopScale, 1f);


        float elapsed = 0f;

        while (elapsed < menuAnimDuration)
        {
            
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / menuAnimDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            actionRect.position = Vector3.Lerp(startPos, endPos, easedT);

            slotRect.localScale = Vector3.Lerp(startScale, endScale, easedT);

            if (actionMenuCanvasGroup != null)
            {
                actionMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, easedT);
            }

            yield return null;
        }

        actionRect.position = endPos;
        if (actionMenuCanvasGroup != null) actionMenuCanvasGroup.alpha = 1f;
    }


    private IEnumerator AnimateMenuOut()
    {
        RectTransform actionRect = actionMenu.GetComponent<RectTransform>();
        RectTransform slotRect = inventorySprites[selectedSlotIndex].rectTransform;

        Vector3 startPos = actionRect.position;
        Vector3 endPos = slotRect.position;


        Vector3 startScale = slotRect.localScale;
        Vector3 endScale = Vector3.one;

        float startAlpha = actionMenuCanvasGroup != null ? actionMenuCanvasGroup.alpha : 1f;
        float elapsed = 0f;

        while (elapsed < menuAnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / menuAnimDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            
            actionRect.position = Vector3.Lerp(startPos, endPos, easedT);
            slotRect.localScale = Vector3.Lerp(startScale, endScale, easedT);


            if (actionMenuCanvasGroup != null)
            {
                actionMenuCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, easedT);
            }

            yield return null;
        }
        actionMenu.SetActive(false);
        selectedSlotIndex = -1;
    }


    public void SetHoverTarget(RectTransform targetSlot)
    {
        currentTarget = targetSlot;
        currentHoveredIndex = -1;


        for (int i = 0; i < inventorySprites.Length; i++)
        {
            if (inventorySprites[i].rectTransform == targetSlot || inventorySprites[i].transform.parent.GetComponent<RectTransform>() == targetSlot)
            {
                currentHoveredIndex = i;
                break;
            }
        }

        if (currentHoveredIndex != -1 && items[currentHoveredIndex] != null)
        {
            itemBox.SetActive(true);

            Item hoveredItem = items[currentHoveredIndex];

            itemNameText.text = hoveredItem.itemName;
            itemDescriptionText.text = hoveredItem.description;
            itemIcon.sprite = hoveredItem.icon;

            itemRarityText.text = "Rarity: " + hoveredItem.rarity.ToString();

            switch (hoveredItem.rarity)
            {
                case Item.Rarity.Common:
                    itemRarityText.color = Color.white;
                    break;
                case Item.Rarity.Rare:
                    itemRarityText.color = new Color(0.2f, 0.6f, 1f); 
                    break;
                case Item.Rarity.Epic:
                    itemRarityText.color = new Color(0.8f, 0.2f, 1f); 
                    break;
                case Item.Rarity.Legendary:
                    itemRarityText.color = new Color(1f, 0.8f, 0.2f);
                    break;
            }
        }
        else
        {
            itemBox.SetActive(false);
        }


    }

    private bool IsGamepad()
    {
        return playerInput.currentControlScheme == "Gamepad";
    }


    public void ClearHoverTarget(RectTransform slotWeJustLeft)
    {
        
        if (currentTarget == slotWeJustLeft)
        {
            currentTarget = null;
        }
    }

    #endregion
}
