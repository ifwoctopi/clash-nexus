using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight1Controller : MonoBehaviour
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

    public int attackDamage = 20;

     [Header("Sequential Combo")]
    public float comboTimeWindow = 1.0f;     // Max time (in seconds) between attacks
    public float generalChainBonus = 1.15f;  // 15% base bonus for any quick chain (NEW FIELD)
    // A list to track the sequence of attacks entered
    private List<string> inputSequence = new List<string>();
    
    // Define your sequential combos and their *higher* bonus damage multipliers
    private readonly Dictionary<string, float> comboDefinitions = new Dictionary<string, float>()
    {
        // Define combos as space-separated strings: L=Light, H=Heavy, S=Special
        {"L L H", 1.35f}, // Light -> Light -> Heavy (Highest bonus)
        {"L S", 1.50f},   // Light -> Special
        {"H L", 1.20f}    // Heavy -> Light
    };

    [Header("Health")]
    public int maxHealth = 3;
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


    private void OnEnable()
    {
        controls.Player1.Enable();
    }

    private void OnDisable()
    {
        controls.Player1.Disable();
    }

    private void Update()
    {
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
        Attack();
    }

    private void Attack2()
    {
        Debug.Log("Attack2");
        animator.SetTrigger("Attack2");
        Attack();
    }

    private void Attack3()
    {
        Debug.Log("Attack3");
        animator.SetTrigger("Attack3");
        Attack();
    }
    
    public void Attack()
    {
        
        // 1. Check if a combo was active and get the damage multiplier.
        // 2. Calculate the damage
        float multiplier = GetComboMultiplier();
        int modifiedDamage = Mathf.RoundToInt(attackDamage * multiplier);
        
        // Detect enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        // Damage each enemy hit
        foreach (Collider2D enemy in hits)
        {
            // Don't damage ourselves
            if (enemy.gameObject == gameObject || enemy.transform.IsChildOf(transform))
            {
                continue;
            }
            
            PlayerHealth health = enemy.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} hit {enemy.name} for {attackDamage} damage. Current Health: {health.currentHealth}");
            }
           //enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
            Debug.Log($"Damage applied: {modifiedDamage} (Base: {attackDamage} * Multiplier: {multiplier:P0})");
        }
    }

    // --- COMBO CALCULATION FUNCTION (Called inside Attack()) ---
    private float GetComboMultiplier()
    {
        float multiplier = 1.0f;
        
        // --- 1. CHECK FOR SPECIAL SEQUENTIAL COMBO (Priority 1) ---
        string currentSequence = string.Join(" ", inputSequence);

        foreach (var combo in comboDefinitions)
        {
            string comboKey = combo.Key;
            
            // Check if the current input sequence ENDS with a defined combo pattern
            if (currentSequence.EndsWith(comboKey))
            {
                multiplier = combo.Value;
                Debug.Log($"✅ SEQUENTIAL COMBO SUCCESS: {comboKey}! Multiplier: {multiplier:P0}");
                
                // Clear sequence and return the highest multiplier
                inputSequence.Clear(); 
                return multiplier;
            }
        }
        
        // --- 2. CHECK FOR GENERAL CHAIN COMBO (Priority 2) ---
        // If no specific combo was found, check if a general quick chain occurred.
        // We look for a chain of at least 2 inputs to qualify as a "chain".
        if (inputSequence.Count >= 2) 
        {
            multiplier = generalChainBonus;
            Debug.Log($"⚠️ GENERAL CHAIN BONUS: {inputSequence.Count} quick hits. Multiplier: {multiplier:P0}");
            
            // Clear the sequence for the next chain, and return the base bonus.
            inputSequence.Clear();
            return multiplier;
        }
        
        // --- 3. NO COMBO ---
        // If neither condition is met, return the base 1.0 multiplier.
        return 1.0f;
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
        Destroy(gameObject, 2f);
    }

    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}




