﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using UnityEditor.UI;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubes : MonoBehaviour
{
	public bool smoothTerrain;
	public bool flatShaded;
	
	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	
	MeshFilter meshFilter;
	private MeshCollider meshCollider;
	private Mesh mesh;

	private float terrainSurface = 0.5f;
	public int width = 32;
	public int height = 8;
	private float[,,] terrainMap;
	
	private void Start()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		transform.tag = "Terrain";
		terrainMap = new float[width + 1, height + 1, width + 1];
		
		mesh = new Mesh();
		
		PopulateTerrainMap2D();
		//PopulateTerrainMap3D();
		
		CreateMeshData();
	}
	
	void PopulateTerrainMap2D()
	{
		for (int x = 0; x < width + 1; x++)
		{
			for (int y = 0; y < height + 1; y++)
			{
				for (int z = 0; z < width + 1; z++)
				{
					float thisHeight;
					/*if (x > 10 && x < 22 && z > 5 && z < 22)
						thisHeight = 1f;
					else*/
						thisHeight = (float) height *
						             Mathf.PerlinNoise((float) (x) / 16f * 1.5f + .001f,
							             (float) (z) / 16f * 1.5f + .001f);

					terrainMap[x, y, z] = (float) y - thisHeight;
				}
			}
		}
	}
	
	void PopulateTerrainMap3D() // Refactoring PopulateTerrainMap to work with 3D mapping and noise.
	{
		// TODO: Implement different layers of Noise (Create more realistic caves).
		
		int perlinX = Random.Range(0, 100); // randomly pick a position in the 3D noise map.
		int perlinY = Random.Range(0, 100); //
		int perlinZ = Random.Range(0, 100); //
		
		Debug.Log(perlinX);
		
		for (int x = 0; x < width + 1; x++)
		{
			for (int y = 0; y < height + 1; y++)
			{
				for (int z = 0; z < width + 1; z++)
				{
					float perlinValue;
					
					// Modify later for chunks
					if (x == 0 || z == 0 || y == 0 || x == width || z == width || y == height)  
						perlinValue = 1f;
					else
						perlinValue = PerlinNoise3D.Perlin3D((float) (x + perlinX) / 16f * 1.5f + .001f,
						(float) (y + perlinY)/ 16f * 1.5f + .001f, (float) (z + perlinZ)/ 16f * 1.5f + .001f);
					
					terrainMap[x, y, z] = perlinValue;
				}
			}
		}
	}

	void CreateMeshData()
	{
		ClearMeshData();
		
		// Loop through each cube in the terrain
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int z = 0; z < width; z++)
				{
					
					
					// Pass the value into MarchCube function.
					MarchCube(new Vector3Int(x,y,z));
				}
			}
		}
		
		BuildMesh();
	}

	int GetCubeConfiguration(float[] cube)
	{
		int configurationIndex = 0;
		for (int i = 0; i < 8; i++)
		{
			if (cube[i] > terrainSurface)
				configurationIndex |= 1 << i;
		}

		return configurationIndex;
	}

	public void PlaceTerrain(Vector3 pos)
	{
		Vector3Int v3Int = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z));
		terrainMap[v3Int.x, v3Int.y, v3Int.z] = 0f;
		CreateMeshData();
	}
	
	public void RemoveTerrain(Vector3 pos)
	{
		Vector3Int v3Int = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		terrainMap[v3Int.x, v3Int.y, v3Int.z] = 1f;
		CreateMeshData();
	}
	
	float SampleTerrain(Vector3Int point)
	{
		return terrainMap[point.x, point.y, point.z];
	}

	int VertForIndice(Vector3 vert)
	{
		// Loop through all the verts currently int the vert List
		for (int i = 0; i < vertices.Count; i++)
		{
			// If find a vert that matches, simply return this index
			if(vertices[i] == vert)
				return i;
		}
		
		//if vert is not found add to List.
		vertices.Add(vert);
		return vertices.Count - 1;
	}
	
	void MarchCube(Vector3Int position)
	{
		// Create array of floats representing each corner of a cube.
		float[] cube = new float[8];
		for (int i = 0; i < 8; i++)
		{
			cube[i] = SampleTerrain(position + GameData.CornerTable[i]);
		}
		
		int configIndex = GetCubeConfiguration(cube);
		
		if (configIndex == 0 || configIndex == 255) return;

		int edgeIndex = 0;
		for (int i = 0; i < 5; i++)
		{
			for (int p = 0; p < 3; p++)
			{
				int indice = GameData.TriangleTable[configIndex, edgeIndex];

				if (indice == -1) return;
				
				// Get verts for start and end of edge
				Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];

				Vector3 vertPosition;
				
				// Calculations to get smoother terrain.
				if (smoothTerrain)
				{
					// Get the terrain values at either end of our edge.
					float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
					float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];
					
					// Calculate the difference between the terrain values.
					float difference = vert2Sample - vert1Sample;
					
					// If difference is 0, then terrain passes through surface.
					if (difference == 0)
						difference = terrainSurface;
					else
						difference = (terrainSurface - vert1Sample) / difference;
					
					// Calculate the point along the edge that the terrain passes through.
					vertPosition = vert1 + ((vert2 - vert1) * difference);
				}
				else
				{
					// Get midpoint of edge
					vertPosition = (vert1 + vert2) * 0.5f;
				}
				
				if (flatShaded)
				{
					// Add to vert and triangles List and increment edgeIndex
					vertices.Add(vertPosition);
					triangles.Add(vertices.Count - 1);
				}
				else
					triangles.Add(VertForIndice(vertPosition));

				edgeIndex++;
			}
		}
	}
	
	void ClearMeshData()
	{
		vertices.Clear();
		triangles.Clear();
	}

	void BuildMesh()
	{
		mesh.Clear();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;
	}
}
