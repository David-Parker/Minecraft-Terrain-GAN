using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material material;
	public const int ChunkSize = 16;

	// Use this for initialization
	void Start ()
	{
		int testChunkCount = 50;
		var chunkData = LidarDataTest.Load16CubeDataSet(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\dummy.txt", testChunkCount);
		for (int chunkIndex = 0; chunkIndex < testChunkCount; chunkIndex++)
		{
			Chunk chunk = new Chunk(new GameObject(), new Vector3(chunkIndex * ChunkSize,0,0), new Vector3(ChunkSize,ChunkSize,ChunkSize), material);
			chunk.BuildChunk(GetVoxelsFromChunk(chunkData[chunkIndex]));
		}

		// chunk.BuildChunk(GetTestLidarData());

		// Utils.TripleForLoop(4,4,4, (x,y,z) => {
		// 	Chunk chunk = new Chunk(new GameObject(), new Vector3(x*ChunkSize,y*ChunkSize,z*ChunkSize), new Vector3(ChunkSize,ChunkSize,ChunkSize), material);
		// 	chunk.BuildChunk(GetRandomVoxels());
		// });
	}

	private int[,,] GetVoxelsFromChunk(LidarWorldData chunkData)
	{
		// var chunkData = LidarDataTest.Load16CubeDataSet(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\dummy.txt", 50);
		int[,,] voxels = new int[ChunkSize,ChunkSize,ChunkSize];
		for (int xOffset = 0; xOffset < ChunkSize; xOffset++)
		{
			for (int yOffset = 0; yOffset < ChunkSize; yOffset++)
			{
				for (int zOffset = 0; zOffset < ChunkSize; zOffset++)
				{
					voxels[xOffset, yOffset, zOffset] = chunkData.GetPointData(new Vector3Int(xOffset, yOffset, zOffset)).Exists ? 1 : 0;
				}
			}
		}
		return voxels;
	}

	private int[,,] GetRandomVoxels()
	{
		int[,,] voxels = new int[ChunkSize,ChunkSize,ChunkSize];

		Utils.TripleForLoop(ChunkSize,ChunkSize,ChunkSize, (x,y,z) => {
			voxels[x,y,z] = Random.Range(0, 2);
		});

		return voxels;
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
}
