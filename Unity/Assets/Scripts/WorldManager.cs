using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material material;
	public const int ChunkSize = 16;

	public const int worldX = 64;
	public const int worldY = 64;
	public const int worldZ = 64;

	private Dictionary<string, Tuple<Chunk, int[,,]>> chunks;

	// Use this for initialization
	void Start ()
	{
		int testChunkCount = 50;
		//var chunkData = LidarDataTest.Load16CubeDataSet(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\dummy.txt", testChunkCount);

		// for (int chunkIndex = 0; chunkIndex < testChunkCount; chunkIndex++)
		// {
		// 	Chunk chunk = new Chunk(new GameObject(), new Vector3(chunkIndex * ChunkSize,0,0), new Vector3(ChunkSize,ChunkSize,ChunkSize), material);
		// 	chunk.BuildChunk(GetVoxelsFromChunk(chunkData[chunkIndex]));
		// }

		chunks = new Dictionary<string, Tuple<Chunk, int[,,]>>();
		int[,,] world = GenerateRandomWorld();

		Utils.TripleForLoop(worldX,worldY,worldZ, (x,y,z) => {
			ChunkIndex cIndex = ChunkIndex.ConvertWorldIndexToChunkIndex(new Vector3Int(x,y,z), ChunkSize);
			string chunkAsString = CreateStringRepresentation(cIndex.chunkIndex);
			Tuple<Chunk, int[,,]> data = null;

			if (chunks.ContainsKey(chunkAsString) == false)
			{
				Chunk chunk = new Chunk(new GameObject(), new Vector3(cIndex.chunkIndex.X*ChunkSize,cIndex.chunkIndex.Y*ChunkSize,cIndex.chunkIndex.Z*ChunkSize), new Vector3(ChunkSize,ChunkSize,ChunkSize), material, ChunkSize);

				data = new Tuple<Chunk, int[,,]>(chunk, new int[ChunkSize, ChunkSize, ChunkSize]);
				
				chunks.Add(chunkAsString, data);
			}
			else
			{
				data = chunks[chunkAsString];
			}

			data.Second[cIndex.blockIndex.X, cIndex.blockIndex.Y, cIndex.blockIndex.Z] = world[x,y,z];
		});

		foreach (var pair in chunks)
		{
			pair.Value.First.BuildChunk(pair.Value.Second);
		}
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

	private int[,,] GenerateRandomWorld()
	{
		int[,,] voxels = new int[worldX,worldY,worldZ];

		Utils.TripleForLoop(worldX,worldY,worldZ, (x,y,z) => {
			voxels[x,y,z] = Random.Range(0, 2);
		});

		return voxels;
	}

	private static string CreateStringRepresentation(Vector3Int pos)
	{
		return "" + pos.X + "_" + pos.Y + "_" + pos.Z;
	}

	// Update is called once per frame
	void Update ()
	{
	}
}
