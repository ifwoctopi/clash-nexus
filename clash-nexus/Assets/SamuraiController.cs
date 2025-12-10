using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamuraiController : MonoBehaviour
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

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    private CapsuleCollider2D playerCollider;

    private Rigidbody2D rb;
    private Player1Controls controls;
    private Vector2 moveInput;

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sr;

    private PlayerIdentity id;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerCollider = GetComponent<CapsuleCollider2D>();

        currentHealth = maxHealth;

        id = GetComponent<PlayerIdentity>();
        if (id == null)
        {
            id = gameObject.AddComponent<PlayerIdentity>();
            id.playerNumber = 1;
        }

        controls = new Player1Controls();

        controls.Player1.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player1.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player1.Jump.performed += ctx => Jump();

        controls.Player1.Attack1.performed += ctx => Attack1();
        controls.Player1.Attack2.performed += ctx => Attack2();
        controls.Player1.Attack3.performed += ctx => Attack3();

        controls.Player1.Defend.performed += ctx => isDefending = true;
        controls.Player1.Defend.canceled += ctx => isDefending = false;
    }

    private void OnEnable() => controls.Player1.Enable();
    private void OnDisable() => controls.Player1.Disable();

    private void Update()
    {
        if (isDead) return;

        CheckGround();

        animator.SetBool("isDefending", isDefending);

        if (wasGrounded && !isGrounded)
            animator.SetTrigger("Jump");

        if (!wasGrounded && isGrounded)
            animator.SetTrigger("Land");

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isFalling", rb.velocity.y < -0.1f);
        animator.SetBool("isRunning", moveInput.x != 0);

        if (moveInput.x > 0) sr.flipX = false;
        else if (moveInput.x < 0) sr.flipX = true;

        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        if (isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    // ------------------- COMBAT -------------------

    private void Attack1()
    {
        if (Time.time < nextAttackTime) return;
        PlayerStatsManager.Instance.GetStats(id.playerNumber).attackAttempts++;

        animator.SetTrigger("Attack1");
        Attack();

        nextAttackTime = Time.time + attackCooldownTime;
    }

    private void Attack2()
    {
        if (Time.time < nextAttackTime) return;
        PlayerStatsManager.Instance.GetStats(id.playerNumber).attackAttempts++;

        animator.SetTrigger("Attack2");
        Attack();

        nextAttackTime = Time.time + attackCooldownTime;
    }

    private void Attack3()
    {
        if (Time.time < nextAttackTime) return;
        PlayerStatsManager.Instance.GetStats(id.playerNumber).attackAttempts++;

        animator.SetTrigger("Attack3");
        Attack();

        nextAttackTime = Time.time + attackCooldownTime;
    }

    private void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider2D enemy in hits)
        {
            if (enemy.gameObject == gameObject ||
                enemy.transform.IsChildOf(transform))
                continue;

            PlayerHealth health = enemy.GetComponent<PlayerHealth>();

            if (health != null)
            {
                PlayerStatsManager.Instance.GetStats(id.playerNumber).RegisterDamageDealt(attackDamage);
                Vector2 knockDir = (transform.position - enemy.transform.position).normalized;
                health.TakeDamage(attackDamage, knockDir);

                Debug.Log($"Samurai hit {enemy.name} for {attackDamage} damage.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    private void OnDestroy()
    {
        if (controls != null)
            controls.Dispose();
    }

}

