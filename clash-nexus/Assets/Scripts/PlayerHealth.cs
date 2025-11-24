using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f; // Default 100 health
    public float currentHealth; // Public so MatchTimer can access it

    
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        // Health bar will be updated by SimpleHealthBar component
    }

    /// <summary>
    /// Takes damage (accepts both float and int for compatibility)
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        
        // Health bar will be updated automatically by SimpleHealthBar component

        if (currentHealth <= 0)
        {
            Die();
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
            matchTimer.OnPlayerDied(playerNumber);
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
