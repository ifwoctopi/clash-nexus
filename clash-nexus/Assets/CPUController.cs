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
    public int player1;
    public int cpu;

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
    public LayerMask playerLayer;
    public int attackDamage = 20;

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("AI Settings")]
    public Transform player;
    public float decisionRate = 0.5f;
    public float attackDistance = 1.5f;
    public float retreatChance = 0.25f;
    public Vector2 retreatDurationRange = new Vector2(0.5f, 1.2f);
    public float jumpDistanceThreshold = 3f;
    public float minDistanceToPlayer = 0.6f;

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

        Physics2D.IgnoreLayerCollision(player1, cpu, true);
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead) return;

        CheckGround();

        if (isDefending && Time.time >= blockEndTime)
        {
            isDefending = false;
            animator.SetBool("isDefending", false);
        }

        AIUpdate();

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isFalling", rb.velocity.y < -0.1f);
        animator.SetBool("isRunning", Mathf.Abs(rb.velocity.x) > 0.1f);
        animator.SetBool("isDefending", isDefending);

        if (rb.velocity.x > 0) sr.flipX = false;
        else if (rb.velocity.x < 0) sr.flipX = true;

        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
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

    private void AIUpdate()
    {
        if (player == null) return;

        float distance = player.position.x - transform.position.x;
        float absDistance = Mathf.Abs(distance);
        float dir = Mathf.Sign(distance);

        if (Time.time > nextDecisionTime)
        {
            nextDecisionTime = Time.time + decisionRate;

            if (isDefending)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                return;
            }

            if (!isDefending && !isAttacking && absDistance < attackDistance)
            {
                if (Random.value < blockChance)
                {
                    StartBlock();
                    return;
                }
            }

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

            if (absDistance < jumpDistanceThreshold && absDistance > minDistanceToPlayer &&
                isGrounded && !isAttacking && !isRetreating)
            {
                if (Random.value < 0.03f)
                    Jump();
            }
        }

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
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (id == 1) animator.SetTrigger("Attack1");
        else if (id == 2) animator.SetTrigger("Attack2");
        else animator.SetTrigger("Attack3");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);

                if (MatchStats.Instance != null)
                {
                    MatchStats.Instance.RegisterHitTaken(attackDamage);
                }


                Debug.Log($"CPU hit {hit.name} for {attackDamage} damage. Current Health: {health.currentHealth}");

                if (DamagePopupManager.Instance != null)
                {
                    DamagePopupManager.Instance.ShowDamagePopup(
                        hit.transform.position + Vector3.up * 1.5f,  // float above player
                        attackDamage,
                        Color.red                                     // player takes red damage
                    );
                }
            }
        }

        StartCoroutine(EndAttackCoroutine());
    }

    private IEnumerator EndAttackCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Dead");

        Destroy(gameObject, 2f);
        FindObjectOfType<MetricsDashboardUI>(true).gameObject.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}

