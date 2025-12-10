using UnityEngine;
using TMPro;

public class DamagePopupEffect : MonoBehaviour
{
    public float lifetime = 0.8f;
    public float floatSpeed = 60f;
    public float scalePunch = 1.2f;

    private TMP_Text text;
    private float timer;
    private Vector3 originalScale;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        originalScale = transform.localScale;

        // Punch scale effect
        transform.localScale = originalScale * scalePunch;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Drift upward
        transform.localPosition += Vector3.up * floatSpeed * Time.deltaTime;

        // Scale back down smoothly
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 8f);

        // Fade out
        if (timer > lifetime * 0.4f)
        {
            float fade = 1f - ((timer - lifetime * 0.4f) / (lifetime * 0.6f));
            Color c = text.color;
            c.a = fade;
            text.color = c;
        }

        // Destroy when done
        if (timer >= lifetime)
            Destroy(gameObject);
    }

    public void SetText(string txt, Color color)
    {
        text.text = txt;
        text.color = color;
    }
}

