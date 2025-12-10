using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight1Controller : MonoBehaviour, IKnockbackable
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
    public float attackCooldownTime = 0.35f;
    private float nextAttackTime = 0f;

    [Header("Sequential Combo")]
    public float comboTimeWindow = 1.0f;
    public float generalChainBonus = 1.15f;
    private List<string> inputSequence = new List<string>();
    private readonly Dictionary<string, float> comboDefinitions = new Dictionary<string, float>()
    {
        {"L L H", 1.35f},
        {"L S", 1.50f},
        {"H L", 1.20f}
    };
    private List<float> attackTimestamps = new List<float>();

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("Layers")]
    public int player1;
    public int cpu;

    private CapsuleCollider2D playerCollider;
    private Rigidbody2D rb;
    private Player1Controls controls;
    private Vector2 moveInput;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sr;

    // --- Knockback fields ---
    private bool isKnockedback = false;
    private Vector2 knockbackVelocity;
    private float knockbackTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerCollider = GetComponent<CapsuleCollider2D>();

        currentHealth = maxHealth;

        controls = new Player1Controls();

        controls.Player1.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player1.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player1.Jump.performed += ctx => Jump();

        controls.Player1.Attack1.performed += ctx => Attack1();
        controls.Player1.Attack2.performed += ctx => Attack2();
        controls.Player1.Attack3.performed += ctx => Attack3();

        controls.Player1.Defend.performed += ctx => isDefending = true;
        controls.Player1.Defend.canceled += ctx => isDefending = false;

        Physics2D.IgnoreLayerCollision(player1, cpu, true);
    }

    private void OnEnable() => controls.Player1.Enable();
    private void OnDisable() => controls.Player1.Disable();

    private void Update()
    {
        if (isDead) return;

        CheckGround();
        animator.SetBool("isDefending", isDefending);

        if (wasGrounded && !isGrounded) animator.SetTrigger("Jump");
        if (!wasGrounded && isGrounded) animator.SetTrigger("Land");

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isFalling", rb.velocity.y < -0.1f);
        wasGrounded = isGrounded;

        animator.SetBool("isRunning", moveInput.x != 0);
        sr.flipX = moveInput.x < 0;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // --- Handle knockback ---
        if (isKnockedback)
        {
            rb.velocity = knockbackVelocity;
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f) isKnockedback = false;
            return;
        }

        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        if (isGrounded) rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void Attack1()
    {
        if (Time.time < nextAttackTime) return;
        animator.SetTrigger("Attack1");
        Attack();
        nextAttackTime = Time.time + attackCooldownTime;
        inputSequence.Add("L");
        LogAttackTime();
    }

    private void Attack2()
    {
        if (Time.time < nextAttackTime) return;
        animator.SetTrigger("Attack2");
        Attack();
        inputSequence.Add("L");
        LogAttackTime();
    }

    private void Attack3()
    {
        if (Time.time < nextAttackTime) return;
        animator.SetTrigger("Attack3");
        Attack();
        inputSequence.Add("L");
        LogAttackTime();
    }

    public void Attack()
    {
        float multiplier = GetComboMultiplier();
        int modifiedDamage = Mathf.RoundToInt(attackDamage * multiplier);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hits)
        {
            if (enemy.gameObject == gameObject || enemy.transform.IsChildOf(transform)) continue;

            PlayerHealth health = enemy.GetComponent<PlayerHealth>();
            if (health != null)
            {
                Vector2 knockDir = (transform.position - enemy.transform.position).normalized;
                health.TakeDamage(attackDamage, knockDir);
                Debug.Log($"{gameObject.name} hit {enemy.name} for {attackDamage} damage. Current Health: {health.currentHealth}");
            }

            Debug.Log($"Damage applied: {modifiedDamage} (Base: {attackDamage} * Multiplier: {multiplier:P0})");
        }
    }

    private void LogAttackTime()
    {
        attackTimestamps.Add(Time.time);
        while (attackTimestamps.Count > 0 && attackTimestamps[0] < Time.time - comboTimeWindow)
            attackTimestamps.RemoveAt(0);
    }

    private float GetComboMultiplier()
    {
        string currentSequence = string.Join(" ", inputSequence);
        foreach (var combo in comboDefinitions)
        {
            if (currentSequence.EndsWith(combo.Key))
            {
                inputSequence.Clear();
                return combo.Value;
            }
        }
        if (inputSequence.Count >= 2)
        {
            inputSequence.Clear();
            return generalChainBonus;
        }
        return 1f;
    }

    // --- Knockback interface method ---
    public void StartKnockback(Vector2 velocity, float duration)
    {
        isKnockedback = true;
        knockbackVelocity = velocity;
        knockbackTimer = duration;
    }

    private void Die()
    {
        isDead = true;
        moveInput = Vector2.zero;
        rb.velocity = Vector2.zero;
        playerCollider.enabled = false;
        rb.simulated = false;
        animator.SetTrigger("Dead");
        controls.Disable();
        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
