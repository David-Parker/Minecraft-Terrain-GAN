using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material material;
	public const int ChunkSize = 16;
	public const int worldSize = 16;

	// Use this for initialization
	void Start ()
	{
		int[,,] world = GenerateRandomWorld();
		Vector3Int currentChunk = new Vector3Int(-1,-1,-1);

		Utils.TripleForLoop(worldSize,worldSize,worldSize, (x,y,z) => {
			ChunkIndex cIndex = ChunkIndex.ConvertWorldIndexToChunkIndex(new Vector3Int(x,y,z), ChunkSize);

			if (!cIndex.chunkIndex.Equals(currentChunk))
			{
				Chunk chunk = new Chunk(new GameObject(), new Vector3(cIndex.chunkIndex.X*ChunkSize,cIndex.chunkIndex.Y*ChunkSize,cIndex.chunkIndex.Z*ChunkSize), new Vector3(ChunkSize,ChunkSize,ChunkSize), material);
				chunk.BuildChunk(GetRandomVoxels());
				currentChunk = cIndex.chunkIndex;
			}
		});
	}

	private int[,,] GetRandomVoxels()
	{
		int[,,] voxels = new int[ChunkSize,ChunkSize,ChunkSize];

		Utils.TripleForLoop(ChunkSize,ChunkSize,ChunkSize, (x,y,z) => {
			voxels[x,y,z] = Random.Range(0, 2);
		});

		return voxels;
	}

	private int[,,] GenerateRandomWorld()
	{
		int[,,] voxels = new int[worldSize,worldSize,worldSize];

		Utils.TripleForLoop(worldSize,worldSize,worldSize, (x,y,z) => {
			voxels[x,y,z] = Random.Range(0, 2);
		});

		return voxels;
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
}
