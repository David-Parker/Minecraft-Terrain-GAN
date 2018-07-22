using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
	public IEnumerable<Mesh> Meshes {
		get
		{
			if (meshes == null)
			{
				throw new System.InvalidOperationException("You must create the meshes first.");
			}

			return meshes;
		}

		private set {}
	}

	public Vector3 worldPosition;

	private enum Cubeside {BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK};
	public GameObject parent;
	private Vector3 localPosition;
	private List<Mesh> meshes;

	private int x
	{
		get
		{
			return (int)this.localPosition.x;
		}
	}

	private int y 
	{
		get
		{
			return (int)this.localPosition.y;
		}
	}

	private int z 
	{
		get
		{
			return (int)this.localPosition.z;
		}
	}

	public Block(Vector3 worldPosition, Vector3 localPosition, GameObject parent)
	{
		if (parent == null)
		{
			throw new System.NullReferenceException("parent");
		}

		this.parent = parent;
		this.worldPosition = worldPosition;
		this.localPosition = localPosition;
	}
	
	public void Create(int[,,] cubeMap)
	{
		this.meshes = new List<Mesh>();
		
		if (cubeMap[x,y,z] == 0)
		{
			return;
		}

		if(!HasSolidNeighbor(cubeMap, x, y, z + 1))
			this.meshes.Add(CreateQuad(Cubeside.FRONT));
		if(!HasSolidNeighbor(cubeMap, x, y, z - 1))
			this.meshes.Add(CreateQuad(Cubeside.BACK));
		if(!HasSolidNeighbor(cubeMap, x, y + 1, z))
			this.meshes.Add(CreateQuad(Cubeside.TOP));
		if(!HasSolidNeighbor(cubeMap, x, y - 1, z))
			this.meshes.Add(CreateQuad(Cubeside.BOTTOM));
		if(!HasSolidNeighbor(cubeMap, x - 1, y, z))
			this.meshes.Add(CreateQuad(Cubeside.LEFT));
		if(!HasSolidNeighbor(cubeMap, x + 1, y, z))
			this.meshes.Add(CreateQuad(Cubeside.RIGHT));
	}

	private bool HasSolidNeighbor(int[,,] cubeMap, int x, int y, int z)
	{
		if ((x >= cubeMap.GetLength(0) || x < 0) ||
			(y >= cubeMap.GetLength(1) || y < 0) ||
			(z >= cubeMap.GetLength(2) || z < 0))
			{
				return false;
			}

			return cubeMap[x,y,z] > 0;
	}

	private Mesh CreateQuad(Cubeside side)
	{
		Mesh mesh = new Mesh();

		Vector3[] vertices;
		Vector3[] normals;
		Vector2[] uvs;
		int[] triangles;

		Vector2 uv00 = new Vector2( 0f, 0f );
		Vector2 uv10 = new Vector2( 1f, 0f );
		Vector2 uv01 = new Vector2( 0f, 1f );
		Vector2 uv11 = new Vector2( 1f, 1f );

		//all possible vertices 
		Vector3 p0 = new Vector3( -0.5f,  -0.5f,  0.5f );
		Vector3 p1 = new Vector3(  0.5f,  -0.5f,  0.5f );
		Vector3 p2 = new Vector3(  0.5f,  -0.5f, -0.5f );
		Vector3 p3 = new Vector3( -0.5f,  -0.5f, -0.5f );		 
		Vector3 p4 = new Vector3( -0.5f,   0.5f,  0.5f );
		Vector3 p5 = new Vector3(  0.5f,   0.5f,  0.5f );
		Vector3 p6 = new Vector3(  0.5f,   0.5f, -0.5f );
		Vector3 p7 = new Vector3( -0.5f,   0.5f, -0.5f );

		switch(side)
		{
			case Cubeside.BOTTOM:
				vertices = new Vector3[] {p0, p1, p2, p3};
				normals = new Vector3[] {Vector3.down, Vector3.down, 
											Vector3.down, Vector3.down};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] { 3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.TOP:
				vertices = new Vector3[] {p7, p6, p5, p4};
				normals = new Vector3[] {Vector3.up, Vector3.up, 
											Vector3.up, Vector3.up};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.LEFT:
				vertices = new Vector3[] {p7, p4, p0, p3};
				normals = new Vector3[] {Vector3.left, Vector3.left, 
											Vector3.left, Vector3.left};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.RIGHT:
				vertices = new Vector3[] {p5, p6, p2, p1};
				normals = new Vector3[] {Vector3.right, Vector3.right, 
											Vector3.right, Vector3.right};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.FRONT:
				vertices = new Vector3[] {p4, p5, p1, p0};
				normals = new Vector3[] {Vector3.forward, Vector3.forward, 
											Vector3.forward, Vector3.forward};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.BACK:
				vertices = new Vector3[] {p6, p7, p3, p2};
				normals = new Vector3[] {Vector3.back, Vector3.back, 
											Vector3.back, Vector3.back};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			default:
				throw new System.Exception();
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		 
		mesh.RecalculateBounds();
		
		return mesh;
	}
}
