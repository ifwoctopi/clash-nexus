using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages health bars displayed at the top of the screen for both players
/// </summary>
public class UIHealthBarManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Health bar slider for Player 1 (left side at top of screen)")]
    [SerializeField] private Slider player1HealthBar;
    
    [Tooltip("Health bar slider for Player 2 (right side at top of screen)")]
    [SerializeField] private Slider player2HealthBar;
    
    [Tooltip("Text label for Player 1 health (optional)")]
    [SerializeField] private Text player1HealthText;
    
    [Tooltip("Text label for Player 2 health (optional)")]
    [SerializeField] private Text player2HealthText;

    private PlayerSpawner playerSpawner;
    private PlayerHealth player1Health;
    private PlayerHealth player2Health;

    void Start()
    {
        // Find PlayerSpawner
        playerSpawner = FindObjectOfType<PlayerSpawner>();
        if (playerSpawner == null)
        {
            Debug.LogWarning("UIHealthBarManager: PlayerSpawner not found. Health bars will update when players spawn.");
        }
        
        // Validate that health bars are assigned
        if (player1HealthBar == null)
        {
            Debug.LogError("UIHealthBarManager: Player 1 health bar not assigned! Please assign the left health bar slider in the Inspector.");
        }
        
        if (player2HealthBar == null)
        {
            Debug.LogError("UIHealthBarManager: Player 2 health bar not assigned! Please assign the right health bar slider in the Inspector.");
        }
        
        // Initialize health bars to full
        if (player1HealthBar != null)
        {
            player1HealthBar.value = 1f;
        }
        if (player2HealthBar != null)
        {
            player2HealthBar.value = 1f;
        }
    }

    void Update()
    {
        // Update health bars if players exist
        if (player1Health == null || player2Health == null)
        {
            FindPlayers();
        }
        
        if (player1Health != null && player1HealthBar != null)
        {
            float healthPercent = player1Health.currentHealth / player1Health.maxHealth;
            player1HealthBar.value = healthPercent;
            UpdateHealthBarColor(player1HealthBar, healthPercent);
            
            if (player1HealthText != null)
            {
                player1HealthText.text = $"P1: {Mathf.CeilToInt(player1Health.currentHealth)}/{Mathf.CeilToInt(player1Health.maxHealth)}";
            }
        }
        
        if (player2Health != null && player2HealthBar != null)
        {
            float healthPercent = player2Health.currentHealth / player2Health.maxHealth;
            player2HealthBar.value = healthPercent;
            UpdateHealthBarColor(player2HealthBar, healthPercent);
            
            if (player2HealthText != null)
            {
                player2HealthText.text = $"P2: {Mathf.CeilToInt(player2Health.currentHealth)}/{Mathf.CeilToInt(player2Health.maxHealth)}";
            }
        }
    }

    void FindPlayers()
    {
        if (playerSpawner == null)
        {
            playerSpawner = FindObjectOfType<PlayerSpawner>();
        }
        
        if (playerSpawner != null)
        {
            GameObject p1 = playerSpawner.GetSpawnedPlayer(1);
            GameObject p2 = playerSpawner.GetSpawnedPlayer(2);
            
            if (p1 != null)
            {
                player1Health = p1.GetComponent<PlayerHealth>();
            }
            
            if (p2 != null)
            {
                player2Health = p2.GetComponent<PlayerHealth>();
            }
        }
    }


    void UpdateHealthBarColor(Slider slider, float healthPercent)
    {
        if (slider == null || slider.fillRect == null) return;
        
        Image fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage == null) return;

        // Fill is always green (current health)
        // Background is red (missing health - set in CreateHealthBar)
        fillImage.color = Color.green;
    }
}

