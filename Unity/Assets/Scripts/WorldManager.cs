using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material materialTraining;
	public Material materialGenerated;
	public const int ChunkSize = 20;
	public int fileStart = 0;
	public int fileEnd = 10;

	// Use this for initialization
	void Start ()
	{
		string fileNameTraining = @"..\Data\FinalData\Baseline\";
		string fileNameGenerated = @"..\Machine-Learning\results\generated-10\gen-";

		Vector3 centerTraining = new Vector3(0,0,0);
		Vector3 centerGenerated = new Vector3(0,0,32);

		GenerateWorldFromFile(fileNameTraining, centerTraining, materialTraining);
		GenerateWorldFromFile(fileNameGenerated, centerGenerated, materialGenerated);
	}

	private void GenerateWorldFromFile(string filePath, Vector3 center, Material material)
	{
		for (int i = fileStart; i < fileEnd; i++)
		{
			ChunkManager chunkManager = new ChunkManager();

			int testChunkCount = 1;

			// Get world dimensions from file .meta

			string fileName = filePath + i;
			string metadataFile = fileName + ".meta";

			Vector3Int dimensions = LidarDataTest.ParseDimensions(metadataFile);

			// World dimensions correspond to the total number of blocks that exist in the world.
			int worldX = dimensions.X;
			int worldY = dimensions.Y;
			int worldZ = dimensions.Z;

			var chunkData = LidarDataTest.LoadCubeFile(fileName, testChunkCount, dimensions);

			int[,,] world = GetVoxelsFromChunk(chunkData[0], dimensions);

			Utils.TripleForLoop(worldX,worldY,worldZ, (x,y,z) => {
				ChunkIndex cIndex = ChunkIndex.ConvertWorldIndexToChunkIndex(new Vector3Int(x,y,z), ChunkSize);
				Chunk chunk;

				if (chunkManager.TryGetChunk(cIndex.chunkIndex, out chunk) == false)
				{
					chunk = new Chunk(
						new GameObject(CreateStringRepresentation(cIndex.chunkIndex)), 
						new Vector3(cIndex.chunkIndex.X*ChunkSize,cIndex.chunkIndex.Y*ChunkSize,cIndex.chunkIndex.Z*ChunkSize) + center, 
						new Vector3Int(ChunkSize,ChunkSize,ChunkSize),
						cIndex.chunkIndex,
						material
					);

					chunkManager.AddChunk(chunk);
				}
				
				chunk.AddVoxel(cIndex.blockIndex, world[x,y,z]);
			});

			chunkManager.GenerateAllChunks();

			center += new Vector3(worldX, 0, 0);
		}
	}

	private int[,,] GetVoxelsFromChunk(LidarWorldData chunkData, Vector3Int dimensions)
	{
		int[,,] voxels = new int[dimensions.X,dimensions.Y,dimensions.Z];

		for (int xOffset = 0; xOffset < dimensions.X; xOffset++)
		{
			for (int yOffset = 0; yOffset < dimensions.Y; yOffset++)
			{
				for (int zOffset = 0; zOffset < dimensions.Z; zOffset++)
				{
					voxels[xOffset, yOffset, zOffset] = chunkData.GetPointData(new Vector3Int(xOffset, yOffset, zOffset)).Exists ? 1 : 0;
				}
			}
		}

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
