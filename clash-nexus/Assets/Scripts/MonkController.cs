using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkController : MonoBehaviour
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

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;
    
    [Header("Layers")]
    public int player1; // e.g., Layer number for Player
    public int cpu;    // e.g., Layer number for CPU

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
        // Detect enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        // Damage each enemy hit
        foreach (Collider2D enemy in hits)
        {
           //enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return; // ignore damage if already dead

        currentHealth -= damage;
        Debug.Log($"Knight 1 took {damage} damage! Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
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
