using System;

public static class Utils
{
    public static void TripleForLoop(int x, int y, int z, Action<int, int, int> body)
    {
        if (x < 0 || y < 0 || z < 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }
        
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    body(i,j,k);
                }
            }
        }
    }
}