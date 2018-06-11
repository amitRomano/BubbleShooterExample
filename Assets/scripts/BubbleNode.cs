using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleNode : MonoBehaviour
{
    public enum Color { BLUE, YELLOW, RED }

    GameController gameController;

    HexGrid grid;
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

    public void updateGameObjectName()
    {
        gameObject.name = "bubble " + getPos().x + "_" + getPos().y;
    }
    public void adjustPosAfterLineDrop()
    {
        setPos(new HexGrid.PosInGrid(getXPos(), getYPos() + 1));        
    }
    public void setAsLineIndicator(HexGrid grid)
    {
        this.grid = grid;
        isLineIndicator = true;
    }


    public HexGrid.PosInGrid findSpawnPos(Vector3 shotCoords)
    {
        Vector3 angleVector = (this.transform.position - shotCoords).normalized;
        if (isHitOnLeftSide(pos, angleVector) || isByRightWall(pos)) return findInLeftSide(angleVector);
        else return findInRightSide(angleVector);
    }

    private HexGrid.PosInGrid findInRightSide(Vector3 angleVector)
    {
        if (isHitTop(angleVector) && isBelowFirstRow(pos) && !grid.isBubbleExist(grid.topRightNeighbor(pos)))
            return grid.topRightNeighbor(pos);
        else if (isHitMid(angleVector) && !grid.isBubbleExist(grid.midRightNeighbor(pos)))
            return grid.midRightNeighbor(pos);
        else return grid.botRightNeighbor(pos);
    }

    private HexGrid.PosInGrid findInLeftSide(Vector3 angleVector)
    {
        if (isHitTop(angleVector) && isBelowFirstRow(pos) && !grid.isBubbleExist(grid.topLeftNeighbor(pos)))
            return grid.topLeftNeighbor(pos);
        else if (isHitMid(angleVector) && !grid.isBubbleExist(grid.midLeftNeighbor(pos)))
            return grid.midLeftNeighbor(pos);
        else return grid.botLeftNeighbor(pos);
    }
    private static bool isHitMid(Vector3 angleVector)
    {
        return angleVector.y <= 0.33;
    }

    private static bool isHitTop(Vector3 angleVector)
    {
        return angleVector.y <= -0.33;
    }

    private bool isHitOnLeftSide(HexGrid.PosInGrid bubbleBeenHitPos, Vector3 angleVector)
    {
        return (angleVector.x >= 0 && !isByLeftWall(bubbleBeenHitPos));
    }

    private bool isByRightWall(HexGrid.PosInGrid bubbleBeenHitPos)
    {
        return grid.isLineLong(bubbleBeenHitPos.y) && bubbleBeenHitPos.x == grid.objectsPerRow - 1;
    }

    private bool isByLeftWall(HexGrid.PosInGrid bubbleBeenHitPos)
    {
        return grid.isLineLong(bubbleBeenHitPos.y) && bubbleBeenHitPos.x == 0;
    }

    private static bool isBelowFirstRow(HexGrid.PosInGrid bubbleBeenHitPos)
    {
        return bubbleBeenHitPos.y > 0;
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
