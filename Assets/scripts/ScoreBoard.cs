using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class ScoreBoard : MonoBehaviour {

    public GameObject textPrefeb;
    public Text[] names;
    public Text[] scores;

    private void Start()
    {
        refreshScoreBoard();
        this.gameObject.SetActive(false);
    }

    public void toggleScoreBoard()
    {
        if (this.gameObject.active) this.gameObject.SetActive(false);
        else
        {
            this.gameObject.SetActive(true);
        }
    }

    public void refreshScoreBoard()
    {
        for (int i = 0; i < 10; i++)
        {
            names[i].text = PlayerPrefs.GetString("highScoreName" + i.ToString(), "");
            if (PlayerPrefs.GetInt("highScore" + i, 0) != 0)
                scores[i].text = PlayerPrefs.GetInt("highScore" + i.ToString()).ToString();
            else scores[i].text = "";
        }
    }
    public void resetScoreBoard()
    {
        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.SetInt("highScore" + i.ToString(), 0);
            PlayerPrefs.SetString("highScoreName" + i.ToString(), "");
            names[i].text = "" ;       
            scores[i].text = "";          
        }
    }
}
