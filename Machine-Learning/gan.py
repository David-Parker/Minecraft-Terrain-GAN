from __future__ import print_function, division

from keras.layers import Input, Dense, Reshape, Flatten, Dropout
from keras.layers import BatchNormalization, Activation, ZeroPadding3D
from keras.layers.advanced_activations import LeakyReLU
from keras.layers.convolutional import UpSampling3D, Conv3D
from keras.models import Sequential, Model
from keras.optimizers import Adam

from dataloader import TerrainDataLoader

import matplotlib.pyplot as plt

import sys

import numpy as np

class GAN():
    def __init__(self):
        # Input shape
        self.input_shape = (16, 16, 16)
        self.latent_dim = 100

        optimizer = Adam(0.0002, 0.5)

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

        model.add(Dense(128 * 7 * 7, activation="relu", input_dim=self.latent_dim))
        model.add(Reshape((7, 7, 128)))
        model.add(UpSampling3D())
        model.add(Conv3D(128, kernel_size=3, padding="same"))
        model.add(BatchNormalization(momentum=0.8))
        model.add(Activation("relu"))
        model.add(UpSampling3D())
        model.add(Conv3D(64, kernel_size=3, padding="same"))
        model.add(BatchNormalization(momentum=0.8))
        model.add(Activation("relu"))
        model.add(Conv3D(self.channels, kernel_size=3, padding="same"))
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
        model.add(ZeroPadding3D(padding=((0,1),(0,1))))
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
        for batch_number, batch in enumerate(data_generator):
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
            print ("%d [D loss: %f, acc.: %.2f%%] [G loss: %f]" % (epoch, d_loss[0], 100*d_loss[1], g_loss))

            # If at save interval => save generated image samples
            if epoch % save_interval == 0:
                self.save_batch(epoch)

    def save_imgs(self, epoch):
        r, c = 5, 5
        noise = np.random.normal(0, 1, (r * c, self.latent_dim))
        gen_batch = self.generator.predict(noise)

        save_dir = f"generated-{epoch}"
        if not os.exists(save_dir):
            os.makedirs(save_dir)
        for i, gen in enumerate(gen_batch):
            save_path = os.path.join(save_dir, f"gen-{i}")
            np.savetxt(gen, save_path, delimiter=',')

if __name__ == '__main__':
    gan = GAN()

    directory = os.path.realpath('../Data/')
    datagenerator = TerrainDataLoader(directory, batch_size=1)
    dcgan.train(epochs=4000, batch_size=1, save_interval=50)
