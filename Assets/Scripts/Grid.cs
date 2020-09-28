using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int width;
    private int height;
    private int cellSize;
    public GridCell[,] gridCells;

    public Grid(int width, int height, int cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        
        gridCells = new GridCell[width, height];

        for (int x = 0; x < gridCells.GetLength(0); x++)
        {
            for (int z = 0; z < gridCells.GetLength(1); z++)
            {
                gridCells[x, z] = new GridCell(x * cellSize, z * cellSize, cellSize);
            }
        }
    }
}
