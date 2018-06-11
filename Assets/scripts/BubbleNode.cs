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
    public bool isNextToSameColorNeighbor;
    public HexGrid.PosInGrid firstSameColorNeighbor;
    public bool isSetToPop;
    HexGrid.PosInGrid pos;
    Color color;
    bool isLineIndicator;

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
            if (!grid.getBubbleNode(neighbor).isNextToSameColorNeighbor)
                grid.getBubbleNode(neighbor).firstSameColorNeighbor = pos;
            if (!isNextToSameColorNeighbor)
                firstSameColorNeighbor = neighbor;
    }
    private void setSameColorBool(BubbleNode neighborNode)
    {
        neighborNode.isNextToSameColorNeighbor = true;
        isNextToSameColorNeighbor = true;
    }

    public HexGrid.PosInGrid findSpawnPos(Vector3 shotCoords)
    {
        Vector3 angleVector = (this.transform.position - shotCoords).normalized;
        if (isHitOnLeftSide(angleVector) || isByRightWall()) return findInLeftSide(angleVector);
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
    private bool isHitOnLeftSide(Vector3 angleVector)
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
    private static bool isBelowFirstRow(HexGrid.PosInGrid bubbleBeenHitPos)
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
    private List<Func<HexGrid.PosInGrid>> neighborFunctions()
    {
        return new List<Func<HexGrid.PosInGrid>>
            (new Func<HexGrid.PosInGrid>[]
            { topLeftNeighbor, topRightNeighbor, botLeftNeighbor,
                botRightNeighbor, midLeftNeighbor, midRightNeighbor});
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

    public void setAsLineIndicator(HexGrid grid)
    {
        this.grid = grid;
        isLineIndicator = true;
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
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isLineIndicator && collision.tag == "topCollider")
        {
            grid.SendMessage("stopGridDrop");
            grid.SendMessage("resetDropGridVar");
        }
    }
}
