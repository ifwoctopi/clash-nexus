using UnityEngine;
using TMPro; // Only needed if using TextMeshPro

public class FadeAndFloat : MonoBehaviour
{
    [Header("Movement")]
    public float floatSpeed = 1.5f;      // upward movement speed
    public float lifetime = 1.0f;        // how long before fade completes
    public float scaleUp = 1.2f;         // optional scale punch

    private float timer = 0f;
    private TextMesh tm;
    private TextMeshPro tmp;
    private Color startColor;

    void Awake()
    {
        tm = GetComponent<TextMesh>();
        tmp = GetComponent<TextMeshPro>();

        if (tm != null)
            startColor = tm.color;
        else if (tmp != null)
            startColor = tmp.color;

        // Small starting scale punch
        transform.localScale *= scaleUp;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 1️⃣ Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 2️⃣ Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);

        if (tm != null)
        {
            tm.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
        else if (tmp != null)
        {
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        // 3️⃣ Shrink slightly over time
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, timer / lifetime);

        // 4️⃣ Destroy when done
        if (timer >= lifetime)
            Destroy(gameObject);
    }
}

