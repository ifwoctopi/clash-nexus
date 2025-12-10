using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MatchStatsUI : MonoBehaviour
{
    public static MatchStatsUI Instance;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // hidden at start
    }

    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI player1StatsText;
    public TextMeshProUGUI player2StatsText;
    public Button closeButton;

    private void Start()
    {

        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        UpdateStats();
    }

    public void UpdateStats()
    {
        var stats1 = PlayerStatsManager.Instance.GetStats(1);
        var stats2 = PlayerStatsManager.Instance.GetStats(2);

        if (titleText != null)
            titleText.text = "Match Stats";

        if (stats1 != null && player1StatsText != null)
        {
            player1StatsText.text =
                $"Player 1\n" +
                $"Damage Dealt: {stats1.totalDamageDealt}\n" +
                $"Hits Landed: {stats1.hitsLanded}\n" +
                $"Attack Attempts: {stats1.attackAttempts}\n" +
                $"KOs: {stats1.knockouts}";
        }

        if (stats2 != null && player2StatsText != null)
        {
            player2StatsText.text =
                $"Player 2\n" +
                $"Damage Dealt: {stats2.totalDamageDealt}\n" +
                $"Hits Landed: {stats2.hitsLanded}\n" +
                $"Attack Attempts: {stats2.attackAttempts}\n" +
                $"KOs: {stats2.knockouts}";
        }
    }
}
