using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    public DamagePopup damagePopupPrefab;
    public Canvas floatingCanvas;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ShowDamagePopup(new Vector3(0, 0, 0), 999, Color.yellow);
    }
    public void ShowDamagePopup(Vector3 worldPos, int damage, Color color)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogError("DamagePopupManager: No prefab assigned!");
            return;
        }

        DamagePopup popup = Instantiate(damagePopupPrefab, floatingCanvas.transform);

        // call your correct INIT method
        popup.Init(damage, worldPos, color);
    }
}
