using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace TestORama
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ConvertSurfaceMapToSolid()
        {
            string inputDataPath = @"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\inputdata\Canyon2x64";
            string outputDataPath = @"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\inputdata\Canyon2x64-Solid";

            for (int fileIndex = 0; fileIndex < 448; fileIndex++)
            {
                string filePath = Path.Combine(inputDataPath, fileIndex.ToString());
                Vector3Int inputDataDimensions = new Vector3Int(64, 24, 64);

                var worldPoints = new int[inputDataDimensions.X, inputDataDimensions.Y, inputDataDimensions.Z];

                var surfaceMapData = LidarDataTest.LoadCubeFile(filePath, 1, inputDataDimensions);
                for (int xOffset = 0; xOffset < inputDataDimensions.X; xOffset++)
                {
                    for (int zOffset = 0; zOffset < inputDataDimensions.Z; zOffset++)
                    {
                        bool solidFill = false;
                        for (int yOffset = inputDataDimensions.Y - 1; yOffset >= 0; yOffset--)
                        {
                            if (solidFill == false)
                            {
                                var surfaceMapPoint = surfaceMapData[0].GetPointData(new Vector3Int(xOffset, yOffset, zOffset));
                                solidFill = surfaceMapPoint.Exists;
                            }
                            worldPoints[xOffset, yOffset, zOffset] = solidFill ? 1 : 0;
                        }
                    }
                }

                OutputFile(inputDataDimensions, worldPoints, Path.Combine(outputDataPath, fileIndex.ToString() + "-solid"));
            }
        }

        [TestMethod]
        public void CreateEllipticParaboloid()
        {
            Vector3Int chunkDimensions = new Vector3Int(32, 32, 32);
            //int chunkDimension = 32;
            string filePath = string.Format(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\Test\ellipty-hilly-{0}", DateTime.Now.ToString("ddHHmm"));
            Directory.CreateDirectory(filePath);

            Random rnd = new Random();
            var worldPoints = new int[chunkDimensions.X, chunkDimensions.Y, chunkDimensions.Z];
            for (int chunkIndex = 0; chunkIndex < 1000; chunkIndex++)
            {
                string filename = string.Format("example{0:D4}.txt", chunkIndex);
                int hillBase = rnd.Next(50, 120);     // initial: 100
                float xCompression = (float)rnd.Next(30, 150) / 100;    // initial 1.5 and .7
                float zCompression = (float)rnd.Next(30, 150) / 100;
                float hillCompression = (float)rnd.Next(40, 80) / 10;       // initial 5

                for (int xOffset = -chunkDimensions.X / 2; xOffset < chunkDimensions.X / 2; xOffset++)
                {
                    for (int zOffset = -chunkDimensions.Z / 2; zOffset < chunkDimensions.Z / 2; zOffset++)
                    {
                        int yHeight = (int)((-(xOffset * xOffset * xCompression) - (zOffset * zOffset * zCompression) + hillBase) / hillCompression);
                        // base of one level
                        worldPoints[xOffset + chunkDimensions.X / 2, 0, zOffset + chunkDimensions.Z / 2] = 1;
                        // now the hill
                        for (int yOffset = 1; yOffset < chunkDimensions.Y; yOffset++)
                        {
                            worldPoints[xOffset + chunkDimensions.X / 2, yOffset, zOffset + chunkDimensions.Z / 2] = yOffset <= yHeight ? 1 : 0;
                        }
                    }
                }
                OutputFile(chunkDimensions, worldPoints, Path.Combine(filePath,filename));
            }
        }

        [TestMethod]
        public void CreateHillsIn16CubeSpace()
        {
            Vector3Int chunkDimensions = new Vector3Int(16, 16, 16);
            //int chunkDimension = 16;
            string filename = string.Format(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\Test\hilly-willy-{0}.txt", DateTime.Now.ToString("ddHHmm"));
            Random rnd = new Random();

            for (int chunkIndex = 0; chunkIndex < 100; chunkIndex++)
            {
                int peakHeight = rnd.Next(8, 15);
                int xPeakOffset = rnd.Next(1, chunkDimensions.X - 2);
                int zPeakOffset = rnd.Next(1, chunkDimensions.Z - 2);
                var worldPoints = new int[chunkDimensions.X, chunkDimensions.Y, chunkDimensions.Z];
                for (int xOffset = 0; xOffset < chunkDimensions.X; xOffset++)
                {
                    for (int zOffset = 0; zOffset < chunkDimensions.Z; zOffset++)
                    {
                        // height of any point on topology is base (1) + peakHeight - distance from peak offset
                        int yHeight = 1 + peakHeight - Math.Min(peakHeight, Math.Max(Math.Abs(zOffset - zPeakOffset), Math.Abs(xOffset - xPeakOffset)));
                        for (int yOffset = 0; yOffset < chunkDimensions.Y; yOffset++)
                        {
                            worldPoints[xOffset, yOffset, zOffset] = yOffset <= yHeight ? 1 : 0;
                        }
                    }
                }
                OutputFile(chunkDimensions, worldPoints, filename);
            }

        }
        private void OutputFile(Vector3Int chunkDimensions /*int chunkDimension*/, int[,,] worldPoints, string filename)
        {
            StringBuilder sbWorldPoints = new StringBuilder();
            for (int xOffset = 0; xOffset < chunkDimensions.X; xOffset++)
            {
                for (int yOffset = 0; yOffset < chunkDimensions.Y; yOffset++)
                {
                    for (int zOffset = 0; zOffset < chunkDimensions.Z; zOffset++)
                    {
                        sbWorldPoints.Append(worldPoints[xOffset, yOffset, zOffset] + ",");
                    }
                }
            }
            sbWorldPoints.Remove((chunkDimensions.X * chunkDimensions.Y * chunkDimensions.Z * 2) - 1, 1);
            if (!File.Exists(filename))
            {
                File.WriteAllText(filename, sbWorldPoints.ToString());
            }
            else
            {
                File.AppendAllText(filename, sbWorldPoints.ToString());
            }
            File.AppendAllText(filename, "\r\n");
        }
    }
}
