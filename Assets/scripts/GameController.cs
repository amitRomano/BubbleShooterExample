using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {
    public GameObject quitTab;
    public GameObject scoreBoard;
    public Sprite[] bubbleColors;
    public Sprite[] amountOfBubblesIndicatorSprites;
    public GameObject endMenu;
    public Text highScore;
    public Text score;
    public bool isPaused;
    public HexGrid grid;
    public BubbleCannon bubbleCannon;
    public static bool isBubbleMidShot;
    public GameObject nextBubbleIndicator;
    public GameObject amountOfBubblesIndicator;
    public int amountOfBubblesUntilNextLine;
    public bool isLastShotPoppedBubble;
    int currentScore;
    private bool isGameOver;

    void Start ()
    {
        quitTab.SetActive(false);
        Screen.orientation = ScreenOrientation.Portrait;
        resetScores();
        amountOfBubblesUntilNextLine = Random.Range(0, amountOfBubblesIndicatorSprites.Length);
        updateAmountOfBubblesIndicator();
    }

    void Update () {
        if (!isBubbleMidShot && !isPaused && !isGameOver
            && amountOfBubblesUntilNextLine >= 0)
            poolInput();
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
    private void resetScores()
    {
        currentScore = 0;
        setScoreIndicator(currentScore);
        setHighestScoreIndicator(PlayerPrefs.GetInt("highScore"+0.ToString(), 0));
    }

    public void restart()
    {
        if (!isPaused || isGameOver)
        {
            scoreBoard.SetActive(false);
           endMenu.SetActive(false);
           isGameOver = false;
           grid.SendMessage("resetGrid");
           bubbleCannon.SendMessage("resetCannon");
           Start();
        }
    }

    public int getCurrentScore()
    {
        return currentScore;
    }

    private void incrementScore(int amount)
    {
        currentScore += amount;
        setScoreIndicator(currentScore);
        updateHighScoreIfNeeded();
    }

    private void setHighestScoreIndicator(int newHighScore)
    {
        Debug.Log("inSetHigh: " + newHighScore);
        highScore.text = "High Score: " + newHighScore.ToString();
    }
    private void setScoreIndicator(int newScore)
    {
        score.text = "Score: " +newScore.ToString();
    }

    private void updateHighScoreIfNeeded()
    {

        if (currentScore >= PlayerPrefs.GetInt("highScore" + 0.ToString(), 0))
        {
            Debug.Log("in if: " + currentScore);
            setHighestScoreIndicator(currentScore);
        }
    }

    private void poolInput()
    {
        Vector3 swipeDelta = MobileInput.Instance.swipeDelta;
        swipeDelta.Set(-swipeDelta.x, -swipeDelta.y, swipeDelta.z);
        if (isCurrentlyDragging(swipeDelta))
        {
            bubbleCannon.aimShot(swipeDelta);
        }
    }

    private static bool isCurrentlyDragging(Vector3 swipeDelta)
    {
        return swipeDelta != Vector3.zero;
    }

    public void shotJoinedGrid()
    {
        if (!isLastShotPoppedBubble)
        {
            if (amountOfBubblesUntilNextLine > 0)
            {
                amountOfBubblesUntilNextLine--;
                updateAmountOfBubblesIndicator();
            }
            else handleShotsEnded();
        }
    }

    public void handleShotsEnded()
    {
        isPaused = true;
        amountOfBubblesUntilNextLine = Random.Range(0, amountOfBubblesIndicatorSprites.Length);
        updateAmountOfBubblesIndicator();
        StartCoroutine("dropLine");
    }
    public void togglePause()
    {
        if (isPaused) isPaused = false;
        else isPaused = true;
    }
    public void updateAmountOfBubblesIndicator()
    {
        amountOfBubblesIndicator.GetComponent<SpriteRenderer>().sprite = 
        amountOfBubblesIndicatorSprites[amountOfBubblesUntilNextLine];
    }

    public void gameOver()
    {
        isGameOver = true;
        endMenu.GetComponent<EndMenu>().toggleEndMenuOn(currentScore);
    }

    IEnumerator dropLine()
    {
        yield return new WaitForSeconds(0.1f);
        grid.GetComponent<HexGrid>().addNewLine();
    }
}
