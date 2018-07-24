using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class LidarDataTest
    {
        public static LidarWorldData[] LoadCubeFile(string filepath, int cubeCount, Vector3Int worldDimensions)
        {
            string[] cubeLines = System.IO.File.ReadAllLines(filepath);

            var result = new LidarWorldData[cubeCount];
            for (int cubeIndex = 0; cubeIndex < cubeCount; cubeIndex++)
            {
                bool[] dataPoints = cubeLines[cubeIndex].Split(new[]{','}).Select(x => x.Equals("1") ? true : false).ToArray();
                result[cubeIndex] = new LidarWorldData(dataPoints, worldDimensions.X, worldDimensions.Y, worldDimensions.Z);
            }
            // string[] cubeLines = System.IO.File.ReadAllLines(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\dummy.txt");
            return result;
        }

        public static Vector3Int ParseDimensions(string filepath)
        {
            string[] lines = System.IO.File.ReadAllLines(filepath);

            string dim = lines[0];

            var data = dim.Split(',').Select(x => Int32.Parse(x)).ToList();

            return new Vector3Int(data[0], data[1], data[2]);
        }
    }
}
