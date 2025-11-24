using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPriestController : MonoBehaviour
{
     [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Combat")]
    private bool isDefending;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    [Header("Attack Damage")]
    public int attack1Damage = 7;  // E key
    public int attack2Damage = 10; // R key
    public int attack3Damage = 13; // T key

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    private CapsuleCollider2D playerCollider; // reference to your main collider
    
    private Rigidbody2D rb;
    private Player1Controls controls;
    private Vector2 moveInput;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerCollider = GetComponent<CapsuleCollider2D>();

        // Ensure PlayerHealth component exists and is synced
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = gameObject.AddComponent<PlayerHealth>();
        }
        playerHealth.maxHealth = maxHealth;
        playerHealth.currentHealth = maxHealth;

        currentHealth = maxHealth;

        controls = new Player1Controls();

        // Movement
        controls.Player1.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player1.Move.canceled += ctx => moveInput = Vector2.zero;

        // Jump
        controls.Player1.Jump.performed += ctx => Jump();

        // Attacks
        controls.Player1.Attack1.performed += ctx => Attack1();
        controls.Player1.Attack2.performed += ctx => Attack2();
        controls.Player1.Attack3.performed += ctx => Attack3();

        // Defense
        controls.Player1.Defend.performed += ctx => isDefending = true;
        controls.Player1.Defend.canceled += ctx => isDefending = false;
    }

    private void Start()
    {
        // Use coroutine to ensure health is set after all Start() methods have run
        StartCoroutine(InitializeHealthDelayed());
    }

    private System.Collections.IEnumerator InitializeHealthDelayed()
    {
        // Wait one frame to ensure PlayerHealth.Start() has run
        yield return null;
        
        // Ensure PlayerHealth component exists and is synced
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = gameObject.AddComponent<PlayerHealth>();
            Debug.Log($"WaterPriestController: Added PlayerHealth component to {gameObject.name}");
        }
        
        // Always sync maxHealth with PlayerHealth component (override any default values)
        playerHealth.maxHealth = maxHealth;
        // Force set currentHealth to maxHealth to ensure it's correct
        playerHealth.currentHealth = maxHealth;
        
        Debug.Log($"WaterPriestController: Initialized health for {gameObject.name} - maxHealth: {maxHealth}, currentHealth: {playerHealth.currentHealth}");
        
        // Double-check after another frame to ensure it wasn't reset
        yield return null;
        if (playerHealth.currentHealth != maxHealth)
        {
            Debug.LogWarning($"WaterPriestController: Health was reset! Fixing from {playerHealth.currentHealth} to {maxHealth}");
            playerHealth.maxHealth = maxHealth;
            playerHealth.currentHealth = maxHealth;
        }
    }

    private void OnEnable()
    {
        controls.Player1.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player1.Disable();
        }
    }

    private void Update()
    {
        // Check if dead from PlayerHealth component
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead() && !isDead)
        {
            Die();
            return;
        }
        
        if (isDead) return;
        CheckGround();
        if(isDefending)
        {
            animator.SetBool("isDefending", true);
        }
        else
        {
            animator.SetBool("isDefending", false);
        }

        // Trigger Jump ONCE on takeoff
        if (wasGrounded && !isGrounded)
        {
            animator.SetTrigger("Jump");
        }

        // Landing trigger (optional)
        if (!wasGrounded && isGrounded)
        {
            animator.SetTrigger("Land");
        }

        // Run animation
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isFalling", rb.velocity.y < -.1);
        wasGrounded = isGrounded;

        // Run animation
        animator.SetBool("isRunning", moveInput.x != 0);

        // Flip sprite
        if (moveInput.x > 0)
            sr.flipX = false;
        else if (moveInput.x < 0)
            sr.flipX = true;
    }
    private void FixedUpdate()
    {
        if (isDead) return;
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void Attack1()
    {
        Debug.Log("Attack1");
        animator.SetTrigger("Attack1");
        Attack(attack1Damage);
    }

    private void Attack2()
    {
        Debug.Log("Attack2");
        animator.SetTrigger("Attack2");
        Attack(attack2Damage);
    }

    private void Attack3()
    {
        Debug.Log("Attack3");
        animator.SetTrigger("Attack3");
        Attack(attack3Damage);
    }
    
    public void Attack(int damage)
    {
        // Detect enemies in range (CPU and other players)
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        // Damage each enemy hit
        foreach (Collider2D enemy in hits)
        {
            PlayerHealth enemyHealth = enemy.GetComponent<PlayerHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // Also check for the other player in 2-player mode (they might be on the same layer)
        if (GameDataManager.Instance != null && GameDataManager.Instance.IsTwoPlayerMode())
        {
            PlayerSpawner spawner = FindObjectOfType<PlayerSpawner>();
            if (spawner != null)
            {
                // Determine which player we are
                bool isPlayer1 = gameObject.name.StartsWith("Player1");
                GameObject otherPlayer = isPlayer1 ? spawner.GetSpawnedPlayer(2) : spawner.GetSpawnedPlayer(1);
                
                if (otherPlayer != null && otherPlayer != gameObject)
                {
                    // Check if other player's collider is in attack range
                    Collider2D otherPlayerCollider = otherPlayer.GetComponent<Collider2D>();
                    if (otherPlayerCollider != null)
                    {
                        // Check distance from attack point to the other player's collider bounds
                        Vector2 closestPoint = otherPlayerCollider.bounds.ClosestPoint(attackPoint.position);
                        float distance = Vector2.Distance(attackPoint.position, closestPoint);
                        
                        if (distance <= attackRange)
                        {
                            PlayerHealth otherPlayerHealth = otherPlayer.GetComponent<PlayerHealth>();
                            if (otherPlayerHealth != null)
                            {
                                Debug.Log($"WaterPriest Attack: Dealing {damage} damage to other player {otherPlayer.name} (distance: {distance})");
                                otherPlayerHealth.TakeDamage(damage);
                            }
                        }
                    }
                    else
                    {
                        // Fallback: use transform position if no collider found
                        float distance = Vector2.Distance(attackPoint.position, otherPlayer.transform.position);
                        if (distance <= attackRange)
                        {
                            PlayerHealth otherPlayerHealth = otherPlayer.GetComponent<PlayerHealth>();
                            if (otherPlayerHealth != null)
                            {
                                Debug.Log($"WaterPriest Attack: Dealing {damage} damage to other player {otherPlayer.name} (distance: {distance}, no collider)");
                                otherPlayerHealth.TakeDamage(damage);
                            }
                        }
                    }
                }
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Forward damage to PlayerHealth component (this is the primary health system)
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            // Sync internal health with PlayerHealth for backwards compatibility
            currentHealth = Mathf.RoundToInt(playerHealth.currentHealth);
            
            // Only trigger hurt animation if not dead
            if (!playerHealth.IsDead())
            {
                animator.SetTrigger("Hurt");
            }
        }
        else
        {
            // Fallback: use internal health if PlayerHealth doesn't exist
            currentHealth -= damage;
            animator.SetTrigger("Hurt");
            if (currentHealth <= 0)
                Die();
        }
    }
    private void Die()
    {
        isDead = true;
        Debug.Log("Knight 1 Died!");

        // Stop movement
        moveInput = Vector2.zero;
        rb.velocity = Vector2.zero;

        // Disable collider so sprite doesn't float
        playerCollider.enabled = false;

        // Optional: disable Rigidbody gravity if desired
        rb.simulated = false;

        // Trigger death animation
        animator.SetTrigger("Dead");

        // Disable input
        controls.Disable();

        // Optional: destroy object after animation ends
        // Destroy(gameObject, 2f);
    }

    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
