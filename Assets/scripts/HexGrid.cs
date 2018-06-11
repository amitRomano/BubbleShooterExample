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


    public int numOfColors = 3;
    [Range(4, 40)]
    public int objectsPerRow = 10;

    [Range(1, 15)]
    public int startLinesAmount;

    int startLinesLeft;

    float leftWallPosX;
    float yOffSet;
    float xOffSet;
    GameObject dropLineIndicator;

    bool shouldGridDrop = false;
    public static BubbleNode.Color bubbleShotColor;

    int nextNewLineY = 0;
    Stack explosionStack = new Stack();
    Queue connectedQueue = new Queue();

    HashSet<PosInGrid> bubblesToRemoveInCleanUp;
    HashSet<PosInGrid> bubblesChecked;
    Hashtable bubbleGrid = new Hashtable();
    public GameObject bubblePrefeb;

    float scale;
    int poppedBubbleCounter;

    public void initialize(BubbleCannon cannon, GameController gameController, Sprite[] colors)
    {
   
    }

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

    private float bubbleRadius()
    {
        return (xOffSet / 2f);
    }

    void Update ()
    {
        if (!isExplosionStackEmpty())
        {
            popBubbleDFS((PosInGrid)explosionStack.Pop());
            poppedBubbleCounter++;
            gameController.SendMessage("incrementScore", getScoreForBubble(poppedBubbleCounter));
            cleanUpAfterFinalPop();
        }

        if (shouldGridDrop) keepDroppingGrid();
    }

    private int getScoreForBubble(int poppedBubbleCounter)
    {
        if (poppedBubbleCounter < 4) return 10;
        else return (poppedBubbleCounter - 2) * 10;            
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
                gameController.SendMessage("incrementScore", poppedBubbleCounter * 100);
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

    private void initializeBubblesToRemove()
    {
        bubblesToRemoveInCleanUp = new HashSet<PosInGrid>();
        foreach (PosInGrid pos in bubbleGrid.Keys)
        {
            bubblesToRemoveInCleanUp.Add(pos);
        }
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

    private bool isExplosionStackEmpty()
    {
        return explosionStack.Count == 0;
    }

    private void keepDroppingGrid()
    {
        this.transform.position = Vector3.MoveTowards(this.transform.position, this.transform.position+Vector3.down, Time.deltaTime);
    }

    private void resetDropGridVar()
    {
        Destroy(dropLineIndicator);
        gameController.isPaused = false;
        if (startLinesLeft > 0) addStartLines();
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

    private void animateLineDrop()
    {
        gameController.isPaused = true;
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

    private void setNewDropLineIndicator()
    {
        dropLineIndicator = (GameObject)Instantiate(bubblePrefeb, getCoordsOf(new PosInGrid(0, nextNewLineY)), Quaternion.identity);
        dropLineIndicator.transform.localScale = dropLineIndicator.transform.localScale * scale;
        dropLineIndicator.GetComponent<SpriteRenderer>().sprite = null;
        dropLineIndicator.transform.SetParent(this.transform);
        dropLineIndicator.GetComponent<BubbleNode>().setAsLineIndicator(this);
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
        List<PosInGrid> sameColorNeighbors = findSameColorNeighbors(pos);
            gameController.isLastShotPoppedBubble = false;
            if (sameColorNeighbors.Count >= 2)
            {
                gameController.isLastShotPoppedBubble = true;
                explosionStack.Push(pos);
            }
            else if (sameColorNeighbors.Count == 1)
            {
                foreach (PosInGrid neighbor in sameColorNeighbors)
                {if(bubbleGrid.Contains(neighbor))
                    if (getBubbleNode(neighbor).isNextToSameColorNeighbor)
                        if (!getBubbleNode(neighbor).firstSameColorNeighbor.Equals(pos))
                        {
                            gameController.isLastShotPoppedBubble = true;
                                explosionStack.Push(pos);
                        }
                }
            }
            gameController.shotJoinedGrid();
        }
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

    public bool isLineLong(int yOfLine)
    {
        return yOfLine % 2 == 0;
    }

    private void createBubbleInShortLine(PosInGrid pos, BubbleNode.Color color)
    {
        GameObject bubble = (GameObject)Instantiate(bubblePrefeb, getCoordsOfShortLine(pos), Quaternion.identity);

        setBubbleAttributes(pos, bubble, color);
    }

    private void setBubbleAttributes(PosInGrid pos, GameObject bubble, BubbleNode.Color color)
    {
        bubble.name = "bubble " + pos.x + "_" + pos.y;
        bubble.transform.SetParent(this.transform);
        setBubbleColor(color, bubble); 
        insertBubbleToGrid(pos, bubble, color);
    }

    public Vector3 getCoordsOf(PosInGrid pos)
    {
        if (isLineLong(pos.y)) return getCoordsOfLongLine(pos);
        return getCoordsOfShortLine(pos);
    }

    private void createBubbleInLongLine(PosInGrid pos, BubbleNode.Color color)
    {
        GameObject bubble = (GameObject)Instantiate(bubblePrefeb, getCoordsOfLongLine(pos), Quaternion.identity);
        setBubbleAttributes(pos, bubble, color);

    }

    public Vector3 getCoordsOfLongLine(PosInGrid pos)
    {
        
        Vector3 coordsInGrid = new Vector3(leftWallPosX + pos.x * xOffSet, pos.y * yOffSet, 0);
        return this.transform.localPosition + coordsInGrid;
    }

    public Vector3 getCoordsOfShortLine(PosInGrid pos)
    {
        Vector3 coordsInGrid = new Vector3((leftWallPosX + pos.x * xOffSet + (xOffSet / 2)), pos.y * yOffSet, 0);
        return this.transform.localPosition + coordsInGrid;
    }

    public void insertBubbleToGrid(PosInGrid pos, GameObject bubble, BubbleNode.Color color)
    {
        bubbleGrid.Add(pos,bubble);
        getBubbleNode(pos).initialize(pos, color, this, gameController);
        updateSameColorAttributesForSelfAndNeighbors(pos);
        
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
    public List<PosInGrid> findSameColorNeighbors(HexGrid.PosInGrid pos)
    {
        List<PosInGrid> sameColorNeighbors = new List<PosInGrid>();
        foreach (Func<HexGrid.PosInGrid, HexGrid.PosInGrid> neighborFunc in neighborFunctions())
        {
            if (bubbleGrid.Contains(neighborFunc(pos)))
                if (isSameColorBubbles(pos, neighborFunc)) sameColorNeighbors.Add(neighborFunc(pos));
        }
        return sameColorNeighbors;
    }

    private bool isSameColorBubbles(PosInGrid pos, Func<PosInGrid, PosInGrid> neighborFunc)
    {
        return getBubbleNode(neighborFunc(pos)).getColor() == getBubbleNode(pos).getColor();
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
            foreach (Func<PosInGrid, PosInGrid> neighborFunc in neighborFunctions())
            {
                if (!bubblesChecked.Contains(neighborFunc(pos)))
                {
                    bubblesChecked.Add(neighborFunc(pos));
                    connectedQueue.Enqueue(neighborFunc(pos));
                }
            }
        }
    }

    public void popBubbleDFS(PosInGrid pos)
    {
        if (isBubbleExist(pos))
        {
            BubbleNode.Color color = getBubbleNode(pos).getColor();
            popBubble(pos);
            pushSameColorNeighborsToExplosionStack(pos, color);
        }
    }

    public void pushSameColorNeighborsToExplosionStack(PosInGrid pos, BubbleNode.Color colorPoped )
    {
        foreach (Func<PosInGrid, PosInGrid> neighborFunc in neighborFunctions())
        {
            if (isBubbleExist(neighborFunc(pos)))
                if (isColor(neighborFunc(pos), colorPoped))
                {
                    if (!getBubbleNodeFromGrid(neighborFunc(pos)).isSetToPop == true)
                        explosionStack.Push(neighborFunc(pos));
                        getBubbleNodeFromGrid(neighborFunc(pos)).isSetToPop = true;
                }
        }
    }

    void pushToExplosionStack(PosInGrid pos)
    {
        explosionStack.Push(pos);
    }


    public void popBubble(PosInGrid pos)
    {
        Destroy(getBubble(pos));
        bubbleGrid.Remove(pos);
    }

    private void setBubble(PosInGrid pos, GameObject bubble)
    {
        bubbleGrid.Add(pos, bubble);
    }

    private GameObject getBubble(PosInGrid pos)
    {
        return (GameObject)bubbleGrid[pos];
    }


    public bool isBubbleExist(PosInGrid pos)
    {
        return bubbleGrid.Contains(pos);
    }

    public bool isColor(PosInGrid pos, BubbleNode.Color expectedColor)
    {
        return colorOf(pos) == expectedColor;
    }

    public BubbleNode.Color colorOf(PosInGrid pos)
    {
        return getBubbleNode(pos).getColor();
    }
    
    private void updateSameColorAttributesForSelfAndNeighbors(PosInGrid pos)
    {
        foreach (Func<PosInGrid, PosInGrid> neighborFunc in neighborFunctions())
        {
            updateSameColorAttributesForSelfAndNeighbor(neighborFunc, pos);
        }
    }

    public void resetGrid()
    {
        popAllRemainingBubbles();
        poppedBubbleCounter = 0;
        startLinesLeft = startLinesAmount;
        addStartLines();
    }

    private void updateSameColorAttributesForSelfAndNeighbor(Func<PosInGrid, PosInGrid> getNeighbor, PosInGrid pos)
    {
        PosInGrid neighbor = getNeighbor(pos);
        if (isBubbleExist(neighbor))
            if (isColor(neighbor, getBubbleNode(pos).getColor()))
            {
                setFirstSameColorNeighborsIfFree(getNeighbor, pos, neighbor);
                setSameColorBool(getBubbleNode(neighbor), getBubbleNode(pos));
            }
    }

    private void setFirstSameColorNeighborsIfFree(Func<PosInGrid, PosInGrid> getNeighbor, PosInGrid pos, PosInGrid neighbor)
    {
        if (!getBubbleNode(neighbor).isNextToSameColorNeighbor)
            getBubbleNode(neighbor).firstSameColorNeighbor = pos;
        if (!getBubbleNode(pos).isNextToSameColorNeighbor)
            getBubbleNode(pos).firstSameColorNeighbor = getNeighbor(pos);
    }

    private void setSameColorBool(BubbleNode bubbleNode1, BubbleNode bubbleNode2)
    {
        bubbleNode1.isNextToSameColorNeighbor = true;
        bubbleNode2.isNextToSameColorNeighbor = true;
    }
    
    private BubbleNode getBubbleNode(PosInGrid pos)
    {
        return getBubble(pos).GetComponent<BubbleNode>();
    }

    public BubbleNode getBubbleNodeFromGrid(PosInGrid pos)
    {
        return getBubble(pos).GetComponent<BubbleNode>();
    }

    public List<Func<PosInGrid, PosInGrid>> neighborFunctions()
    {
        return new List<Func<PosInGrid, PosInGrid>>
            (new Func<PosInGrid, PosInGrid>[]
            { topLeftNeighbor, topRightNeighbor, botLeftNeighbor, botRightNeighbor, midLeftNeighbor, midRightNeighbor});
    }
    public PosInGrid midRightNeighbor(PosInGrid pos)
    {
        return new PosInGrid(pos.x + 1, pos.y);
    }
    public PosInGrid midLeftNeighbor(PosInGrid pos)
    {
        return new PosInGrid(pos.x - 1, pos.y);
    }
    public PosInGrid botRightNeighbor(PosInGrid pos)
    {
        PosInGrid neighborPos = new PosInGrid(pos.x, pos.y - 1);
        if (!isLineLong(pos.y)) neighborPos.x++;
        return neighborPos;
    }
    public PosInGrid botLeftNeighbor(PosInGrid pos)
    {
        PosInGrid neighborPos = new PosInGrid(pos.x - 1, pos.y - 1);
        if (!isLineLong(pos.y)) neighborPos.x++;
        return neighborPos;
    }
    public PosInGrid topRightNeighbor(PosInGrid pos)
    {
        PosInGrid neighborPos = new PosInGrid(pos.x, pos.y + 1);
        if (!isLineLong(pos.y)) neighborPos.x++;
        return neighborPos;
    }
    public PosInGrid topLeftNeighbor(PosInGrid pos)
    {
        PosInGrid neighborPos = new PosInGrid(pos.x - 1, pos.y + 1);
        if (!isLineLong(pos.y)) neighborPos.x++;
        return neighborPos;
    }

    public void stopGridDrop()
    {
        shouldGridDrop = false;
    }
}
