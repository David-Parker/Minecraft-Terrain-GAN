using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// represent the raw Lidar World Data as received from external source
    /// World Data will have attributes describing the size of the world in points in 3 dimensions
    /// world Data will also have flat array of PointData which will need to be folded back into 3D space
    /// tightly coupled encoding of world requires that the array of PointData was constructed where a point
    /// at [X,Y,Z] in 3D space ends up at index [X + (Y * YSize) + (Z * YSize * ZSize)]
    /// </summary>
    public class LidarWorldData
    {
        public int XSize { get; set; }
        public int YSize { get; set; }
        public int ZSize { get; set; }

        /// <summary>
        /// Constructs LidarWorldData given only an array of booleans which indicate whether a point in 3D space
        /// exists or not.
        /// </summary>
        /// <param name="worldDataExistsOnly"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        /// <param name="zSize"></param>
        public LidarWorldData(bool[] pointExistsArray, int xSize, int ySize, int zSize)
        {
            XSize = xSize;
            YSize = ySize;
            ZSize = zSize;
            int expectedWorldDataSize = xSize * ySize * zSize;
            if (expectedWorldDataSize != pointExistsArray.Length)
            {
                var errorString = string.Format("Expected pointExistsArray.Length to be xSize*ySize*zSize or {0}, but found {1} instead", expectedWorldDataSize, pointExistsArray.Length);
                throw new ArgumentOutOfRangeException("pointExistsArray.Length", errorString);
            }
            RawPoints = new LidarPointData[xSize * ySize * zSize];
            // @todo awoods: ParallelFor?
            for (int i = 0; i < expectedWorldDataSize; i++)
            {
                RawPoints[i] = new LidarPointData(){ Exists = pointExistsArray[i]};
            }
        }

        public LidarPointData[] RawPoints { get; set; }

        public LidarPointData GetPointData(Vector3Int pointOffset)
        {
            return RawPoints[pointOffset.X + (pointOffset.Y * ZSize) + (pointOffset.Z * ZSize * YSize)];
        }
    }

    public class LidarPointData
    {
        public bool Exists { get; set; }
    }
}
