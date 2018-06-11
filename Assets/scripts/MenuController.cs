using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class MenuController : MonoBehaviour {

    public GameObject quitTab;
    public Text highScore;

    void Start () {
        quitTab.SetActive(false);
        Screen.orientation = ScreenOrientation.Portrait;
        highScore.text = PlayerPrefs.GetInt("highScore"+0.ToString(), 0).ToString();
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            quitTab.SetActive(true);
        }
	}

    public void quitApp()
    {
        Application.Quit();
    }

    public void toggleOffQuitTab()
    {
        quitTab.SetActive(false);
    }

    public void OnPlayClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
