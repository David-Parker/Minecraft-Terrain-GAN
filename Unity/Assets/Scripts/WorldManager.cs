using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material material;
	public const int ChunkSize = 20;

	// World dimensions correspond to the total number of blocks that exist in the world.
	public int worldX = 64;
	public int worldY = 64;
	public int worldZ = 64;

	private ChunkManager chunkManager;

	// Use this for initialization
	void Start ()
	{
		this.chunkManager = new ChunkManager();

		int testChunkCount = 50;
		//var chunkData = LidarDataTest.Load16CubeDataSet(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\dummy.txt", testChunkCount);

		int[,,] world = GenerateRandomWorld(); // TODO: Load from data file

		Utils.TripleForLoop(worldX,worldY,worldZ, (x,y,z) => {
			ChunkIndex cIndex = ChunkIndex.ConvertWorldIndexToChunkIndex(new Vector3Int(x,y,z), ChunkSize);
			Chunk chunk;

			if (this.chunkManager.TryGetChunk(cIndex.chunkIndex, out chunk) == false)
			{
				chunk = new Chunk(
					new GameObject(), 
					new Vector3(cIndex.chunkIndex.X*ChunkSize,cIndex.chunkIndex.Y*ChunkSize,cIndex.chunkIndex.Z*ChunkSize), 
					new Vector3Int(ChunkSize,ChunkSize,ChunkSize),
					cIndex.chunkIndex,
					material
				);

				this.chunkManager.AddChunk(chunk);
			}
			
			chunk.AddVoxel(cIndex.blockIndex, world[x,y,z]);
		});

		this.chunkManager.GenerateAllChunks();
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
