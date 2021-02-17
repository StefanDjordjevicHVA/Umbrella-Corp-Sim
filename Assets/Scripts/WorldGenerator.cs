using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int WorldSizeInChunks;
    
    Dictionary<Vector3Int, Chunk> _chunks = new Dictionary<Vector3Int, Chunk>();
    
    public bool smooth;
    public bool flatShaded;
    
    void Start()
    {
        //Chunk chunk = new Chunk(Vector3Int.zero, smooth, flatShaded);
        
        Generate();
    }

    void Generate()
    {
        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth,0, z * GameData.ChunkWidth);
                _chunks.Add(chunkPos, new Chunk(chunkPos, smooth, flatShaded));
                _chunks[chunkPos].chunkObject.transform.SetParent(transform);
            }
        }
        
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * GameData.ChunkWidth)));
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = (int) pos.x;
        int y = (int) pos.y;
        int z = (int) pos.z;

        return _chunks[new Vector3Int(x, y, z)];
    }
}
