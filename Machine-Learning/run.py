# -*- coding: utf-8 -*-
"""
Main Engine for TerrainGAN
"""
import argparse

import os

from datetime import datetime

from dataloader import TerrainDataLoader
from terraingan import TerrainGAN

if __name__ == '__main__':
    parser = argparse.ArgumentParser()

    subparsers = parser.add_subparsers(dest='mode')
    subparsers.required = True

    train_parser = subparsers.add_parser('train')
    train_parser.add_argument('--input_shape', action='store', type=int, nargs='+', default=[64,64,24], help='Shape of the input data.')
    train_parser.add_argument('--batch_size', action='store', type=int, default=32, help='batch size for data loading')
    train_parser.add_argument('--directory', action='store', type=str, default='../Data/FinalData/BaseLine', help='Directory from where to load data')
    train_parser.add_argument('--results_save_path', action='store', type=str, default='results/', help='Directory to save results to')
    train_parser.add_argument('--load_dir', action='store', type=str, default=None, help='Where to load weights from')
    train_parser.add_argument('--save_interval', action='store', type=int, default=10, help='Period to save results')
    train_parser.add_argument('--gradient_norm', action='store_true', help='Whether or not to print the gradient norms')
    train_parser.add_argument('--label_smoothing', action='store_true', help='Whether or not to apply label smoothing. NOTE that this ruins the accuracy metric.')

    gen_parser = subparsers.add_parser('generate')
    gen_parser.add_argument('model_dir', action='store', type=str, help='Where the .h5 for the generator and discriminator exist.')
    gen_parser.add_argument('number', action='store', type=int, default=25, help='Number of samples to generate')
    gen_parser.add_argument('save_dir', action='store', type=str, help='Directory to save results to')
    gen_parser.add_argument('--input_shape', action='store', type=int, nargs='+', default=[64,64,24], help='Shape of the input data')
    args = parser.parse_args()

    if args.mode == 'train':
        print ("Training mode")
        batch_size = args.batch_size
        directory = os.path.realpath(args.directory)
        results_save_path = os.path.join(os.path.realpath(args.results_save_path), datetime.utcnow().strftime("%y-%m-%dT%H-%M-%S"))

        input_shape = tuple(args.input_shape)

        if len(input_shape) != 3:
            raise ValueError(f'Expected an input shape of 3D, instead got {input_shape}')
        print(f"Input shape: {input_shape}")

        datagenerator = TerrainDataLoader(directory, batch_size=batch_size, input_shape=input_shape)
        gan = TerrainGAN(results_path=results_save_path, input_shape=input_shape)

        if args.load_dir:
            load_dir = os.path.realpath(args.load_dir)
            print (f"Loading weights from {load_dir}")
            gan.load_from_dir(load_dir)

        gan.train(datagenerator, epochs=4000, save_interval=10, label_smoothing=args.label_smoothing, gradient_norm=args.gradient_norm)
    elif args.mode == 'generate':
        print ("Generation mode")
        model_dir = os.path.realpath(args.model_dir)
        save_dir = os.path.realpath(args.save_dir)

        if not os.path.exists(save_dir):
            os.makedirs(save_dir)
        num_samples = args.number
        input_shape = tuple(args.input_shape)

        gan = TerrainGAN(input_shape=input_shape)
        gan.load_from_dir(model_dir)
        gan.generate_samples(save_dir, num_samples)
    else:
        raise ValueError(f'Invalid mode {args.mode}')
