using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputController : MonoBehaviour
{
    private PlayerInput inputHandler;
    public float moveSpeed = 5f;

    void Start()
    {
        inputHandler = GetComponent<PlayerInput>();
    }

    void Update()
    {
        // Movement
        transform.Translate(Vector3.right * inputHandler.HorizontalInput * moveSpeed * Time.deltaTime);

        // Attacks
        if (inputHandler.IsLightAttackPressed)
        {
            // Trigger light attack animation/logic
        }
        else if (inputHandler.IsHeavyAttackPressed)
        {
            // Trigger heavy attack animation/logic
        }
        else if (inputHandler.IsSpecialAttackPressed)
        {
            // Trigger special attack animation/logic
        }

        // Blocking & Dashing
        else if (inputHandler.IsBlocking)
        {
            // Enter blocking state
        }
        else if (inputHandler.IsDashing)
        {
            // begin dashing animations
        }
    }
}
