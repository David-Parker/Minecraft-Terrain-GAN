using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material material;
	public const int ChunkSize = 16;

	// Use this for initialization
	void Start ()
	{
		int[,,] world = GenerateRandomWorld();
		Vector3Int currentChunk = new Vector3Int(-1,-1,-1);

		Utils.TripleForLoop(16,16,16, (x,y,z) => {
			ChunkIndex cIndex = ChunkIndex.ConvertWorldIndexToChunkIndex(new Vector3Int(x,y,z), ChunkSize);

			if (cIndex.chunkIndex != currentChunk)
			{
				Chunk chunk = new Chunk(new GameObject(), new Vector3(cIndex.chunkIndex.X*ChunkSize,cIndex.chunkIndex.Y*ChunkSize,cIndex.chunkIndex.Z*ChunkSize), new Vector3(ChunkSize,ChunkSize,ChunkSize), material);
				chunk.BuildChunk(GetRandomVoxels());
				currentChunk = cIndex.chunkIndex;
			}
		});

		// Utils.TripleForLoop(4,4,4, (x,y,z) => {
		// 	Chunk chunk = new Chunk(new GameObject(), new Vector3(x*ChunkSize,y*ChunkSize,z*ChunkSize), new Vector3(ChunkSize,ChunkSize,ChunkSize), material);
		// 	chunk.BuildChunk(GetRandomVoxels());
		// });
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
		int[,,] voxels = new int[16,16,16];

		Utils.TripleForLoop(16,16,16, (x,y,z) => {
			voxels[x,y,z] = Random.Range(0, 2);
		});

		return voxels;
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
}
