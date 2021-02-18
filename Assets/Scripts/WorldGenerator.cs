using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int worldSizeInChunks;
    
    Dictionary<Vector3Int, Chunk> _chunks = new Dictionary<Vector3Int, Chunk>();
    
    public bool smooth;
    public bool flatShaded;
    
    void Start()
    {
        //Picking a random starting location within the perlinNoise.
        Vector3Int perlinStartingPosition = new Vector3Int( 
            (int)Random.Range(0, 100), 
            (int)Random.Range(0, 100),
            (int)Random.Range(0, 100));
        
        Generate(perlinStartingPosition, worldSizeInChunks);
    }

    void Generate(Vector3Int perlinStart, int worldInChunks)
    {
        StartCoroutine(CreateChunk(perlinStart, worldInChunks));
        
        Debug.Log(string.Format("{0} x {0} x {0} world generated.", (worldSizeInChunks * GameData.ChunkWidth)));
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = (int) pos.x;
        int y = (int) pos.y;
        int z = (int) pos.z;

        return _chunks[new Vector3Int(x, y, z)];
    }

    IEnumerator CreateChunk(Vector3Int pStart, int wChunks)
    {
        for (int x = 0; x < worldSizeInChunks; x++)
        {
            for (int y = 0; y < worldSizeInChunks; y++)
            {
                for (int z = 0; z < worldSizeInChunks; z++)
                {
                    Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth, y * GameData.ChunkWidth,
                        z * GameData.ChunkWidth);
                    _chunks.Add(chunkPos, new Chunk(chunkPos, pStart, wChunks, smooth, flatShaded));
                    _chunks[chunkPos].chunkObject.transform.SetParent(transform);
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        
    }
}
