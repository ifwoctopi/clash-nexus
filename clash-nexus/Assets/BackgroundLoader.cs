using UnityEngine;

public class BackgroundLoader : MonoBehaviour
{
    public GameObject[] backgroundPrefabs;

    void Start()
    {
        string bgName = PlayerPrefs.GetString("SelectedBackground", "");

        if (string.IsNullOrEmpty(bgName)) 
        {
            Debug.LogWarning("No background selected. Loading default.");
            Instantiate(backgroundPrefabs[0]);
            return;
        }

        foreach (GameObject bg in backgroundPrefabs)
        {
            if (bg.name == bgName)
            {
                Instantiate(bg);
                return;
            }
        }

        Debug.LogWarning("Background name not found. Loading default.");
        Instantiate(backgroundPrefabs[0]);
    }
}