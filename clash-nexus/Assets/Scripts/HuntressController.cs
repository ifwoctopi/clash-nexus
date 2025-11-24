using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntressController : MonoBehaviour
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
    
    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    private bool isDashing = false;
    private float dashEndTime;
    private float nextDashTime;
    
    [Header("Layers")]
    public int player1; // e.g., Layer number for Player
    public int cpu;    // e.g., Layer number for CPU
    
    [Header("Projectiles")]
    public GameObject projectilePrefab; // assign your projectile prefab
    public Transform firePoint;         // position from which projectiles spawn
    public float projectileSpeed = 10f; // optional override

    [Header("Combat")]
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
        Physics2D.IgnoreLayerCollision(player1, cpu, true); // prevents physics push
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerCollider = GetComponent<CapsuleCollider2D>();

        // Ensure maxHealth is at least 100 (safeguard against Inspector misconfiguration)
        if (maxHealth < 100)
        {
            maxHealth = 100;
            Debug.LogWarning($"HuntressController: maxHealth was less than 100, setting to 100");
        }

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
        controls.Player1.Defend.performed += ctx => TryDash();
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
            Debug.Log($"HuntressController: Added PlayerHealth component to {gameObject.name}");
        }
        
        // Always sync maxHealth with PlayerHealth component (override any default values)
        playerHealth.maxHealth = maxHealth;
        // Force set currentHealth to maxHealth to ensure it's correct
        playerHealth.currentHealth = maxHealth;
        
        Debug.Log($"HuntressController: Initialized health for {gameObject.name} - maxHealth: {maxHealth}, currentHealth: {playerHealth.currentHealth}");
        
        // Double-check after another frame to ensure it wasn't reset
        yield return null;
        if (playerHealth.currentHealth != maxHealth)
        {
            Debug.LogWarning($"HuntressController: Health was reset! Fixing from {playerHealth.currentHealth} to {maxHealth}");
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

        // DASH OVERRIDES NORMAL MOVEMENT
        if (isDashing)
        {
            float dir = sr.flipX ? -1f : 1f; // dash in facing direction
            rb.velocity = new Vector2(dir * dashSpeed, rb.velocity.y);

            // End dash when duration is over
            if (Time.time >= dashEndTime)
                isDashing = false;

            return; // skip normal movement while dashing
        }

        // Normal movement
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
    }


    private void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
    
    private void TryDash()
    {
        // must be running to dash
        if (!animator.GetBool("isRunning")) return;

        // prevent dash spamming
        if (Time.time < nextDashTime) return;

        // trigger dash
        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;

        // play dash animation if you have one
        animator.SetTrigger("Dash");
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

        Debug.Log($"Huntress Attack: Found {hits.Length} colliders in range");

        // Damage each enemy hit
        foreach (Collider2D enemy in hits)
        {
            PlayerHealth enemyHealth = enemy.GetComponent<PlayerHealth>();
            if (enemyHealth != null)
            {
                Debug.Log($"Huntress Attack: Dealing {damage} damage to {enemy.name}");
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning($"Huntress Attack: Hit {enemy.name} but no PlayerHealth component found");
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
                                Debug.Log($"Huntress Attack: Dealing {damage} damage to other player {otherPlayer.name} (distance: {distance})");
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
                                Debug.Log($"Huntress Attack: Dealing {damage} damage to other player {otherPlayer.name} (distance: {distance}, no collider)");
                                otherPlayerHealth.TakeDamage(damage);
                            }
                        }
                    }
                }
            }
        }
    }
    
    public void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        ProjectileScript projectileScript = proj.GetComponent<ProjectileScript>();

        // Set the direction based on the player's facing
        projectileScript.direction = sr.flipX ? Vector2.left : Vector2.right;
        projectileScript.speed = projectileSpeed;
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

        //disable Rigidbody gravity if desired
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
