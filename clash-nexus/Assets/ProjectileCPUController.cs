using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCPUController: MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

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
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask playerLayer; // Target layer (player)
    public int attackDamage = 20;
    
    [Header("Projectiles")]
    public GameObject projectilePrefab; // assign the projectile prefab
    public Transform firePoint;         // position from which projectiles spawn
    public float projectileSpeed = 10f;
    
    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashEndTime;
    private float nextDashTime;


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
        AIUpdate();

        // Animations
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isFalling", rb.velocity.y < -0.1f);
        animator.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0.1f);

        // Flip sprite based on movement
        if (rb.velocity.x > 0) sr.flipX = false;
        else if (rb.velocity.x < 0) sr.flipX = true;

        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // DASH OVERRIDES MOVEMENT
        if (isDashing)
        {
            float dir = sr.flipX ? -1f : 1f;
            rb.velocity = new Vector2(dir * dashSpeed, rb.velocity.y);

            if (Time.time >= dashEndTime)
                isDashing = false;

            return;
        }

        // Stop horizontal movement during attack
        if (isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // Normal AI movement is already applied in AIUpdate()
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
    
    private void TryDash()
    {
        // Must be running to dash
        if (!animator.GetBool("isRunning")) return;

        // Cooldown
        if (Time.time < nextDashTime) return;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;

        animator.SetTrigger("Dash");
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
            
            // CPU dash decision (random small chance)
            if (animator.GetBool("isRunning") && !isAttacking && !isRetreating && isGrounded)
            {
                if (Random.value < 0.10f) // 10% chance every decision tick
                {
                    TryDash();
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

    private void TriggerAttack(int id)
    {
        if (isAttacking) return;

        isAttacking = true;
        rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal movement

        if (id == 1) animator.SetTrigger("Attack1");
        else if (id == 2) animator.SetTrigger("Attack2");
        else if (id == 3)
        {
            animator.SetTrigger("Attack3");
        }

        // Damage detection for melee
        if (id != 3) // optional: projectiles might handle damage separately
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
            foreach (Collider2D hit in hits)
            {
                // hit.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
            }
        }

        StartCoroutine(EndAttackCoroutine());
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        ProjectileScript projectileScript = proj.GetComponent<ProjectileScript>();

        if (projectileScript != null)
        {
            projectileScript.direction = sr.flipX ? Vector2.left : Vector2.right;
            projectileScript.speed = projectileSpeed;
        }
    }


    private IEnumerator EndAttackCoroutine()
    {
        // Wait for animation to finish (approximate)
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // ------------------- Health -------------------
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        collider2D.enabled = false;
        animator.SetTrigger("Dead");
    }

    // ------------------- Debug -------------------
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
