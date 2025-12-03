using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TMP_Text text;
    public float lifetime = 0.7f;
    public float floatSpeed = 40f;

    private float timer;
    private RectTransform rect;
    private Vector3 worldPosition;
    private Color startColor;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }
        startColor = text.color;
    }

    // called right after instantiate
    public void Init(int amount, Vector3 worldPos, Color color)
    {
        worldPosition = worldPos;
        text.text = "-" + amount.ToString();
        text.color = color;
        startColor = color;

        // set initial position
        UpdateScreenPosition(0f);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // move upward a bit over time
        float yOffset = timer * 1.0f;
        UpdateScreenPosition(yOffset);

        // fade out over lifetime
        float t = timer / lifetime;
        Color c = text.color;
        c.a = Mathf.Lerp(1f, 0f, t);
        text.color = c;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateScreenPosition(float yOffset)
    {
        if (Camera.main == null) return;

        Vector3 world = worldPosition + new Vector3(0, yOffset, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(world);
        rect.position = screenPos;
    }
}
