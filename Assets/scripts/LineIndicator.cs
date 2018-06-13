using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineIndicator : MonoBehaviour {
    private HexGrid grid;
    
    public void setGrid(HexGrid grid)
    {
        this.grid = grid;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "topCollider")
        {
            grid.SendMessage("stopGridDrop");
            Destroy(this.gameObject);
        }
    }
}