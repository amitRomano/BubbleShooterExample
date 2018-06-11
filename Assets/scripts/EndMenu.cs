using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class EndMenu : MonoBehaviour {
    public GameController gameController;
    public Text newHighScore;
    public Text endScore;
    public InputField inputName;
    public GameObject notHighScoreButtons;
    public GameObject scoreBoard;

    string playerName;

    void Start ()
    {
        gameObject.SetActive(false);
    }
    public void toggleEndMenuOff()
    {
        gameObject.SetActive(false);
    }
    public void toggleEndMenuOn(int score)
    {
        setEndScore(score);
        if (isTopTen(score)) showHighScoreMenu();
        else showNotHighScoreMenu();
        
        this.gameObject.SetActive(true);
    }

    public void saveHighScore()
   {
       playerName = inputName.text;
       int rank = findRank(gameController.getCurrentScore());
       dropAllLinesUnderRankOneDown(rank);
       setHighScoreInRank(gameController.getCurrentScore(), rank, playerName);
       scoreBoard.GetComponent<ScoreBoard>().refreshScoreBoard();

       scoreBoard.SetActive(true);
       this.gameObject.SetActive(false);
    }
    private int findRank(int score)
   {
       int i = 0;
       while (score < PlayerPrefs.GetInt("highScore" + i, 0))
       {
           i++;
       }
       return i;
   }
    private void dropAllLinesUnderRankOneDown(int rank)
   {
       for (int i = 8; i >= rank; i--)
       {
           dropOneLine(i);
       }
   }
    private void setHighScoreInRank(int score, int rank, string newName)
   {
        PlayerPrefs.SetString("highScoreName" + rank.ToString(), newName);
        PlayerPrefs.SetInt("highScore" + rank.ToString(), score);
   }
    private void dropOneLine(int i)
    {
        PlayerPrefs.SetString("highScoreName" + (i + 1).ToString(), PlayerPrefs.GetString("highScoreName" + i.ToString(), ""));
        PlayerPrefs.SetInt("highScore" + (i + 1).ToString(), PlayerPrefs.GetInt("highScore" + i.ToString(), 0));
    }

    private void showNotHighScoreMenu()
    {
        newHighScore.gameObject.SetActive(false);
        notHighScoreButtons.gameObject.SetActive(true);
    }
    private void showHighScoreMenu()
    {
        notHighScoreButtons.gameObject.SetActive(false);
        newHighScore.gameObject.SetActive(true);
    }
    private bool isTopTen(int score)
    {
        return (score >= PlayerPrefs.GetInt("highScore" + 9.ToString(), 1));
    }
    public void setEndScore(int score)
    {
        endScore.text = score.ToString();
    }
}
