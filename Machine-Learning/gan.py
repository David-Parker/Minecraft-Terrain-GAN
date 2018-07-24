# -*- coding: utf-8 -*-
"""
GAN for terrain generation
"""
from __future__ import print_function, division

import tensorflow as tf
from keras.layers import Input, Dense, Reshape, Flatten, Dropout
from keras.layers import BatchNormalization, Activation, ZeroPadding3D
from keras.layers.advanced_activations import LeakyReLU
from keras.layers.convolutional import UpSampling3D, Conv3D
from keras.models import Sequential, Model
from keras.optimizers import Adam
from keras.callbacks import TensorBoard
from keras import backend as K

from dataloader import TerrainDataLoader

import sys
import os
import numpy as np

import argparse

class TerrainGAN():
    """
    GAN for terrain generation

    Parameters
    ----------
    results_path : str
        The path to save results to

    """
    def __init__(self, results_path):
        if K.image_data_format() == "channels_first":
            self.input_shape = (1, 16, 16, 16) # 1 channel, 16x16x16 terrain 
        else:
            self.input_shape = (16, 16, 16, 1) # 16x16x16 terrain, 1 channel 

        self.latent_dim = 100

        optimizer = Adam(0.0002, 0.5)

        self.results_path = results_path

        # Build and compile the discriminator
        self.discriminator = self.build_discriminator()
        self.discriminator.compile(loss='binary_crossentropy',
            optimizer=optimizer,
            metrics=['accuracy'])

        # Build the generator
        self.generator = self.build_generator()

        # The generator takes noise as input and generates imgs
        z = Input(shape=(100,))
        img = self.generator(z)

        # For the combined model we will only train the generator
        self.discriminator.trainable = False

        # The discriminator takes generated images as input and determines validity
        valid = self.discriminator(img)

        # The combined model  (stacked generator and discriminator)
        # Trains the generator to fool the discriminator
        self.combined = Model(z, valid)
        self.combined.compile(loss='binary_crossentropy', optimizer=optimizer)

        self.log_path = os.path.join(self.results_path, 'logs')

    def build_generator(self):
        """Construct the graph for the generator"""
        model = Sequential()

        model.add(Dense(128 * 4 * 4 * 4, activation="relu", input_dim=self.latent_dim))
        model.add(Reshape((4, 4, 4, 128)))
        model.add(UpSampling3D())
        model.add(Conv3D(128, kernel_size=3, padding="same"))
        model.add(BatchNormalization(momentum=0.8))
        model.add(Activation("relu"))
        model.add(UpSampling3D())
        model.add(Conv3D(64, kernel_size=3, padding="same"))
        model.add(BatchNormalization(momentum=0.8))
        model.add(Activation("relu"))
        model.add(Conv3D(1, kernel_size=3, padding="same"))
        model.add(Activation("sigmoid"))

        model.summary()

        noise = Input(shape=(self.latent_dim,))
        img = model(noise)

        return Model(noise, img)

    def build_discriminator(self):
        """Construct the graph for the discriminator"""
        model = Sequential()

        model.add(Conv3D(32, kernel_size=3, strides=2, input_shape=self.input_shape, padding="same"))
        model.add(LeakyReLU(alpha=0.2))
        model.add(Dropout(0.25))
        model.add(Conv3D(64, kernel_size=3, strides=2, padding="same"))
        model.add(ZeroPadding3D(padding=((0,1),(0,1), (0,1))))
        model.add(BatchNormalization(momentum=0.8))
        model.add(LeakyReLU(alpha=0.2))
        model.add(Dropout(0.25))
        model.add(Conv3D(128, kernel_size=3, strides=2, padding="same"))
        model.add(BatchNormalization(momentum=0.8))
        model.add(LeakyReLU(alpha=0.2))
        model.add(Dropout(0.25))
        model.add(Conv3D(256, kernel_size=3, strides=1, padding="same"))
        model.add(BatchNormalization(momentum=0.8))
        model.add(LeakyReLU(alpha=0.2))
        model.add(Dropout(0.25))
        model.add(Flatten())
        model.add(Dense(1, activation='sigmoid'))

        model.summary()

        img = Input(shape=self.input_shape)
        validity = model(img)

        return Model(img, validity)

    def write_to_log(self, callback, names, logs, batch_number):
        """Write information to TensorBoard"""
        for name, value in zip(names, logs):
            summary = tf.Summary()
            summary_value = summary.value.add()
            summary_value.simple_value = value
            summary_value.tag = name
            callback.writer.add_summary(summary, batch_number)
            callback.writer.flush()

    def train(self, data_generator, epochs, save_interval=50, label_smoothing=False):
        """Train the GAN

        Parameters
        ----------
        data_generator : TerrainDataLoader
        epochs : int
        save_interval : int, optional
            Defaults to 50
        label_smoothing : bool, optional
            Defaults to False.
            A trick in stable GAN training is to use label smoothing,
            e.g. instead of 0s putting values between 0 and 0.3, and instead of 
            1s put value between 0.7-1.0
        
        """
        num_epochs = 0
        batch_size = data_generator.batch_size

        callback = TensorBoard(self.log_path)
        callback.set_model(self.combined)

        # Add random uniform noise to the labels to be between
        # 0.7-1.0 for real labels and 0-0.3 for fake labels
        valid = np.ones((batch_size, 1))
        fake = np.zeros((batch_size, 1))

        batches_in_epoch = len(data_generator)

        discriminator_names = ['discrimination_loss', 'discrimination_accuracy']
        generator_names = ['gen_loss']
        for batch_number, batch in enumerate(data_generator):
            if len(batch) < batch_size:
                # It's possible to return a batch with less than specified batch
                # size if we're at the end of the list. 
                num_epochs += 1
                continue 
            if batch_number % batches_in_epoch == batches_in_epoch - 1:
                # We have to manually break as the generator continues indefinitely
                if num_epochs >= epochs:
                    break
                num_epochs += 1

            if label_smoothing:
                valid_labels = valid - np.random.uniform(0, 0.3, (batch_size, 1))
                fake_labels = fake + np.random.uniform(0, 0.3, (batch_size, 1))
            else:
                valid_labels = valid
                fake_labels = fake

            noise = np.random.normal(0, 1, (batch_size, self.latent_dim))
            gen_batch = self.generator.predict(noise)

            # Train the discriminator (real classified as ones and generated classified as zeros)
            d_loss_real = self.discriminator.train_on_batch(batch, valid_labels)
            d_loss_fake = self.discriminator.train_on_batch(gen_batch, fake_labels)
            d_loss = 0.5 * np.add(d_loss_real, d_loss_fake)
            self.write_to_log(callback, discriminator_names, d_loss, batch_number)

            # ---------------------
            #  Train Generator
            # ---------------------

            # Train the generator (wants discriminator to mistake images as real)
            g_loss = self.combined.train_on_batch(noise, valid_labels)

            self.write_to_log(callback, generator_names, [g_loss], batch_number)

            # Plot the progress
            print ("Epoch: %d Batch: %d/%d [D loss: %f, acc.: %.2f%%] [G loss: %f]" % (num_epochs, batch_number % batches_in_epoch, batches_in_epoch, d_loss[0], 100*d_loss[1], g_loss))

            # If at save interval => save generated image samples
            if num_epochs % save_interval == 0 and batch_number % batches_in_epoch == 0:
                self.save_batch(num_epochs)

    def load_from_dir(self, directory):
        """Load model weights from a directory"""
        generator_path = os.path.join(directory, f'generator.h5')
        disc_path = os.path.join(directory, f'disc.h5')

        self.generator.load_weights(generator_path)
        self.discriminator.load_weights(disc_path)

    def generate_batch(self, save_dir):
        """Generate and save a batch for the generator"""
        r, c = 5, 5
        noise = np.random.normal(0, 1, (r * c, self.latent_dim))
        gen_batch = self.generator.predict(noise)

        for i, gen in enumerate(gen_batch):
            gen = np.round(gen.flatten()).astype(np.int16)
            save_path = os.path.join(save_dir, f"gen-{i}")
            np.savetxt(save_path, gen[None,:], fmt="%d", delimiter=',')

    def save_batch(self, epoch):
        """Save information while training"""
        save_dir = os.path.join(self.results_path, f"generated-{epoch}")
        if not os.path.exists(save_dir):
            os.makedirs(save_dir)
        self.generate_batch(save_dir)

        generator_save_path = os.path.join(save_dir, f'generator.h5')
        disc_save_path = os.path.join(save_dir, f'disc.h5')
        self.generator.save(generator_save_path)
        self.discriminator.save(disc_save_path)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()

    parser.add_argument('--batch_size', action='store', type=int, default=32, help='batch size for data loading')
    parser.add_argument('--directory', action='store', type=str, default='../Data/FinalData/BaseLine', help='Directory from where to load data')
    parser.add_argument('--results_save_path', action='store', type=str, default='results/', help='Directory to save results to')
    parser.add_argument('--load_dir', action='store', type=str, default=None, help='Where to load weights from')
    parser.add_argument('--save_interval', action='store', type=int, default=10, help='Period to save results')
    parser.add_argument('--label_smoothing', action='store_true', help='Whether or not to apply label smoothing. NOTE that this ruins the accuracy metric.')
    args = parser.parse_args()

    batch_size = args.batch_size
    directory = os.path.realpath(args.directory)
    results_save_path = os.path.realpath(args.results_save_path)

    datagenerator = TerrainDataLoader(directory, batch_size=batch_size)
    gan = TerrainGAN(results_save_path)

    if args.load_dir:
        load_dir = os.path.realpath(args.load_dir)
        print (f"Loading weights from {load_dir}")
        gan.load_from_dir(load_dir)

    gan.train(datagenerator, epochs=4000, save_interval=10, label_smoothing=args.label_smoothing)

