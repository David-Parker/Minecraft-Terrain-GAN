using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	public Material material;
	public const int ChunkSize = 16;

	public const int worldX = 17;
	public const int worldY = 42;
	public const int worldZ = 25;

	private Dictionary<string, Tuple<Chunk, int[,,]>> chunks;

	// Use this for initialization
	void Start ()
	{
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
