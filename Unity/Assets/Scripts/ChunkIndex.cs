using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Converts a global index into a chunk index + block index.
public class ChunkIndex
{
    private ChunkIndex() {}

    public Vector3Int chunkIndex;
    public Vector3Int blockIndex;

    public static ChunkIndex ConvertWorldIndexToChunkIndex(Vector3Int worldIndex, int chunkSize)
    {
        if (worldIndex.X < 0 || worldIndex.Y < 0 || worldIndex.Z < 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        int chunkIndexX = worldIndex.X / chunkSize;
        int chunkIndexY = worldIndex.Y / chunkSize;
        int chunkIndexZ = worldIndex.Z / chunkSize;

        int blockIndexX = worldIndex.X % chunkSize;
        int blockIndexY = worldIndex.Y % chunkSize;
        int blockIndexZ = worldIndex.Z % chunkSize;

        Vector3Int cIndex = new Vector3Int(chunkIndexX, chunkIndexY, chunkIndexZ);
        Vector3Int bIndex = new Vector3Int(blockIndexX, blockIndexY, blockIndexZ);
        ChunkIndex result = new ChunkIndex();

        result.chunkIndex = cIndex;
        result.blockIndex = bIndex;

        return result;
    }
}