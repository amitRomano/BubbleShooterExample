using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleNode : MonoBehaviour
{
    public enum Color { BLUE, YELLOW, RED }

    GameController gameController;

    public List<HexGrid.PosInGrid> neighborsPos;
    private HexGrid grid;
    bool isNextToSameColorFlag;
    public HexGrid.PosInGrid firstSameColorNeighbor;
    bool isSetToPop;
    HexGrid.PosInGrid pos;
    Color color;

    public void initialize(HexGrid.PosInGrid pos, Color color, HexGrid grid, GameController gameController)
    {
        this.gameController = gameController;
        this.grid = grid;
        this.pos = pos;
        this.color = color;
        setNeighborsPos();
    }
    public void adjustPosAfterLineDrop()
    {
        setPos(new HexGrid.PosInGrid(getXPos(), getYPos() + 1));        
    }
    public void updateGameObjectName()
    {
        gameObject.name = "bubble " + getPos().x + "_" + getPos().y;
    }

    public bool getIsSetToPop()
    {
        return isSetToPop;
    }
    public void setToPop()
    {
        isSetToPop = true;
    }
    public bool isNextToSameColor()
    {
        return isNextToSameColorFlag;
    }
    public void setNextToSameColor()
    {
        isNextToSameColorFlag = true;
    }
    public int getXPos()
    {
        return pos.x;
    }
    public int getYPos()
    {
        return pos.y;
    }
    public HexGrid.PosInGrid getPos()
    {
        return this.pos;
    }
    public void setPos(HexGrid.PosInGrid newPos)
    {
        this.pos = newPos;
    }
    public Color getColor()
    {
        return this.color;
    }

    public void updateSameColorAttributesForSelfAndNeighbors()
    {
        foreach (HexGrid.PosInGrid neighbor in neighborsPos)
        {
            updateSameColorAttributesForSelfAndNeighbor(neighbor);
        }
    }
    private void updateSameColorAttributesForSelfAndNeighbor(HexGrid.PosInGrid neighbor)
    {
        if (grid.isBubbleExist(neighbor))
            if (isColorMatch(neighbor))
            {
                setFirstSameColorNeighborsIfFree(neighbor);
                setSameColorBool(grid.getBubbleNode(neighbor));
            }
    }
    private void setFirstSameColorNeighborsIfFree(HexGrid.PosInGrid neighbor)
    {
            if (!grid.getBubbleNode(neighbor).isNextToSameColorFlag)
                grid.getBubbleNode(neighbor).firstSameColorNeighbor = pos;
            if (!isNextToSameColorFlag)
                firstSameColorNeighbor = neighbor;
    }
    private void setSameColorBool(BubbleNode neighborNode)
    {
        neighborNode.isNextToSameColorFlag = true;
        isNextToSameColorFlag = true;
    }

    public HexGrid.PosInGrid findSpawnPos(Vector3 shotCoords)
    {
        Vector3 angleVector = (this.transform.position - shotCoords).normalized;
        if (isHitLeftSide(angleVector) || isByRightWall()) return findInLeftSide(angleVector);
        else return findInRightSide(angleVector);
    }
    private HexGrid.PosInGrid findInRightSide(Vector3 angleVector)
    {
        if (isHitTop(angleVector) && isBelowFirstRow(pos) && !grid.isBubbleExist(topRightNeighbor()))
            return topRightNeighbor();
        else if (isHitMid(angleVector) && !grid.isBubbleExist(midRightNeighbor()))
            return midRightNeighbor();
        else return botRightNeighbor();
    }
    private HexGrid.PosInGrid findInLeftSide(Vector3 angleVector)
    {
        if (isHitTop(angleVector) && isBelowFirstRow(pos) && !grid.isBubbleExist(topLeftNeighbor()))
            return topLeftNeighbor();
        else if (isHitMid(angleVector) && !grid.isBubbleExist(midLeftNeighbor()))
            return midLeftNeighbor();
        else return botLeftNeighbor();
    }
    private bool isHitMid(Vector3 angleVector)
    {
        return angleVector.y <= 0.33;
    }
    private bool isHitTop(Vector3 angleVector)
    {
        return angleVector.y <= -0.33;
    }
    private bool isHitLeftSide(Vector3 angleVector)
    {
        return (angleVector.x >= 0 && !isByLeftWall());
    }
    private bool isByRightWall()
    {
        return grid.isLineLong(pos.y) && pos.x == grid.objectsPerRow - 1;
    }
    private bool isByLeftWall()
    {
        return grid.isLineLong(pos.y) && pos.x == 0;
    }
    private bool isBelowFirstRow(HexGrid.PosInGrid bubbleBeenHitPos)
    {
        return bubbleBeenHitPos.y > 0;
    }

    public List<HexGrid.PosInGrid> findSameColorNeighbors()
    {
        List<HexGrid.PosInGrid> sameColorNeighbors = new List<HexGrid.PosInGrid>();
            foreach (HexGrid.PosInGrid neighbor in neighborsPos)
            {
                if (grid.isBubbleExist(neighbor))
                    if (isColorMatch(neighbor)) sameColorNeighbors.Add(neighbor);
            }      
        return sameColorNeighbors;
    }
    private bool isColorMatch(HexGrid.PosInGrid neighborPos)
    {
        return grid.colorOf(neighborPos) == color;
    }

    private void setNeighborsPos()
    {
        neighborsPos = new List<HexGrid.PosInGrid> (new HexGrid.PosInGrid[]{ 
            topLeftNeighbor(), topRightNeighbor(), botLeftNeighbor(),
                botRightNeighbor(), midLeftNeighbor(), midRightNeighbor()});
    }

    private HexGrid.PosInGrid midRightNeighbor()
    {
        return new HexGrid.PosInGrid(pos.x + 1, pos.y);
    }
    private HexGrid.PosInGrid midLeftNeighbor()
    {
        return new HexGrid.PosInGrid(pos.x - 1, pos.y);
    }
    private HexGrid.PosInGrid botRightNeighbor()
    {
        HexGrid.PosInGrid neighborPos = new HexGrid.PosInGrid(pos.x, pos.y - 1);
        if (!grid.isLineLong(pos.y)) neighborPos.x++;
        return neighborPos;
    }
    private HexGrid.PosInGrid botLeftNeighbor()
    {
        HexGrid.PosInGrid neighborPos = new HexGrid.PosInGrid(pos.x - 1, pos.y - 1);
        if (!grid.isLineLong(pos.y)) neighborPos.x++;
        return neighborPos;
    }
    private HexGrid.PosInGrid topRightNeighbor()
    {
        HexGrid.PosInGrid neighborPos = new HexGrid.PosInGrid(pos.x, pos.y + 1);
        if (!grid.isLineLong(pos.y)) neighborPos.x++;
        return neighborPos;
    }
    private HexGrid.PosInGrid topLeftNeighbor()
    {
        HexGrid.PosInGrid neighborPos = new HexGrid.PosInGrid(pos.x - 1, pos.y + 1);
        if (!grid.isLineLong(pos.y)) neighborPos.x++;
        return neighborPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "bubbleShot")
        {

        }
        else if (collision.tag == "botCollider")
        {
            gameController.SendMessage("gameOver");
        }
    
}

}
