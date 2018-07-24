using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material material;
	public const int ChunkSize = 24;

	// World dimensions correspond to the total number of blocks that exist in the world.
	public int worldX = 16;
	public int worldY = 16;
	public int worldZ = 16;

	private ChunkManager chunkManager;

	// Use this for initialization
	void Start ()
	{
		this.chunkManager = new ChunkManager();

		int testChunkCount = 1;
		var chunkData = LidarDataTest.Load16CubeDataSet(@"..\Data\FinalData\0B5EE8367C1FBDAE2D831F180F031A2A", testChunkCount, new Vector3Int(worldX, worldY, worldZ));

		int[,,] world = GetVoxelsFromChunk(chunkData[0]);

		Utils.TripleForLoop(worldX,worldY,worldZ, (x,y,z) => {
			ChunkIndex cIndex = ChunkIndex.ConvertWorldIndexToChunkIndex(new Vector3Int(x,y,z), ChunkSize);
			Chunk chunk;

			if (this.chunkManager.TryGetChunk(cIndex.chunkIndex, out chunk) == false)
			{
				chunk = new Chunk(
					new GameObject(CreateStringRepresentation(cIndex.chunkIndex)), 
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
		int[,,] voxels = new int[worldX,worldY,worldZ];
		for (int xOffset = 0; xOffset < worldX; xOffset++)
		{
			for (int yOffset = 0; yOffset < worldY; yOffset++)
			{
				for (int zOffset = 0; zOffset < worldZ; zOffset++)
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
