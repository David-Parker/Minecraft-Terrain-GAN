import numpy as np
import math

final_dimx = 16
final_dimy = 16
final_dimz = 16

# C-style flatten
def flatten_cube(cube):
    npcube = np.array(cube)
    return npcube.flatten('C')

def gaussian(x, mu, sig):
    return np.exp(-np.power(x - mu, 2.) / (2 * np.power(sig, 2.)))

# returns 16 x 16 x 16, x[i][j][k]
def generate_cube(mu, sig):
    cube = []

    # Generate empty cube
    for i in range(final_dimx):
        row_y = []

        for j in range(final_dimy):
            row_y.append([])

        cube.append(row_y)

    for i in range(final_dimx):
        points = gaussian(np.linspace(-3, 3, final_dimy), mu, sig)
        for j in range(final_dimy):

            # every z value is 0 except for the one specified in the gauss output:
            row_z = [0]*final_dimz

            # Scale to height (since points will be between 0 and 1):
            z_index = min(math.floor(points[j]*final_dimz), final_dimz-1)
            row_z[z_index] = 1
            cube[i][j] = row_z

    return cube


# Generates "number" # of random gaussian rows. 
def generate_rows(number):
    rows = []
    means = np.linspace(0.5, 1, number+1)

    for i in means[:-1]:
        cube = generate_cube(i, 1)
        flattened = flatten_cube(cube)
        rows.append(flatten_cube(cube))

    return rows

# Pass rows to a file
def write_to_file(rows):
    f = open("dummy.txt", 'w+')

    for row in rows:
        f.write(",".join(map(str, row)) + "\n")


def run(number):
    rows = generate_rows(number)
    write_to_file(rows)

run(50)

