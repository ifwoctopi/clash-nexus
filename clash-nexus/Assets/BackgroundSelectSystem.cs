using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class BackgroundSelectSystem : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundButtonPair
    {
        public Button button;           // The UI button already in the scene
        public GameObject background;   // The prefab this button represents
    }

    public BackgroundButtonPair[] backgrounds;
    public string nextSceneName = "FightScene";

    private void Start()
    {
        foreach (var pair in backgrounds)
        {
            GameObject bg = pair.background;  // capture local ref for lambda
            pair.button.onClick.AddListener(() => SelectBackground(bg));
        }
    }

    private void SelectBackground(GameObject bg)
    {
        PlayerPrefs.SetString("SelectedBackground", bg.name);
        PlayerPrefs.Save();

        SceneManager.LoadScene(nextSceneName);
    }
}
