import os

"""
Appends metadata info to all blobs in the path specified in datafilepath
"""

file_path = "../Data\inputdata\Canyon2x64-Gen3/"

def run(xdim, ydim, zdim):
    files = os.listdir(file_path)
    for f in files:
        generate_metadata(f, xdim, ydim, zdim)

def generate_metadata(filename, xdim, ydim, zdim):
    f = open(file_path + filename + ".meta", "w+")
    f.write(str(xdim)+ "," + str(ydim) + "," + str(zdim))
    f.close()


run(128,24,128)