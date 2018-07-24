import os
import numpy as np

final_dimx = 16
final_dimy = 16
final_dimz = 16

file_path = "rawdata/"
out_path = "inputdata/"

def run(xdim, ydim, zdim):
    files = os.listdir(file_path)
    for f in files:
        create_file_for_cube(f, xdim, ydim, zdim)


def create_file_for_cube(f, xdim, ydim, zdim):
    cube = generate_empty_cube(xdim, ydim, zdim)

    file_loc = file_path + f
    print("Reading from: " + file_loc)
    file_data = open(file_loc, 'r')

    for line in file_data:
        # Fill in the cube values, swapping the y & z values...
        # index 0 = xvalue, 1 = zvalue, 2 = yvalue, 
        feature_values = line.split(",")
        cube[int(feature_values[0])][int(feature_values[2])][int(feature_values[1])] = 1

        # flatten the cube
        flat_cube = flatten_data(cube)

    #write cube to file
    write_cube_to_file(f, flat_cube)


def generate_empty_cube(xdim, ydim, zdim):
    cube = []

    # Generate empty cube
    for i in range(xdim):
        row_y = []

        for j in range(ydim):
            row_y.append([0]*zdim)

        cube.append(row_y)

    return cube

def write_cube_to_file(file_name, flat_cube):
    outfile_loc = out_path + file_name
    print("Writing input data to: " + outfile_loc)

    f = open(outfile_loc, 'w+')
    f.write(",".join(map(str, flat_cube)))

#Pass in cube 16x16x16 and flattens to an array
def flatten_data(cube):
    npcube = np.array(cube)
    return npcube.flatten('C')


run(final_dimx, final_dimy, final_dimz)


# Given a cube c[i][j][k], reverse the j,k values. 
"""
def reverse_y_z_plane(cube):
    new_cube = copy.deepcopy(cube)
    for i in range(final_dimx):
        for j in range(final_dimy):
            for k in range(final_dimz):
                storedvalue = new_cube[i][j][k]
                new_cube[i][j][k] = new_cube[i][k][j]
                new_cube[i][k][j] = storedvalue

    return new_cube
"""