using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material materialTraining;
	public Material materialGenerated;
	public const int ChunkSize = 20;
	public int fileStartTraining = 0;
	public int fileEndTraining = 10;
	public int fileStartGenerated = 0;
	public int fileEndGenerated = 10;

	// Use this for initialization
	void Start ()
	{
		StartTrainingRender();
		//StartTestRender();
	}

	void StartTrainingRender()
	{
		string fileNameTraining = @"..\Data\inputdata\Canyon2x64\";
		string fileNameGenerated = @"..\Machine-Learning\Canyon-Final\generated-170\gen-";

		Vector3 centerTraining = new Vector3(0,0,0);
		Vector3 centerGenerated = new Vector3(0,0,96);

		GenerateWorldFromFileSet(fileNameTraining, centerTraining, materialTraining, fileStartTraining, fileEndTraining);
		GenerateWorldFromFileSet(fileNameGenerated, centerGenerated, materialGenerated, fileStartGenerated, fileEndGenerated);
	}

	void StartTestRender()
	{
		//string filePathTest = @"..\Data\Test\ellipty-hilly-241820";
		string filePathTest = @"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Machine-Learning\MCTerrainGen05\results\18-07-25T02-55-51\generated-20";
		//string fileNameTest = @"..\Data\Test\ellipty-hilly-241702.txt";

		Vector3 cubeCenter = new Vector3(0,0,0);
		Vector3Int cubeDimensions = new Vector3Int(32, 32, 32);
		int fileStart = 0;
		int fileEnd = 25;
		int dimensionSquare = (int)Math.Ceiling(Math.Sqrt(fileEnd));


		for (int fileIndex = fileStart; fileIndex < fileEnd; fileIndex++)
		{
			var center = new Vector3(32 * (fileIndex / dimensionSquare), 0, 32 * (fileIndex % dimensionSquare));
			// string fileNameTest = string.Format("example{0:D4}.txt", fileIndex);
			string fileNameTest = string.Format("gen-{0}", fileIndex);

			GenerateWorldFromFile(Path.Combine(filePathTest, fileNameTest), cubeDimensions, materialTraining, center);
		}
	}

	private void GenerateWorldFromFileSet(string filePath, Vector3 center, Material material, int start, int end)
	{
		for (int i = start; i < end; i++)
		{
			string fileName = filePath + i;
			string metadataFile = fileName + ".meta";

			// Get world dimensions from file .meta
			Vector3Int dimensions = LidarDataTest.ParseDimensions(metadataFile);

			GenerateWorldFromFile(fileName, dimensions, material, center);
			center += new Vector3(dimensions.X, 0, 0);
		}
	}

	private void GenerateWorldFromFile(string fileName, Vector3Int dimensions, Material material, Vector3 center)
	{
			ChunkManager chunkManager = new ChunkManager();

			int testChunkCount = 1;

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

			// center += new Vector3(worldX, 0, 0);
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
