using UnityEngine;
using TMPro;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    public GameObject popupPrefab;
    public RectTransform popupParent;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDamage(int value, Vector3 worldPos, int playerNumber)
    {
        if (popupPrefab == null || popupParent == null)
        {
            Debug.LogError("DamagePopupManager not set up!");
            return;
        }

        // Spawn UI popup under Canvas
        GameObject popupObj = Instantiate(popupPrefab, popupParent);
        DamagePopupEffect effect = popupObj.GetComponent<DamagePopupEffect>();

        // Format text
        string text = "-" + value.ToString();

        // Color logic
        Color color = (playerNumber == 1) ? new Color(1f, 0f, 0f) : new Color(0.2f, 0.95f, 1f);

        // Apply to TMP
        effect.SetText(text, color);

        // Convert world position → UI anchored position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupParent,
            screenPos,
            null,
            out Vector2 localPos
        );

        popupObj.GetComponent<RectTransform>().anchoredPosition = localPos;
    }
}
