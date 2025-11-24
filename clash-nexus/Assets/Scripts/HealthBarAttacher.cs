using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attaches a specific health bar canvas to a player/enemy and makes it follow them
/// </summary>
public class HealthBarAttacher : MonoBehaviour
{
    [Header("Health Bar Canvas")]
    [Tooltip("The specific canvas GameObject that contains the health bar UI")]
    public GameObject healthBarCanvas;

    [Header("Settings")]
    [Tooltip("Offset above the character")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private PlayerHealth playerHealth;
    private Slider healthSlider;

    void Start()
    {
        // Get PlayerHealth component
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning($"HealthBarAttacher: No PlayerHealth component found on {gameObject.name}");
            return;
        }

        // If canvas is assigned, attach it
        if (healthBarCanvas != null)
        {
            AttachHealthBar();
        }
        else
        {
            Debug.LogWarning($"HealthBarAttacher: Health bar canvas not assigned on {gameObject.name}");
        }
    }

    void AttachHealthBar()
    {
        // Attach the canvas to this character
        // Use worldPositionStays = false to reset the transform when parenting
        healthBarCanvas.transform.SetParent(transform, false);
        
        // Set local position, rotation, and scale
        healthBarCanvas.transform.localPosition = offset;
        healthBarCanvas.transform.localRotation = Quaternion.identity;
        healthBarCanvas.transform.localScale = Vector3.one;

        // Get the canvas component
        Canvas canvas = healthBarCanvas.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = healthBarCanvas.GetComponentInChildren<Canvas>();
        }

        // Make sure it's set to World Space
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            
            // Set the main camera if needed
            if (Camera.main != null)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        // Find the health slider in the canvas
        healthSlider = healthBarCanvas.GetComponentInChildren<Slider>();
        if (healthSlider == null)
        {
            Debug.LogWarning($"HealthBarAttacher: No Slider component found in health bar canvas on {gameObject.name}");
        }
        else
        {
            // Initialize slider
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        Debug.Log($"HealthBarAttacher: Attached health bar canvas to {gameObject.name}");
    }

    void Update()
    {
        if (playerHealth == null || healthSlider == null) return;

        // Update health bar value
        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
        healthSlider.value = healthPercent;

        // Update fill color (green for health, red background shows missing health)
        if (healthSlider.fillRect != null)
        {
            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.green; // Always green for current health
            }
        }

        // Make health bar face camera (billboard effect)
        if (healthBarCanvas != null && Camera.main != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - healthBarCanvas.transform.position;
            directionToCamera.y = 0; // Keep upright
            
            if (directionToCamera != Vector3.zero)
            {
                healthBarCanvas.transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
            }
        }
    }
}

