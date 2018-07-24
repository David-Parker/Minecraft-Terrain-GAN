# -*- coding: utf-8 -*-
"""
Main Engine for TerrainGAN
"""
import argparse

import os

from dataloader import TerrainDataLoader
from terraingan import TerrainGAN

if __name__ == '__main__':
    parser = argparse.ArgumentParser()

    parser.add_argument('--input_shape', action='store', type=tuple, default=(16,16,16), help='Shape of the input data.')
    parser.add_argument('--batch_size', action='store', type=int, default=32, help='batch size for data loading')
    parser.add_argument('--directory', action='store', type=str, default='../Data/FinalData/BaseLine', help='Directory from where to load data')
    parser.add_argument('--results_save_path', action='store', type=str, default='results/', help='Directory to save results to')
    parser.add_argument('--load_dir', action='store', type=str, default=None, help='Where to load weights from')
    parser.add_argument('--save_interval', action='store', type=int, default=10, help='Period to save results')
    parser.add_argument('--gradient_norm', action='store_true', help='Whether or not to print the gradient norms')
    parser.add_argument('--label_smoothing', action='store_true', help='Whether or not to apply label smoothing. NOTE that this ruins the accuracy metric.')
    args = parser.parse_args()

    batch_size = args.batch_size
    directory = os.path.realpath(args.directory)
    results_save_path = os.path.realpath(args.results_save_path)

    input_shape = args.input_shape

    if len(input_shape) != 3:
        raise ValueError(f'Expected an input shape of 3D, instead got {input_shape}')

    datagenerator = TerrainDataLoader(directory, batch_size=batch_size, input_shape=input_shape)
    gan = TerrainGAN(results_save_path)

    if args.load_dir:
        load_dir = os.path.realpath(args.load_dir)
        print (f"Loading weights from {load_dir}")
        gan.load_from_dir(load_dir)

    gan.train(datagenerator, epochs=4000, save_interval=10, label_smoothing=args.label_smoothing, gradient_norm=args.gradient_norm)

