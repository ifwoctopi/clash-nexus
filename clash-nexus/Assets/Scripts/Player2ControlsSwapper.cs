using UnityEngine;
using System.Reflection;

/// <summary>
/// Swaps Player1Controls for Player2Controls on Player 2 characters in 2-player mode.
/// This component intercepts Player1Controls input and replaces it with arrow key input.
/// </summary>
public class Player2ControlsSwapper : MonoBehaviour
{
    private MonoBehaviour characterController;
    private FieldInfo controlsField;
    private FieldInfo moveInputField;
    private Player1Controls originalControls;
    private bool controlsDisabled = false;

    private void Start()
    {
        // Wait a frame to ensure all controllers have initialized
        StartCoroutine(SwapControlsDelayed());
    }

    private System.Collections.IEnumerator SwapControlsDelayed()
    {
        yield return null; // Wait one frame
        InitializeSwapper();
    }

    private void InitializeSwapper()
    {
        // Find the character controller component
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        
        foreach (MonoBehaviour component in components)
        {
            if (component == null || component == this) continue;
            
            // Look for a controller that has Player1Controls
            controlsField = component.GetType().GetField("controls", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            
            if (controlsField != null && controlsField.FieldType == typeof(Player1Controls))
            {
                characterController = component;
                originalControls = controlsField.GetValue(component) as Player1Controls;
                
                // Get moveInput field
                moveInputField = component.GetType().GetField("moveInput", 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                
                // Disable original controls
                if (originalControls != null)
                {
                    originalControls.Disable();
                    controlsDisabled = true;
                }
                
                Debug.Log($"Player2ControlsSwapper: Initialized for {component.GetType().Name}");
                break;
            }
        }
    }

    private void Update()
    {
        if (characterController == null || !controlsDisabled) return;

        // Read arrow key input and update moveInput
        Vector2 moveInput = Vector2.zero;
        if (Input.GetKey(KeyCode.LeftArrow)) moveInput.x = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput.x = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) moveInput.y = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveInput.y = -1f;
        
        if (moveInputField != null)
        {
            moveInputField.SetValue(characterController, moveInput);
        }

        // Handle jump (Numpad 0)
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            MethodInfo jumpMethod = characterController.GetType().GetMethod("Jump", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (jumpMethod != null)
            {
                jumpMethod.Invoke(characterController, null);
            }
        }

        // Handle attacks
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            MethodInfo attack1Method = characterController.GetType().GetMethod("Attack1", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (attack1Method != null)
            {
                attack1Method.Invoke(characterController, null);
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            MethodInfo attack2Method = characterController.GetType().GetMethod("Attack2", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (attack2Method != null)
            {
                attack2Method.Invoke(characterController, null);
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            MethodInfo attack3Method = characterController.GetType().GetMethod("Attack3", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (attack3Method != null)
            {
                attack3Method.Invoke(characterController, null);
            }
        }

        // Handle defend/dash (Numpad 5)
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            MethodInfo tryDashMethod = characterController.GetType().GetMethod("TryDash", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (tryDashMethod != null)
            {
                tryDashMethod.Invoke(characterController, null);
            }
            else
            {
                // Some controllers use isDefending field
                FieldInfo isDefendingField = characterController.GetType().GetField("isDefending", 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (isDefendingField != null)
                {
                    isDefendingField.SetValue(characterController, true);
                }
            }
        }
        
        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            FieldInfo isDefendingField = characterController.GetType().GetField("isDefending", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (isDefendingField != null)
            {
                isDefendingField.SetValue(characterController, false);
            }
        }
    }

    private void OnDestroy()
    {
        // Re-enable original controls if needed
        if (originalControls != null && controlsDisabled)
        {
            originalControls.Enable();
        }
    }
}

