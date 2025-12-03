using UnityEngine;

public class MatchStats : MonoBehaviour
{
    public static MatchStats Instance;

    // --- Combat Stats ---
    public int attacksThrown = 0;          // Player pressed attack buttons
    public int hitsLanded = 0;             // Player actually hit the CPU
    public int blocks = 0;                 // Player blocked (dash)
    public float totalDamageDealt = 0f;    // Player → CPU
    public float totalDamageTaken = 0f;    // CPU → Player

    // --- Combo Statistics ---
    public float currentComboDamage = 0f;
    public float maxComboDamage = 0f;
    public float comboTimeout = 1.0f;
    private float lastHitTime = 0f;

    // --- Score ---
    public int finalScore = 0;

    // --- Timer ---
    private float matchStartTime;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        ResetStats();
    }

    //                RESET ALL STATS
    public void ResetStats()
    {
        attacksThrown = 0;
        hitsLanded = 0;
        blocks = 0;
        totalDamageDealt = 0;
        totalDamageTaken = 0;

        currentComboDamage = 0;
        maxComboDamage = 0;

        matchStartTime = Time.time;
        finalScore = 0;
    }

    //                REGISTER EVENTS

    // Player pressed Attack1 / Attack2 / Attack3
    public void RegisterAttackThrown()
    {
        attacksThrown++;
    }

    // Player successfully hit CPU
    public void RegisterHit(float damage)
    {
        hitsLanded++;
        totalDamageDealt += damage;

        // Combo tracking
        if (Time.time - lastHitTime <= comboTimeout)
        {
            currentComboDamage += damage;
        }
        else
        {
            currentComboDamage = damage;
        }

        maxComboDamage = Mathf.Max(maxComboDamage, currentComboDamage);
        lastHitTime = Time.time;
    }

    // Player took damage from CPU
    public void RegisterHitTaken(float damage)
    {
        totalDamageTaken += damage;

        // Combo breaks when player gets hit
        currentComboDamage = 0f;
    }

    // Player blocked (dash)
    public void RegisterBlock()
    {
        blocks++;
    }

    //                CALCULATIONS
    public float GetAccuracy()
    {
        if (attacksThrown == 0) return 0f;
        return (float)hitsLanded / attacksThrown;
    }

    public float GetMatchTime()
    {
        return Time.time - matchStartTime;
    }

    public int CalculateFinalScore()
    {
        finalScore =
            Mathf.RoundToInt(totalDamageDealt * 2 +
                             maxComboDamage * 3 +
                             GetAccuracy() * 500 +
                             blocks * 10 -
                             totalDamageTaken);

        return finalScore;
    }
}