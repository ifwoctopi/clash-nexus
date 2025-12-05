using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Animator animator; // assign CPU or Player animator in Inspector
    
    [Header("Health")]
    public float maxHealth = 100f; // Default 100 health
    public float currentHealth; // Public so MatchTimer can access it

    [Header("Audio")]
    [Tooltip("Sound to play when taking damage")]
    public AudioClip punchSound;
    
    [Range(0f, 1f)]
    [Tooltip("Volume of the punch sound")]
    public float punchSoundVolume = 0.5f;
    
    private bool isDead = false;
    private AudioSource audioSource;
    
    // Practice mode health regeneration
    private bool isPracticeDummy = false;
    private float lastDamageTime = 0f;
    private float healthRegenDelay = 5f; // Regenerate after 5 seconds of no damage
    private float healthRegenRate = 50f; // Health per second when regenerating (faster for practice mode)

    private void Start()
    {
        currentHealth = maxHealth;
        // Health bar will be updated by SimpleHealthBar component
        
        // Get or create AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        // Ensure AudioSource volume is at 1.0 so PlayOneShot volume parameter works correctly
        audioSource.volume = 1f;
        
        // Check if this is Player 2 in practice mode
        CheckIfPracticeDummy();
    }
    
    private void Update()
    {
        // Handle health regeneration for practice dummy
        if (isPracticeDummy && currentHealth < maxHealth)
        {
            if (Time.time - lastDamageTime >= healthRegenDelay)
            {
                // Regenerate health
                currentHealth = Mathf.Min(currentHealth + healthRegenRate * Time.deltaTime, maxHealth);
            }
        }
    }
    
    private void CheckIfPracticeDummy()
    {
        GameDataManager dataManager = GameDataManager.Instance;
        if (dataManager != null && dataManager.IsPracticeMode())
        {
            // Check if this is Player 2
            string name = gameObject.name;
            if (name.StartsWith("Player2") || name.Contains("Player2_"))
            {
                isPracticeDummy = true;
                Debug.Log($"PlayerHealth: {gameObject.name} is a practice dummy - will regenerate health");
            }
        }
    }

    /// <summary>
    /// Takes damage (accepts both float and int for compatibility)
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        animator.SetTrigger("Hurt");
        
        // Play punch sound when taking damage
        if (punchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(punchSound, punchSoundVolume);
        }
        
        // Update last damage time for practice dummy regeneration
        if (isPracticeDummy)
        {
            lastDamageTime = Time.time;
        }
        
        // Health bar will be updated automatically by SimpleHealthBar component

        // Practice dummy never dies
        if (currentHealth <= 0 && !isPracticeDummy)
        {
            Die();
        }
        else if (currentHealth <= 0 && isPracticeDummy)
        {
            // For practice dummy, just keep health at 0 visually but don't die
            currentHealth = 0;
            Debug.Log($"{gameObject.name} (practice dummy) health at 0 but not dying");
        }
    }

    /// <summary>
    /// Takes damage (int overload for compatibility with existing code)
    /// </summary>
    public void TakeDamage(int damage)
    {
        TakeDamage((float)damage);
    }

    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);
        // Health bar will be updated automatically by SimpleHealthBar component
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has died.");
        
        // Determine player number from name (PlayerSpawner names them "Player1_..." or "Player2_...")
        int playerNumber = 1;
        string name = gameObject.name;
        if (name.StartsWith("Player2") || name.Contains("Player2_"))
        {
            playerNumber = 2;
        }
        else if (name.StartsWith("Player1") || name.Contains("Player1_"))
        {
            playerNumber = 1;
        }
        else
        {
            // Fallback: check if it's the second spawned player by checking PlayerSpawner
            PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
            if (spawner != null)
            {
                GameObject p2 = spawner.GetSpawnedPlayer(2);
                if (p2 == gameObject)
                {
                    playerNumber = 2;
                }
            }
        }
        
        // Notify MatchTimer that a player died
        MatchTimer matchTimer = FindObjectOfType<MatchTimer>();
        if (matchTimer != null)
        {
            matchTimer.OnPlayerDied(); //playerNumber
        }
        
        // Try to call Die() on any controller component
        MonoBehaviour[] controllers = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour controller in controllers)
        {
            if (controller != this && controller.GetType().Name.Contains("Controller"))
            {
                System.Reflection.MethodInfo dieMethod = controller.GetType().GetMethod("Die", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (dieMethod != null)
                {
                    dieMethod.Invoke(controller, null);
                }
            }
        }
    }

    /// <summary>
    /// Checks if the player is dead
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}
