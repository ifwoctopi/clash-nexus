using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    public int damage = 10;
    public Vector2 direction;
    public GameObject target;

    void Start()
    {
        // Flip sprite based on movement direction
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (direction.x < 0)
                sr.flipX = true;
            else
                sr.flipX = false;
        }

        Destroy(gameObject, lifetime);
    }
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == target)
        {
            Debug.Log("Projectile Hit");
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                Vector2 knockDir = (transform.position - collision.transform.position).normalized;
                health.TakeDamage(damage, knockDir);
                Debug.Log($"CPU hit {collision.name} with PROJECTILE for {damage} damage. Current Health: {health.currentHealth}");
            }
            Destroy(gameObject);
        }
    }
}
