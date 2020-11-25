using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerationScript : MonoBehaviour
{
    Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    
    //gridsettings
    public float cellSize;
    public Vector3 gridOffset;
    public int gridSizeX, gridSizeY;
    
    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        MakeDiscreteProceduralGrid();
        UpdateMesh();
    }

    void MakeDiscreteProceduralGrid()
    {
        //set array sizes
        vertices = new Vector3[gridSizeX * gridSizeY * 4];
        triangles = new int[gridSizeX * gridSizeY * 6];
        
        //set tracker integers
        int v = 0;
        int t = 0;

        //set vertex offset
        float vertexOffset = cellSize * 0.5f;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 cellOffset = new Vector3(x * cellSize, 0, y * cellSize);
                
                //populate vertices and triagles arrays
                vertices[v] = new Vector3(    -vertexOffset, Random.value, -vertexOffset) + cellOffset + gridOffset;
                vertices[v + 1] = new Vector3(-vertexOffset, Random.value,  vertexOffset) + cellOffset + gridOffset;
                vertices[v + 2] = new Vector3( vertexOffset, Random.value, -vertexOffset) + cellOffset + gridOffset;
                vertices[v + 3] = new Vector3( vertexOffset, Random.value,  vertexOffset) + cellOffset + gridOffset;

                triangles[t    ] = v;
                triangles[t + 1] = triangles[t + 4] = v + 1;
                triangles[t + 2] = triangles[t + 3] = v + 2;
                triangles[t + 5] = v + 3;

                v += 4;
                t += 6;
            }
        }
        
    }
    
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
