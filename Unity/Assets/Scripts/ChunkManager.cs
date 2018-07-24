using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

// Manages the state and position of chunks.
public class ChunkManager
{
	// Dictionary builds a mapping between the chunk index represented as a string, i.e 0_0_0 and 0_0_1
	// And the corresponding chunk object.
	private Dictionary<string, Chunk> chunks;

    public ChunkManager()
    {
        this.chunks = new Dictionary<string, Chunk>();
    }

    public bool TryGetChunk(Vector3Int chunkIndex, out Chunk chunk)
    {
        string chunkIndexAsString = CreateStringRepresentation(chunkIndex);

        if (chunks.ContainsKey(chunkIndexAsString) == false)
        {
            chunk = null;
            return false;
        }
        else
        {
            chunk = chunks[chunkIndexAsString];
            return true;
        }
    }

    public void AddChunk(Chunk chunk)
    {
        if (chunk == null)
        {
            throw new System.ArgumentNullException("chunk");
        }

        string chunkIndexAsString = CreateStringRepresentation(chunk.ChunkIndex);

        if (chunks.ContainsKey(chunkIndexAsString) == true)
        {
            throw new System.InvalidOperationException("Chunk already exists!");
        }

        this.chunks[chunkIndexAsString] = chunk;
    }

    public void GenerateAllChunks()
    {
        foreach (var pair in chunks)
		{
			pair.Value.BuildChunk();
		}
    }

    private static string CreateStringRepresentation(Vector3Int pos)
	{
		return "" + pos.X + "_" + pos.Y + "_" + pos.Z;
	}
}