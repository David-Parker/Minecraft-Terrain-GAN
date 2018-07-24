import os

"""
Appends metadata info to all blobs in the path specified in datafilepath
"""

file_path = "somepath/"

def run(xdim, ydim, zdim):
    files = os.listdir(file_path)
    for f in files:
        generate_metadata(f, xdim, ydim, zdim)

def generate_metadata(filename, xdim, ydim, zdim):
    f = open(file_path + filename + ".meta", "w+")
    f.write(str(xdim)+ "," + str(ydim) + "," + str(zdim))
    f.close()


run(256,16,256)