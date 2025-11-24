using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button that cycles through timer options: 30s, 60s, 90s, 120s, Unlimited
/// Attach this to a button in the mode selection or character select screen.
/// </summary>
public class TimerSelectionButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component that displays the current timer selection (optional)")]
    [SerializeField] private Text timerText;

    [Header("Timer Options")]
    [Tooltip("Available timer durations in seconds. -1 represents unlimited time.")]
    [SerializeField] private float[] timerOptions = { -1f, 30f, 60f, 90f, 120f };

    private int currentTimerIndex = 0; // Start at 0, which is Unlimited (-1)
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = GetComponentInParent<Button>();
        }

        // If no text component assigned, try to find one in children
        if (timerText == null)
        {
            timerText = GetComponentInChildren<Text>();
        }
    }

    void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        // Initialize timer in GameDataManager
        UpdateTimer();
        UpdateDisplay();
    }

    /// <summary>
    /// Called when the button is clicked - cycles to next timer option
    /// </summary>
    private void OnButtonClicked()
    {
        // Cycle to next timer option
        currentTimerIndex = (currentTimerIndex + 1) % timerOptions.Length;
        
        UpdateTimer();
        UpdateDisplay();
    }

    /// <summary>
    /// Updates the timer in GameDataManager
    /// </summary>
    private void UpdateTimer()
    {
        float selectedTimer = timerOptions[currentTimerIndex];
        GameDataManager.Instance.SetMatchTimer(selectedTimer);
    }

    /// <summary>
    /// Updates the button text to show current timer selection
    /// </summary>
    private void UpdateDisplay()
    {
        if (timerText != null)
        {
            float selectedTimer = timerOptions[currentTimerIndex];
            if (selectedTimer < 0f)
            {
                timerText.text = "Unlimited Time";
            }
            else
            {
                timerText.text = $"{selectedTimer}s";
            }
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

