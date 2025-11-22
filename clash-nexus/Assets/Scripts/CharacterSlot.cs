using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a character slot that can be selected by dragging a circle onto it.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CharacterSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [Tooltip("The character ID or name this slot represents")]
    [SerializeField] private string characterId;
    
    [Tooltip("Visual indicator when selected (optional)")]
    [SerializeField] private Image selectionIndicator;

    private DraggableCircle player1Circle;
    private DraggableCircle player2Circle;

    void Awake()
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.enabled = false;
        }
    }

    /// <summary>
    /// Checks if this slot can be selected by the given player
    /// </summary>
    public bool CanSelect(int playerNumber)
    {
        // Always allow selection - both players can select the same character
        return true;
    }

    /// <summary>
    /// Sets the selection for this slot
    /// </summary>
    public void SetSelection(DraggableCircle circle, int playerNumber)
    {
        // Store circle for the appropriate player
        if (playerNumber == 1)
        {
            player1Circle = circle;
        }
        else if (playerNumber == 2)
        {
            player2Circle = circle;
        }
        
        // Show selection indicator if any player has selected
        if (selectionIndicator != null)
        {
            selectionIndicator.enabled = (player1Circle != null || player2Circle != null);
        }
    }

    /// <summary>
    /// Clears the selection from this slot for a specific player
    /// </summary>
    public void ClearSelection(int playerNumber)
    {
        if (playerNumber == 1)
        {
            player1Circle = null;
        }
        else if (playerNumber == 2)
        {
            player2Circle = null;
        }
        
        // Hide selection indicator only if no players have selected
        if (selectionIndicator != null)
        {
            selectionIndicator.enabled = (player1Circle != null || player2Circle != null);
        }
    }

    /// <summary>
    /// Clears all selections from this slot (for backwards compatibility)
    /// </summary>
    public void ClearSelection()
    {
        player1Circle = null;
        player2Circle = null;
        
        // Hide selection indicator
        if (selectionIndicator != null)
        {
            selectionIndicator.enabled = false;
        }
    }

    /// <summary>
    /// Gets the character ID this slot represents
    /// </summary>
    public string GetCharacterId()
    {
        return characterId;
    }

    /// <summary>
    /// Gets the player number that has selected this slot (0 if none, or bitwise: 1=player1, 2=player2, 3=both)
    /// </summary>
    public int GetSelectedPlayer()
    {
        int result = 0;
        if (player1Circle != null) result |= 1;
        if (player2Circle != null) result |= 2;
        return result;
    }

    /// <summary>
    /// Checks if a specific player has selected this slot
    /// </summary>
    public bool IsSelectedByPlayer(int playerNumber)
    {
        if (playerNumber == 1) return player1Circle != null;
        if (playerNumber == 2) return player2Circle != null;
        return false;
    }

    /// <summary>
    /// Checks if this slot is selected by any player
    /// </summary>
    public bool IsSelected()
    {
        return player1Circle != null || player2Circle != null;
    }
}

