using System;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour {
    public struct PosInGrid
    {
        public int x;
        public int y;
        public PosInGrid(int posX, int posY)
        {
            x = posX;
            y = posY;
        }
    }

    public BubbleCannon cannon;
    public GameController gameController;
    public Sprite[] colors;
    public GameObject bubblePrefeb;
    public GameObject dropLineIndicatorPrefeb;

    public int numOfColors = 3;
    [Range(4, 40)]
    public int objectsPerRow = 10;

    [Range(1, 15)]
    public int startLinesAmount;
    int startLinesLeft;
    int nextNewLineY = 0;

    float scale;
    float leftWallPosX;
    float yOffSet;
    float xOffSet;

    bool shouldGridDrop = false;

    int poppedBubbleCounter;
    Stack explosionStack = new Stack();
    Queue connectedQueue = new Queue();

    HashSet<PosInGrid> bubblesToRemoveInCleanUp;
    HashSet<PosInGrid> bubblesChecked;
    Hashtable bubbleGrid = new Hashtable();

    void Start ()
    {
        scale = 7.6f / objectsPerRow;
        cannon.updateShotScale(scale);
        //0.63 is the bubble sprite diamiter
        leftWallPosX = (0.63f / 2f) * scale;
        xOffSet = (0.63f) * scale;
        //xOffSet is the bubble diameter 
        yOffSet = bubbleRadius() * Mathf.Sqrt(3f);
        startLinesLeft = startLinesAmount;
        addStartLines();
        poppedBubbleCounter = 0;
    }


    void Update ()
    {
        if (!isExplosionStackEmpty())
        {
            popBubbleDFS((PosInGrid)explosionStack.Pop());
            poppedBubbleCounter++;
            gameController.SendMessage("incrementScore", getScoreForBubble(poppedBubbleCounter , true));
            cleanUpAfterFinalPop();
        }

        if (shouldGridDrop) keepDroppingGrid();
    }

    private void addStartLines()
    {
        addNewLine();
        startLinesLeft--;
    }
    public void addNewLine()
    {
        createLine();
        animateLineDrop();
        nextNewLineY++;
    }
    private void createLine()
    {
        shouldGridDrop = true;
        setNewDropLineIndicator();
        for (int x = 0; x < objectsPerRow; x++)
        {
            createBubble(new PosInGrid(x, nextNewLineY), colorPicker(), false);
        }
    }
    private void animateLineDrop()
    {
        gameController.isPaused = true;
    }
    private void keepDroppingGrid()
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, this.transform.position+Vector3.down, Time.deltaTime);
    }
    private void setNewDropLineIndicator()
    {
        GameObject dropLineIndicator = Instantiate(dropLineIndicatorPrefeb, getCoordsOf(new PosInGrid(0, nextNewLineY)), Quaternion.identity);
        dropLineIndicator.transform.localScale = dropLineIndicator.transform.localScale * scale;
        dropLineIndicator.transform.SetParent(this.transform);
        dropLineIndicator.GetComponent<LineIndicator>().setGrid(this);
    }
    private void resetDropGridVar()
    {
        gameController.isPaused = false;
        if (startLinesLeft > 0) addStartLines();
    }

    private int getScoreForBubble(int poppedBubbleCounter, bool isFromExplosion)
    {
        if (isFromExplosion)
            if (poppedBubbleCounter < 4) return 10;
            else return (poppedBubbleCounter - 2) * 10;
        else return poppedBubbleCounter * 100; //more points for hanging bubbles
    }
    private void cleanUpAfterFinalPop()
    {
        if (isExplosionStackEmpty())
        {
            poppedBubbleCounter = 0;
            List<PosInGrid> firstRowBubbles = findFirstRowBubbles();
            if (firstRowBubbles.Count == 0)
            {
                popAllRemainingBubbles();
                gameController.SendMessage("incrementScore", getScoreForBubble(poppedBubbleCounter, false));
                poppedBubbleCounter = 0;
                gameController.SendMessage("gameOver");
            }
            else
            {
                // put every buble(pos) in a list, remove bubbles that are connected to a first
                // row bubble from said list (using BFS) and pop the remaining bubbles.
                initializeBubblesToRemove();
                bubblesChecked = new HashSet<PosInGrid>();
                foreach(PosInGrid firstRowBubble in firstRowBubbles)
                {
                    removeConnectedFromBubblesToRemoveBFS(firstRowBubble);
                }
                foreach (PosInGrid bubble in bubblesToRemoveInCleanUp)
                {
                    if (bubblesToRemoveInCleanUp.Contains(bubble))
                    {
                        poppedBubbleCounter++;
                        popBubble(bubble);
                    }
                }
                gameController.SendMessage("incrementScore", poppedBubbleCounter * 100);
                poppedBubbleCounter = 0;
            }
        }
    }
    private void popAllRemainingBubbles()
    {
        List<PosInGrid> leftOverBubbles = new List<PosInGrid>(); ;
        foreach (PosInGrid bubblePos in bubbleGrid.Keys)
        {
            leftOverBubbles.Add(bubblePos);
        }

        foreach (PosInGrid bubblePos in leftOverBubbles)
        {
            poppedBubbleCounter++;
            popBubble(bubblePos);
        }
    }

    private bool isExplosionStackEmpty()
    {
        return explosionStack.Count == 0;
    }
    private void startExplosionIfNeeded(PosInGrid pos)
    {
        List<PosInGrid> sameColorNeighbors = getBubbleNode(pos).findSameColorNeighbors();        
        if (sameColorNeighbors.Count >= 2)
        {
            startExplosion(pos);
        }
        else if (sameColorNeighbors.Count == 1)
        {
            foreach (PosInGrid neighbor in sameColorNeighbors)
            {
                if (bubbleBeenHitIsTouchingOtherSameColorBubble(pos, neighbor))
                {
                    startExplosion(pos);
                }
            }
        }
        else gameController.isLastShotPoppedBubble = false;
    }
    private bool bubbleBeenHitIsTouchingOtherSameColorBubble(PosInGrid pos, PosInGrid neighbor)
    {
        return getBubbleNode(neighbor).isNextToSameColorNeighbor &&
                            !getBubbleNode(neighbor).firstSameColorNeighbor.Equals(pos);
    }
    private void startExplosion(PosInGrid pos)
    {
        gameController.isLastShotPoppedBubble = true;
        explosionStack.Push(pos);
    }
    private List<PosInGrid> findFirstRowBubbles()
    {
        List<PosInGrid> firstRowBubbles = new List<PosInGrid>();
        for (int x = 0; x <= objectsPerRow; x++)
        {
            PosInGrid posInQuestion = new PosInGrid(x, getFirstLineY());
            if (bubbleGrid.Contains(posInQuestion)) firstRowBubbles.Add(posInQuestion);
        }
        return firstRowBubbles;
    }
    public int getFirstLineY()
    {
        return nextNewLineY - 1;
    }
    
    public bool isLineLong(int yOfLine)
    {
        return yOfLine % 2 == 0;
    }
    private bool isValidForPlacement(PosInGrid pos)
    {
        if (isLineLong(pos.y)) return pos.x >= 0 && pos.x < objectsPerRow;
        else return pos.x >= 0 && pos.x < objectsPerRow - 1;
    }
    private bool isXOutOfBoundsForShortLine(int x)
    {
        return x == objectsPerRow - 1;
    }
    private void createBubbleInLongLine(PosInGrid pos, BubbleNode.Color color)
    {
        GameObject bubble = (GameObject)Instantiate(bubblePrefeb, getCoordsOfLongLine(pos), Quaternion.identity);
        setBubbleAttributes(pos, bubble, color);

    }
    private void createBubbleInShortLine(PosInGrid pos, BubbleNode.Color color)
    {
        GameObject bubble = (GameObject)Instantiate(bubblePrefeb, getCoordsOfShortLine(pos), Quaternion.identity);

        setBubbleAttributes(pos, bubble, color);
    }
    private float bubbleRadius()
    {
        return (xOffSet / 2f);
    }
    private Vector3 getCoordsOf(PosInGrid pos)
    {
        if (isLineLong(pos.y)) return getCoordsOfLongLine(pos);
        return getCoordsOfShortLine(pos);
    }
    private Vector3 getCoordsOfLongLine(PosInGrid pos)
    {
        
        Vector3 coordsInGrid = new Vector3(leftWallPosX + pos.x * xOffSet, pos.y * yOffSet, 0);
        return this.transform.localPosition + coordsInGrid;
    }
    private Vector3 getCoordsOfShortLine(PosInGrid pos)
    {
        Vector3 coordsInGrid = new Vector3((leftWallPosX + pos.x * xOffSet + (xOffSet / 2)), pos.y * yOffSet, 0);
        return this.transform.localPosition + coordsInGrid;
    }
    public int findXforTopColliderHit(float x)
    {
        if (isLineLong(getFirstLineY()))
        {
            return Mathf.RoundToInt((x-leftWallPosX)/ xOffSet);
        }
        else
        {
            return Mathf.Min(objectsPerRow - 1, Mathf.RoundToInt((x - (xOffSet/2) -leftWallPosX) / xOffSet));
        }
    }

    private void initializeBubblesToRemove()
    {
        bubblesToRemoveInCleanUp = new HashSet<PosInGrid>();
        foreach (PosInGrid pos in bubbleGrid.Keys)
        {
            bubblesToRemoveInCleanUp.Add(pos);
        }
    }
    private void removeConnectedFromBubblesToRemoveBFS(PosInGrid firstRowBubble)
    {
        connectedQueue.Enqueue(firstRowBubble);
        bubblesChecked.Add(firstRowBubble);
        while (connectedQueue.Count > 0)
        {
            removeConnectedBubbles((PosInGrid)connectedQueue.Dequeue());
        }
    }
    private void removeConnectedBubbles(PosInGrid pos)
    {
        if (isBubbleExist(pos))
        {
            bubblesToRemoveInCleanUp.Remove(pos);
            foreach (PosInGrid neighbor in getBubbleNode(pos).neighborsPos)
            {
                if (!bubblesChecked.Contains(neighbor))
                {
                    bubblesChecked.Add(neighbor);
                    connectedQueue.Enqueue(neighbor);
                }
            }
        }
    }

    private void popBubbleDFS(PosInGrid pos)
    {
        if (isBubbleExist(pos))
        {
            BubbleNode.Color color = getBubbleNode(pos).getColor();
            pushSameColorNeighborsToExplosionStack(pos, color);
            popBubble(pos);
        }
    }
    private void pushSameColorNeighborsToExplosionStack(PosInGrid pos, BubbleNode.Color colorPoped )
    {
        foreach (PosInGrid neighbor in getBubbleNode(pos).neighborsPos)
        {
            if (isBubbleExist(neighbor))
                if (isColorMatch(neighbor, colorPoped) && !getBubbleNode(neighbor).isSetToPop)
                {
                    explosionStack.Push(neighbor);
                    getBubbleNode(neighbor).isSetToPop = true;
                }
        }    
    }
    private void pushToExplosionStack(PosInGrid pos)
    {
        explosionStack.Push(pos);

                       }
    private void popBubble(PosInGrid pos)
    {
        Destroy(getBubble(pos));
        bubbleGrid.Remove(pos);
    }

    //will not creat if pos is out of bounds or if position is taken
    public void createBubble(PosInGrid pos, BubbleNode.Color color, bool isFromCannon)
    {
        if (!isBubbleExist(pos) && isValidForPlacement(pos))
        {
            GameObject bubble = (GameObject)Instantiate(bubblePrefeb, getCoordsOf(pos), Quaternion.identity);
            bubble.transform.localScale = bubble.transform.localScale * scale;
            setBubbleAttributes(pos, bubble, color);

        }
        if (isFromCannon)
        {
            startExplosionIfNeeded(pos);
            gameController.shotHitGrid();
        }
    }

    private bool isColorMatch(PosInGrid pos, BubbleNode.Color expectedColor)
    {
        return colorOf(pos) == expectedColor;
    }

    private void setBubble(PosInGrid pos, GameObject bubble)
    {
        bubbleGrid.Add(pos, bubble);
    }  
    private void setBubbleAttributes(PosInGrid pos, GameObject bubble, BubbleNode.Color color)
    {
        bubble.name = "bubble " + pos.x + "_" + pos.y;
        bubble.transform.SetParent(this.transform);
        setBubbleColor(color, bubble); 
        insertBubbleToGrid(pos, bubble, color);
    }
    public void setBubbleColor(BubbleNode.Color color, GameObject bubble)
    {
        switch (color)
        {
            case BubbleNode.Color.BLUE:
                bubble.GetComponent<SpriteRenderer>().sprite = colors[0];
                break;

            case BubbleNode.Color.RED:
                bubble.GetComponent<SpriteRenderer>().sprite = colors[1];
                break;

            default:
                bubble.GetComponent<SpriteRenderer>().sprite = colors[2];
                break;
        }
    }
    public BubbleNode.Color colorOf(PosInGrid pos)
    {
        return getBubbleNode(pos).getColor();
    }
    public BubbleNode.Color colorPicker()
    {
        int colorNum = UnityEngine.Random.Range(0, 3);
        switch (colorNum)
        {
            case 0:
                return BubbleNode.Color.BLUE;
            case 1:
                return BubbleNode.Color.RED;
            default:
                return BubbleNode.Color.YELLOW;
        }
    }
    private GameObject getBubble(PosInGrid pos)
    {
        return (GameObject)bubbleGrid[pos];
    }
    public BubbleNode getBubbleNode(PosInGrid pos)
    {
        return getBubble(pos).GetComponent<BubbleNode>();
    }
    private void insertBubbleToGrid(PosInGrid pos, GameObject bubble, BubbleNode.Color color)
    {
        bubbleGrid.Add(pos,bubble);
        getBubbleNode(pos).initialize(pos, color, this, gameController);
        getBubbleNode(pos).updateSameColorAttributesForSelfAndNeighbors();      
    }
    public bool isBubbleExist(PosInGrid pos)
    {
        return bubbleGrid.Contains(pos);
    }

    public void resetGrid()
    {
        popAllRemainingBubbles();
        poppedBubbleCounter = 0;
        startLinesLeft = startLinesAmount;
        addStartLines();
    }
    public void stopGridDrop()
    {
        shouldGridDrop = false;
    }
}
