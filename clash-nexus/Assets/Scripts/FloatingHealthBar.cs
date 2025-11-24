using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider; // assign in Inspector
    public float smoothSpeed = 5f;          // animation speed

    private float targetValue;              // what the slider should move toward

    // called to update slider values based on health system
    public void SetHealth(float current, float max)
    {
        targetValue = current / max;
    }

    private void Update()
    {
        if (slider != null)
        {
            slider.value = Mathf.Lerp(slider.value, targetValue, Time.deltaTime * smoothSpeed);
        }
    }
}