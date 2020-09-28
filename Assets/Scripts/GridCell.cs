using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public int posX, posZ, cellSize;
    public bool isDug = false;
    
    public GridCell(int posX, int posZ, int cellSize)
    {
        this.posX = posX;
        this.posZ = posZ;
        this.cellSize = cellSize;
    }
}
