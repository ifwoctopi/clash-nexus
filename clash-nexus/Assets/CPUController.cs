using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CPUController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("Blocking")]
    public float blockChance = 0.25f;       // % chance to block when threatened
    public float blockDuration = 0.5f;      // how long CPU blocks
    private float blockEndTime;

    [Header("Layers")]
    public int player1; // e.g., Layer number for Player
    public int cpu;    // e.g., Layer number for CPU
    
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
    public LayerMask playerLayer; // Target layer (player)
    public int attackDamage = 20;

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("AI Settings")]
    public Transform player; // Target player
    public float decisionRate = 0.5f;
    public float attackDistance = 1.5f;
    public float retreatChance = 0.25f;
    public Vector2 retreatDurationRange = new Vector2(0.5f, 1.2f);
    public float jumpDistanceThreshold = 3f;
    public float minDistanceToPlayer = 0.6f;

    // Internal state
    private Rigidbody2D rb;
    private CapsuleCollider2D collider2D;
    private float nextDecisionTime;
    private bool isAttacking = false;
    private bool isJumping = false;
    private bool isRetreating = false;
    private float retreatEndTime;

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<CapsuleCollider2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        Physics2D.IgnoreLayerCollision(player1, cpu, true); // prevents physics push


        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead) return;

        CheckGround();
        
        // If blocking, check expiration
        if (isDefending && Time.time >= blockEndTime)
        {
            isDefending = false;
            animator.SetBool("isDefending", false);
        }
        
        AIUpdate();

        // Animations
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isFalling", rb.velocity.y < -0.1f);
        animator.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0.1f);
        animator.SetBool("isDefending", isDefending);

        // Flip sprite based on movement
        if (rb.velocity.x > 0) sr.flipX = false;
        else if (rb.velocity.x < 0) sr.flipX = true;

        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // Stop horizontal movement during attack
        if (isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y); // Horizontal movement applied in AI
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (!wasGrounded && isGrounded && isJumping)
        {
            animator.SetTrigger("Land");
            isJumping = false;
        }

        if (wasGrounded && !isGrounded && isJumping)
        {
            animator.SetTrigger("Jump");
        }
    }

    // ------------------- AI Logic -------------------
    private void AIUpdate()
    {
        if (player == null) return;

        float distance = player.position.x - transform.position.x;
        float absDistance = Mathf.Abs(distance);
        float dir = Mathf.Sign(distance);
        
        

        // Periodic decisions
        if (Time.time > nextDecisionTime)
        {
            nextDecisionTime = Time.time + decisionRate;
            
            // If blocking, CPU stops moving and does nothing
            if (isDefending)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                return;
            }
            
            // CPU BLOCK DECISION
            // If the player is close, block randomly
            if (!isDefending && !isAttacking && absDistance < attackDistance)
            {
                if (Random.value < blockChance)
                {
                    StartBlock();
                    return;
                }
            }
            // Attack / Retreat
            if (absDistance < attackDistance)
            {
                if (!isRetreating && Random.value < retreatChance)
                {
                    isRetreating = true;
                    retreatEndTime = Time.time + Random.Range(retreatDurationRange.x, retreatDurationRange.y);
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    return;
                }

                TriggerAttack(Random.Range(1, 4));
                return;
            }

            // Jump logic
            if (absDistance < jumpDistanceThreshold && absDistance > minDistanceToPlayer && isGrounded && !isAttacking && !isRetreating)
            {
                if (Random.value < 0.03f)
                    Jump();
            }
        }

        // Movement
        if (isRetreating)
        {
            rb.velocity = new Vector2(-dir * moveSpeed, rb.velocity.y);

            if (Time.time > retreatEndTime)
                isRetreating = false;
        }
        else
        {
            rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        }
    }

    // ------------------- Actions -------------------
    private void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
        }
    }
    
    private void StartBlock()
    {
        if (isDefending || isAttacking || isRetreating) return;

        isDefending = true;
        blockEndTime = Time.time + blockDuration;

        animator.SetBool("isDefending", true);
    }

    private void TriggerAttack(int id)
    {
        if (isAttacking) return;

        isAttacking = true;
        rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal movement

        if (id == 1) animator.SetTrigger("Attack1");
        else if (id == 2) animator.SetTrigger("Attack2");
        else animator.SetTrigger("Attack3");

        // Damage detection
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
                Debug.Log($"CPU hit {hit.name} for {attackDamage} damage. Current Health: {health.currentHealth}");
            }
        }

        StartCoroutine(EndAttackCoroutine());
    }

    private IEnumerator EndAttackCoroutine()
    {
        // Wait for animation to finish (approximate)
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // ------------------- Health -------------------

    private void Die()
    {
        isDead = true;
        //rb.velocity = Vector2.zero;
        //collider2D.enabled = false;
        animator.SetTrigger("Dead");
        
        Destroy(gameObject, 2f);
        
    }

    // ------------------- Debug -------------------
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

