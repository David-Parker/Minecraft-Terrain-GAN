using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Represents 3 integers.
public class Vector3Int 
{
    public int X
    {
        get; set;
    }

    public int Y
    {
        get; set;
    }

    public int Z
    {
        get; set;
    }

    public Vector3Int() {}

    public Vector3Int(int X, int Y, int Z)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }

    public Vector3Int(Vector3 vec3)
    {
        this.X = (int)vec3.x;
        this.Y = (int)vec3.y;
        this.Z = (int)vec3.z;
    }
}