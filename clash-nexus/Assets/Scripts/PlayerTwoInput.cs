using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTwoInput : MonoBehaviour
{
    [Header("Player Configuration")]
    public int playerNumber = 1; // Used to identify the player (e.g., P1 or P2)
    public bool canBlock = false; // Toggle for characters that can block

    [Header("Input Key Codes")]
    // Movement
    public KeyCode moveLeftKey = KeyCode.LeftArrow;
    public KeyCode moveRightKey = KeyCode.RightArrow;

    // Attacks
    public KeyCode lightAttackKey = KeyCode.LeftControl;
    public KeyCode heavyAttackKey = KeyCode.LeftShift;
    public KeyCode specialAttackKey = KeyCode.Keypad0;

    // Defense
    public KeyCode blockKey = KeyCode.DownArrow; 

    // Public properties to check the state of inputs
    public float HorizontalInput { get; private set; }
    public bool IsLightAttackPressed { get; private set; }
    public bool IsHeavyAttackPressed { get; private set; }
    public bool IsSpecialAttackPressed { get; private set; }
    public bool IsBlocking { get; private set; }

    void Update()
    {
        // 1. --- Movement Input (Left/Right) ---
        HorizontalInput = 0f;
        
        if (Input.GetKey(moveLeftKey))
        {
            HorizontalInput = -1f;
            Debug.Log("Left direction input");
        }
        else if (Input.GetKey(moveRightKey))
        {
            HorizontalInput = 1f;
            Debug.Log("Right direction input");
        }

        // 2. --- Attack Inputs (GetKeyDown for single frame activation) ---
        IsLightAttackPressed = Input.GetKeyDown(lightAttackKey);
        IsHeavyAttackPressed = Input.GetKeyDown(heavyAttackKey);
        IsSpecialAttackPressed = Input.GetKeyDown(specialAttackKey);

        // 3. --- Blocking Input (Only if the character can block) ---
        if (canBlock)
        {
            IsBlocking = Input.GetKey(blockKey);
        }
        else
        {
            IsBlocking = false;
        }
    }
}