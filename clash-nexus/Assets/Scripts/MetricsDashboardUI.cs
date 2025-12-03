using UnityEngine;
using TMPro;

public class MetricsDashboardUI : MonoBehaviour
{
    public TMP_Text totalDamage;
    public TMP_Text maxCombo;
    public TMP_Text accuracy;
    public TMP_Text blocks;
    public TMP_Text damageTaken;
    public TMP_Text matchTime;
    public TMP_Text finalScore;

    private void OnEnable()
    {
        var stats = MatchStats.Instance;
        if (stats == null)
        {
            Debug.LogWarning("MetricsDashboardUI: MatchStats.Instance is null");
            return;
        }

        totalDamage.text = $"Total Damage: {Mathf.RoundToInt(stats.totalDamageDealt)}";
        maxCombo.text = $"Max Combo: {Mathf.RoundToInt(stats.maxComboDamage)}";
        accuracy.text = $"Accuracy: {(stats.GetAccuracy() * 100f):0.0}%";
        blocks.text = $"Blocks: {stats.blocks}";
        damageTaken.text = $"Damage Taken: {Mathf.RoundToInt(stats.totalDamageTaken)}";
        matchTime.text = $"Match Time: {stats.GetMatchTime():0.0}s";

        int score = stats.CalculateFinalScore();
        finalScore.text = $"Final Score: {score}";
    }
}