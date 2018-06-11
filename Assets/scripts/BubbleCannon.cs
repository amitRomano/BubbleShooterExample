using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleCannon : MonoBehaviour {
    public class messageFromCannon
    {
        Vector3 bubbleShotCoords;
        HexGrid.PosInGrid bubbleBeenHitPos;
    }
    public HexGrid grid;
    public Sprite[] bubbleColors;
    public Sprite[] amountOfBubblesIndicatorSprites;
    public Sprite[] shotPreviewColors;
    public GameObject amountOfBubblesIndicator;
    public GameObject nextBubbleIndicator;
    public GameController gameController;
    public GameObject shotPreview;

    new Rigidbody2D rigidbody;
    private float speed = 10f;
    Vector3 startPos;

    public GameObject nextBubblePreview;
    public GameObject bubblesUntillNewLineIndicator;

    BubbleNode.Color lastShotColor;
    BubbleNode.Color currentShotColor;
    BubbleNode.Color nextShotColor;

    private void Start()
    {
        nextShotColor = grid.colorPicker();
        resetShot();
        shotPreview.gameObject.SetActive(false);
        rigidbody = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }


    void Update ()
    {

    }


    private int getAmountOfBubbles()
    {
        return gameController.GetComponent<GameController>().amountOfBubblesUntilNextLine;
    }

    private bool isPaused()
    {
        return gameController.GetComponent<GameController>().isPaused;
    }

    public void aimShot(Vector3 swipeDelta)
    {
        if (isAimedTwardsBubbles(swipeDelta)) shotPreview.gameObject.SetActive(false);
        else
        {
            aimPreview(swipeDelta);
            if (MobileInput.Instance.release)
            {
                lastShotColor = currentShotColor;
                sendBubbleInDirection(swipeDelta.normalized);
                    shotPreview.gameObject.SetActive(false);
            }
        }   
    }

    public void resetShot()
    {
        currentShotColor = nextShotColor;
        nextShotColor = grid.colorPicker();
        updateShotColorIndicators();
        updateNextShotColorIndicators();
    }
    public void resetCannon()
    {
        resetPosAndVelocity();
        resetShot();
        resetShot();
    }

    private void updateShotColorIndicators()
    {
        setColor(currentShotColor, this.gameObject, bubbleColors);
        setColor(currentShotColor, shotPreview, shotPreviewColors);
    }

    private void updateNextShotColorIndicators()
    {
        setColor(nextShotColor, nextBubbleIndicator, bubbleColors);
    }

    private void aimPreview(Vector3 swipeDelta)
    {
        shotPreview.transform.parent.up = swipeDelta.normalized;
        shotPreview.gameObject.SetActive(true);
    }

    private static bool isAimedTwardsBubbles(Vector3 swipeDelta)
    {
        return swipeDelta.normalized.y < 0.3;
    }

    private void createBubbleAndMoveToNextShot(HexGrid.PosInGrid bubbleBeenHitPos)
    {
        grid.createBubble(bubbleBeenHitPos, currentShotColor, true);
        resetShot();
        HexGrid.bubbleShotColor = lastShotColor;
        resetPosAndVelocity();
    }

    private bool bubbleHitIsTopCollider(HexGrid.PosInGrid bubbleBeenHitPos)
    {
        return bubbleBeenHitPos.y == grid.getFirstLineY() + 1;
    }

    private void resetPosAndVelocity()
    {
        rigidbody.velocity = Vector3.zero;
        transform.position = startPos;
    }

    private void sendBubbleInDirection(Vector3 diraction)
    {
        GameController.isBubbleMidShot = true;
        rigidbody.velocity = diraction * speed;
    }

    public void setColor(BubbleNode.Color color, GameObject bubble, Sprite[] colorSprites)
    {
        switch (color)
        {
            case BubbleNode.Color.BLUE:
                bubble.GetComponent<SpriteRenderer>().sprite = colorSprites[0];
                break;
            case BubbleNode.Color.RED:
                bubble.GetComponent<SpriteRenderer>().sprite = colorSprites[1];
                break;
            default:
                bubble.GetComponent<SpriteRenderer>().sprite = colorSprites[2];
                break;
        }
    }

    public void updateShotScale(float scale)
    {
        this.transform.localScale = this.transform.localScale * scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "bubbleInGrid")
        {
            if (GameController.isBubbleMidShot)
                createBubbleAndMoveToNextShot(
                    collision.GetComponent<BubbleNode>().findSpawnPos(transform.position)
                        );
           
        }else if(collision.tag == "topCollider")
        {
            createBubbleAndMoveToNextShot(new HexGrid.PosInGrid(grid.findXforTopColliderHit(transform.position.x) ,grid.getFirstLineY()));
        }
        GameController.isBubbleMidShot = false;
    }


}
