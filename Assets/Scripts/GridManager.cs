using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width;
    public int height;
    public int cellSize;
    
    
    public GridCell[,] _gridCellsGizmo;
    
    void Start()
    {
        Grid grid = new Grid(width, height, cellSize);
        _gridCellsGizmo = grid.gridCells;
        
        Debug.Log(width + "" + height);
    }





    void OnDrawGizmos()
    {
        for (int x = 0; x < _gridCellsGizmo.GetLength(0); x++)
        {
            for (int z = 0; z < _gridCellsGizmo.GetLength(1); z++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(new Vector3(_gridCellsGizmo[x, z].posX + (_gridCellsGizmo[x, z].cellSize * 0.5f),0,_gridCellsGizmo[x, z].posZ+ (_gridCellsGizmo[x, z].cellSize * 0.5f)),
                    new Vector3(_gridCellsGizmo[x, z].cellSize*0.1f,_gridCellsGizmo[x, z].cellSize*0.1f,_gridCellsGizmo[x, z].cellSize*0.1f) );
            }
        }
        
    }
}
