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
    private PlayerIdentity id;


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
        id = GetComponent<PlayerIdentity>();

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
        Debug.Log($"{gameObject.name} TOOK DAMAGE CALL: {damage}");

        if (isDead) return;

        int dmg = Mathf.RoundToInt(damage);

        Debug.Log($"[Damage] {gameObject.name} is taking {dmg} raw damage.");

        // Show popup
        Debug.Log($"[Popup] Showing popup for {dmg} damage at {transform.position + Vector3.up * 1.5f}");
        DamagePopupManager.Instance?.ShowDamage(
            dmg,
            transform.position + Vector3.up * 1.5f,
            id.playerNumber
        );

        // Register stats
        if (id.playerNumber == 1)
        {
            Debug.Log($"[Stats] Player1: Took {dmg}, Player2: Dealt {dmg}");
            PlayerStatsManager.Instance.GetStats(1).RegisterDamageTaken(dmg);
            PlayerStatsManager.Instance.GetStats(2).RegisterDamageDealt(dmg);
        }
        else
        {
            Debug.Log($"[Stats] Player2: Took {dmg}, Player1: Dealt {dmg}");
            PlayerStatsManager.Instance.GetStats(2).RegisterDamageTaken(dmg);
            PlayerStatsManager.Instance.GetStats(1).RegisterDamageDealt(dmg);
        }
        var s1 = PlayerStatsManager.Instance.GetStats(1);
        var s2 = PlayerStatsManager.Instance.GetStats(2);

        Debug.Log($"[STATS SUMMARY] P1: Dealt={s1.totalDamageDealt}, Taken={s1.totalDamageTaken}, " +
                  $"HitsLanded={s1.hitsLanded}, HitsReceived={s1.hitsReceived}, Attempts={s1.attackAttempts}");

        Debug.Log($"[STATS SUMMARY] P2: Dealt={s2.totalDamageDealt}, Taken={s2.totalDamageTaken}, " +
                  $"HitsLanded={s2.hitsLanded}, HitsReceived={s2.hitsReceived}, Attempts={s2.attackAttempts}");

      
        // Softer damage
        float scaledDamage = damage * 0.65f;
        Debug.Log($"[Damage Scaling] Raw Damage: {damage}, Scaled Damage: {scaledDamage}");

        currentHealth = Mathf.Clamp(currentHealth - scaledDamage, 0, maxHealth);

        Debug.Log($"[Health] {gameObject.name} current health after damage: {currentHealth}/{maxHealth}");

        animator.SetTrigger("Hurt");

        // Death check
        if (currentHealth <= 0 && !isPracticeDummy)
        {
            Debug.Log($"[Death] {gameObject.name} has died.");
            Die();
        }
        else if (currentHealth <= 0 && isPracticeDummy)
        {
            Debug.Log($"[Death Prevented] Practice dummy reached 0 health but does NOT die.");
            currentHealth = 0;
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
        Debug.Log($"{gameObject.name} NEW HEALTH = {currentHealth}");

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
