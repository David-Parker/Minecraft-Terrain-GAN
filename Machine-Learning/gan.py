from __future__ import print_function, division

from keras.layers import Input, Dense, Reshape, Flatten, Dropout
from keras.layers import BatchNormalization, Activation, ZeroPadding3D
from keras.layers.advanced_activations import LeakyReLU
from keras.layers.convolutional import UpSampling3D, Conv3D
from keras.models import Sequential, Model
from keras.optimizers import Adam
from keras import backend as K

from dataloader import TerrainDataLoader

import sys
import os
import numpy as np

import argparse

class GAN():
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

    def build_generator(self):
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
        model.add(Activation("tanh"))

        model.summary()

        noise = Input(shape=(self.latent_dim,))
        img = model(noise)

        return Model(noise, img)

    def build_discriminator(self):
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

    def train(self, data_generator, epochs, save_interval=50):
        num_epochs = 0
        batch_size = data_generator.batch_size

        valid = np.ones((batch_size, 1))
        fake = np.zeros((batch_size, 1))

        batches_in_epoch = len(data_generator)

        for batch_number, batch in enumerate(data_generator):
            if len(batch) < batch_size:
                # It's possible to return a batch with less than specified batch
                # size if we're at the end of the list. 
                continue 
            if batch_number >= len(data_generator) - 1:
                # We have to manually break as the generator continues indefinitely
                if num_epochs >= epochs:
                    break
                num_epochs += 1

            noise = np.random.normal(0, 1, (batch_size, self.latent_dim))
            gen_batch = self.generator.predict(noise)

            # Train the discriminator (real classified as ones and generated classified as zeros)
            d_loss_real = self.discriminator.train_on_batch(batch, valid)
            d_loss_fake = self.discriminator.train_on_batch(gen_batch, fake)
            d_loss = 0.5 * np.add(d_loss_real, d_loss_fake)

            # ---------------------
            #  Train Generator
            # ---------------------

            # Train the generator (wants discriminator to mistake images as real)
            g_loss = self.combined.train_on_batch(noise, valid)

            # Plot the progress
            print ("Epoch: %d Batch: %d/%d [D loss: %f, acc.: %.2f%%] [G loss: %f]" % (num_epochs, batch_number % batches_in_epoch, batches_in_epoch, d_loss[0], 100*d_loss[1], g_loss))

            # If at save interval => save generated image samples
            if num_epochs % save_interval == 0:
                self.save_batch(num_epochs)

    def save_batch(self, epoch):
        r, c = 5, 5
        noise = np.random.normal(0, 1, (r * c, self.latent_dim))
        gen_batch = self.generator.predict(noise)

        save_dir = os.path.join(self.results_path, f"generated-{epoch}")
        if not os.path.exists(save_dir):
            os.makedirs(save_dir)
        for i, gen in enumerate(gen_batch):
            gen = np.round(gen.flatten()).astype(np.int16)
            save_path = os.path.join(save_dir, f"gen-{i}.txt")
            np.savetxt(save_path, gen[None,:], fmt="%d", delimiter=',')
        generator_save_path = os.path.join(save_dir, f'generator-{epoch}.h5')
        disc_save_path = os.path.join(save_dir, f'disc-{epoch}.h5')
        combined_save_path = os.path.join(save_dir, f'combined-{epoch}.h5')
        self.generator.save(generator_save_path)
        self.discriminator.save(disc_save_path)
        self.combined.save(combined_save_path)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()

    parser.add_argument('--batch_size', action='store', type=int, default=32, help='batch size for data loading')
    parser.add_argument('--directory', action='store', type=str, default='../Data/FinalData', help='Directory from where to load data')
    parser.add_argument('--results_save_path', action='store', type=str, default='results/', help='Directory to save results to')
    args = parser.parse_args()

    batch_size = args.batch_size
    directory = os.path.realpath(args.directory)
    results_save_path = os.path.realpath(args.results_save_path)

    datagenerator = TerrainDataLoader(directory, batch_size=batch_size)
    gan = GAN(results_save_path)
    gan.train(datagenerator, epochs=4000, save_interval=50)

