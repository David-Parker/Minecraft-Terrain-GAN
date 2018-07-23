using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk
{
	public Material material;
	public Block[,,] chunkData;

	private GameObject parent;
	private Vector3 center;
	private Vector3 dimensions;

	public Chunk(GameObject parent, Vector3 center, Vector3 dimensions, Material material)
	{
		if (parent == null)
		{
			throw new System.ArgumentNullException("parent");
		}

		if (material == null)
		{
			throw new System.ArgumentNullException("material");
		}

		this.parent = parent;
		this.center = center;
		this.dimensions = dimensions;
		this.material = material;
	}

	public void BuildChunk(int[,,] voxels)
	{
		int sizeX = (int)this.dimensions.x;
		int sizeY = (int)this.dimensions.y;
		int sizeZ = (int)this.dimensions.z;

		if (voxels.GetLength(0) != sizeX || voxels.GetLength(1) != sizeY || voxels.GetLength(2) != sizeZ)
		{
			throw new System.ArgumentOutOfRangeException("The dimensions of the voxel grid must match the dimensions of the chunk.");
		}

		chunkData = new Block[sizeX,sizeY,sizeZ];

		Utils.TripleForLoop(sizeX,sizeY,sizeZ, (x,y,z) =>
		{
			Vector3 pos = new Vector3(x,y,z);
			chunkData[x,y,z] = new Block(pos + this.center, pos, this.parent);
			chunkData[x,y,z].Create(voxels);
		});

		CombineQuads();
	}

	// Combines all of the meshes of each individual quad (cube face) into a single mesh.
	// This will greatly increase rendering performance.
	private void CombineQuads()
	{
		int sizeX = (int)this.dimensions.x;
		int sizeY = (int)this.dimensions.y;
		int sizeZ = (int)this.dimensions.z;

		List<Block> blocks = new List<Block>();

		Utils.TripleForLoop(sizeX,sizeY,sizeZ, (x,y,z) =>
		{
			blocks.Add(chunkData[x,y,z]);
		});

		int meshes = blocks.Sum(x => x.Meshes.Count());

        CombineInstance[] combine = new CombineInstance[meshes];

		// Combine all children meshes
		int index = 0;
		foreach (Block block in blocks)
		{
			Vector3 oldPos = block.parent.transform.position;
			block.parent.transform.position = block.worldPosition;
			Matrix4x4 transformMatrix = block.parent.transform.localToWorldMatrix;
			block.parent.transform.position = oldPos;

			foreach (Mesh mesh in block.Meshes)
			{
				combine[index].mesh = mesh;
            	combine[index].transform = transformMatrix;
				index++;
			}
		}

        MeshFilter mf = (MeshFilter) this.parent.AddComponent(typeof(MeshFilter));
        mf.mesh = new Mesh();

        mf.mesh.CombineMeshes(combine);

		MeshRenderer renderer = this.parent.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		renderer.material = material;
	}

	public override int GetHashCode()
	{
		string pos = "" + this.center.x + this.center.y + this.center.z;
		return pos.GetHashCode();
	}

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
}
