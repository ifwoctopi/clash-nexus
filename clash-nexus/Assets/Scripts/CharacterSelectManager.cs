using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages character selection state and enables/disables the ready button.
/// </summary>
public class CharacterSelectManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The ready button that should be enabled when both players have selected")]
    [SerializeField] private Button readyButton;
    
    [Tooltip("Text to show selection status (optional)")]
    [SerializeField] private UnityEngine.UI.Text statusText;

    [Header("Player Circles")]
    [Tooltip("Circle for Player 1")]
    [SerializeField] private DraggableCircle player1Circle;
    
    [Tooltip("Circle for Player 2")]
    [SerializeField] private DraggableCircle player2Circle;

    private bool player1Selected = false;
    private bool player2Selected = false;
    private CharacterSlot player1Slot;
    private CharacterSlot player2Slot;

    void Start()
    {
        UpdateReadyButton();
        UpdateStatusText();
        
        // Connect ready button to StartGame method
        // Note: We add our listener without removing all listeners to preserve ButtonSoundPlayer
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(StartGame);
        }
    }

    /// <summary>
    /// Called when a player selects a character slot
    /// </summary>
    public void OnPlayerSelected(int playerNumber, CharacterSlot slot)
    {
        if (playerNumber == 1)
        {
            player1Selected = true;
            player1Slot = slot;
        }
        else if (playerNumber == 2)
        {
            player2Selected = true;
            player2Slot = slot;
        }
        
        UpdateReadyButton();
        UpdateStatusText();
    }

    /// <summary>
    /// Called when a player deselects (removes their circle)
    /// </summary>
    public void OnPlayerDeselected(int playerNumber)
    {
        if (playerNumber == 1)
        {
            player1Selected = false;
            player1Slot = null;
        }
        else if (playerNumber == 2)
        {
            player2Selected = false;
            player2Slot = null;
        }
        
        UpdateReadyButton();
        UpdateStatusText();
    }

    private void UpdateReadyButton()
    {
        if (readyButton != null)
        {
            readyButton.interactable = player1Selected && player2Selected;
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            if (player1Selected && player2Selected)
            {
                statusText.text = "Both players ready!";
            }
            else if (player1Selected)
            {
                statusText.text = "Player 1 ready. Waiting for Player 2...";
            }
            else if (player2Selected)
            {
                statusText.text = "Player 2 ready. Waiting for Player 1...";
            }
            else
            {
                statusText.text = "Select your characters";
            }
        }
    }

    /// <summary>
    /// Gets the selected character ID for a player
    /// </summary>
    public string GetSelectedCharacter(int playerNumber)
    {
        if (playerNumber == 1 && player1Slot != null)
        {
            return player1Slot.GetCharacterId();
        }
        else if (playerNumber == 2 && player2Slot != null)
        {
            return player2Slot.GetCharacterId();
        }
        return "";
    }

    /// <summary>
    /// Checks if both players have selected
    /// </summary>
    public bool BothPlayersReady()
    {
        return player1Selected && player2Selected;
    }

    /// <summary>
    /// Saves the selected characters to GameDataManager and transitions to the game scene
    /// </summary>
    public void StartGame()
    {
        if (!BothPlayersReady())
        {
            Debug.LogWarning("CharacterSelectManager: Cannot start game - not all players have selected");
            return;
        }

        // Save selections to GameDataManager
        GameDataManager dataManager = GameDataManager.Instance;
        if (player1Slot != null)
        {
            dataManager.SetPlayerCharacter(1, player1Slot.GetCharacterId());
        }
        if (player2Slot != null)
        {
            dataManager.SetPlayerCharacter(2, player2Slot.GetCharacterId());
        }

        // Transition to ChallengeSystem scene first
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChallengeSystem");
    }
}

