import numpy as np
import pandas as pd


def readFileToArr(fileName):
    arr = []
    with open(fileName, 'r') as pFile:
        arr.append(pFile.readline().split(","))

        for line in pFile:
            tempLine = line.split(",")
            finalLine = []
            for entry in tempLine:
                finalLine.append((float)(entry))
            arr.append(finalLine)
    return arr


pArray = readFileToArr('points.txt')
df = pd.DataFrame(pArray[1:], columns=pArray[0])

df[['x', 'y', 'z']] = np.round(df[['x', 'y', 'z']])
df[['x', 'y', 'z']] -= np.min(df[['x', 'y', 'z']], axis=0).astype(np.int)
df = df.drop_duplicates(['x', 'y'])

df.to_csv("lowerRes.txt", index=False, sep=',')
