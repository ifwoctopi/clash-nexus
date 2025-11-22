using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Makes a UI circle draggable. Used for character selection.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class DraggableCircle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Player Settings")]
    [Tooltip("Which player this circle belongs to (1 or 2)")]
    [SerializeField] private int playerNumber = 1;
    
    [Header("Visual Settings")]
    [Tooltip("Color of the circle for this player")]
    [SerializeField] private Color circleColor = Color.white;
    
    [Tooltip("Size of the circle")]
    [SerializeField] private Vector2 circleSize = new Vector2(100, 100);

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private CharacterSlot currentSlot;
    private CharacterSelectManager selectionManager;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Find canvas
        canvas = GetComponentInParent<Canvas>();
        
        // Add CanvasGroup for drag functionality
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Set up visual appearance
        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.color = circleColor;
        }
        
        // Set size
        rectTransform.sizeDelta = circleSize;
        
        // Find selection manager
        selectionManager = FindObjectOfType<CharacterSelectManager>();
    }

    void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = rectTransform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Allow dragging
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
        // Move to top of hierarchy so it appears above other elements
        rectTransform.SetAsLastSibling();
        
        // Ensure circle is parented to canvas for dragging
        if (rectTransform.parent != canvas.transform)
        {
            rectTransform.SetParent(canvas.transform, false);
        }
        
        // If placed on a slot, clear the selection for this player only
        if (currentSlot != null)
        {
            // Clear slot selection for this specific player
            currentSlot.ClearSelection(playerNumber);
            
            // Notify manager that player deselected
            if (selectionManager != null)
            {
                selectionManager.OnPlayerDeselected(playerNumber);
            }
            
            currentSlot = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the circle to follow the mouse/touch
        if (canvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPoint);
            
            rectTransform.position = canvas.transform.TransformPoint(localPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore visual state
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Check if we're over a character slot
        CharacterSlot slot = GetSlotUnderPointer(eventData);
        
        if (slot != null && slot.CanSelect(playerNumber))
        {
            // Place circle on the slot
            PlaceOnSlot(slot);
        }
        else
        {
            // If we were previously on a slot, return to original position
            // Otherwise, stay where we are (allows free positioning)
            if (rectTransform.parent == canvas.transform)
            {
                ReturnToOriginalPosition();
            }
        }
    }

    private CharacterSlot GetSlotUnderPointer(PointerEventData eventData)
    {
        // Use EventSystem to find what's under the pointer
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            CharacterSlot slot = result.gameObject.GetComponent<CharacterSlot>();
            if (slot != null)
            {
                return slot;
            }
        }
        
        return null;
    }

    private void PlaceOnSlot(CharacterSlot slot)
    {
        // Clear previous slot if switching (only for this player)
        if (currentSlot != null && currentSlot != slot)
        {
            currentSlot.ClearSelection(playerNumber);
        }
        
        currentSlot = slot;
        slot.SetSelection(this, playerNumber);
        
        // Get the slot's RectTransform
        RectTransform slotRect = slot.GetComponent<RectTransform>();
        
        // Keep circle as child of canvas (don't parent to slot to avoid disappearing)
        if (rectTransform.parent != canvas.transform)
        {
            rectTransform.SetParent(canvas.transform, false);
        }
        
        // Set anchors and pivot to center for proper positioning
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // Position circle over the slot using world position
        if (slotRect != null)
        {
            // Get the slot's world position and place circle there
            Vector3 slotWorldPos = slotRect.position;
            rectTransform.position = slotWorldPos;
        }
        else
        {
            // Fallback: use the slot's transform position
            rectTransform.position = slot.transform.position;
        }
        
        // Make sure the circle is visible (bring to front)
        rectTransform.SetAsLastSibling();
        
        // Ensure the circle stays visible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Notify selection manager
        if (selectionManager != null)
        {
            selectionManager.OnPlayerSelected(playerNumber, slot);
        }
    }

    private void ReturnToOriginalPosition()
    {
        rectTransform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        
        // Notify selection manager that player deselected
        if (selectionManager != null)
        {
            selectionManager.OnPlayerDeselected(playerNumber);
        }
    }

    /// <summary>
    /// Gets the player number this circle belongs to
    /// </summary>
    public int GetPlayerNumber()
    {
        return playerNumber;
    }

    /// <summary>
    /// Gets the current slot this circle is placed on (null if not placed)
    /// </summary>
    public CharacterSlot GetCurrentSlot()
    {
        return currentSlot;
    }

    /// <summary>
    /// Resets the circle to its original position
    /// </summary>
    public void ResetPosition()
    {
        if (currentSlot != null)
        {
            currentSlot.ClearSelection(playerNumber);
            currentSlot = null;
        }
        ReturnToOriginalPosition();
    }
}

