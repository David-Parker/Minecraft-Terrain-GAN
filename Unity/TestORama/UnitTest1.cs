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
        public void CreateEllipticParaboloid()
        {
            int chunkDimension = 32;
            string filename = string.Format(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\Test\ellipty-hilly-{0}.txt", DateTime.Now.ToString("ddHHmm"));
            Random rnd = new Random();
            var worldPoints = new int[chunkDimension, chunkDimension, chunkDimension];

            for (int xOffset = -chunkDimension / 2; xOffset < chunkDimension / 2; xOffset++)
            {
                for (int zOffset = -chunkDimension / 2; zOffset < chunkDimension / 2; zOffset++)
                {
                    int yHeight = (-(xOffset * xOffset * 2) - (zOffset * zOffset) + 100) / -3;
                    for (int yOffset = 0; yOffset < chunkDimension; yOffset++)
                    {
                        worldPoints[xOffset + chunkDimension / 2, yOffset, zOffset + chunkDimension / 2] = yOffset <= yHeight ? 1 : 0;
                    }
                }
            }
            OutputFile(chunkDimension, worldPoints, filename);
        }

        [TestMethod]
        public void CreateHillsIn16CubeSpace()
        {
            int chunkDimension = 16;
            string filename = string.Format(@"C:\Src\Hackathon2018\Minecraft-Terrain-GAN\Data\Test\hilly-willy-{0}.txt", DateTime.Now.ToString("ddHHmm"));
            Random rnd = new Random();

            for (int chunkIndex = 0; chunkIndex < 100; chunkIndex++)
            {
                int peakHeight = rnd.Next(8, 15);
                int xPeakOffset = rnd.Next(1, chunkDimension - 2);
                int zPeakOffset = rnd.Next(1, chunkDimension - 2);
                var worldPoints = new int[chunkDimension, chunkDimension, chunkDimension];
                for (int xOffset = 0; xOffset < chunkDimension; xOffset++)
                {
                    for (int zOffset = 0; zOffset < chunkDimension; zOffset++)
                    {
                        // height of any point on topology is base (1) + peakHeight - distance from peak offset
                        int yHeight = 1 + peakHeight - Math.Min(peakHeight, Math.Max(Math.Abs(zOffset - zPeakOffset), Math.Abs(xOffset - xPeakOffset)));
                        for (int yOffset = 0; yOffset < chunkDimension; yOffset++)
                        {
                            worldPoints[xOffset, yOffset, zOffset] = yOffset <= yHeight ? 1 : 0;
                        }
                    }
                }
                //int[] worldPointsFlatArray = new int[chunkDimension * chunkDimension * chunkDimension];
                StringBuilder sbWorldPoints = new StringBuilder();
                for (int xOffset = 0; xOffset < chunkDimension; xOffset++)
                {
                    for (int yOffset = 0; yOffset < chunkDimension; yOffset++)
                    {
                        for (int zOffset = 0; zOffset < chunkDimension; zOffset++)
                        {
                            sbWorldPoints.Append(worldPoints[xOffset, yOffset, zOffset] + ",");
                        }
                    }
                }
                sbWorldPoints.Remove((chunkDimension * chunkDimension * chunkDimension * 2) - 1, 1);
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
        private void OutputFile(int chunkDimension, int[,,] worldPoints, string filename)
        {
            StringBuilder sbWorldPoints = new StringBuilder();
            for (int xOffset = 0; xOffset < chunkDimension; xOffset++)
            {
                for (int yOffset = 0; yOffset < chunkDimension; yOffset++)
                {
                    for (int zOffset = 0; zOffset < chunkDimension; zOffset++)
                    {
                        sbWorldPoints.Append(worldPoints[xOffset, yOffset, zOffset] + ",");
                    }
                }
            }
            sbWorldPoints.Remove((chunkDimension * chunkDimension * chunkDimension * 2) - 1, 1);
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
