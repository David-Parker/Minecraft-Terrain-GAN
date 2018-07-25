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

import sys
import os
import numpy as np


def get_gradient_norm_func(model):
    """Get the gradient norm function of a Keras model"""
    grads = K.gradients(model.total_loss, model.trainable_weights)
    summed_squares = [K.sum(K.square(g)) for g in grads]
    norm = K.sqrt(sum(summed_squares))
    inputs = model._feed_inputs + model._feed_targets + model._feed_sample_weights
    func = K.function(inputs, [norm])
    return func

class TerrainGAN():
    """
    GAN for terrain generation

    Parameters
    ----------
    results_path : str, optional
        The path to save results to.
        Defaults to None.
    input_shape : tuple, optional
        Defaults to (16, 16, 16)

    """
    def __init__(self, results_path=None, input_shape=(16,16,16)):
        if K.image_data_format() == "channels_first":
            self.input_shape = (1,) + input_shape 
        else:
            self.input_shape = input_shape + (1,)

        if not results_path:
            results_path = os.path.dirname(__file__)

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

        # The generator takes noise as input and generates terrains 
        z = Input(shape=(100,))
        terrain = self.generator(z)

        # For the combined model we will only train the generator
        self.discriminator.trainable = False

        # The discriminator takes generated terrains as input and determines validity
        valid = self.discriminator(terrain)

        # The combined model  (stacked generator and discriminator)
        # Trains the generator to fool the discriminator
        self.combined = Model(z, valid)
        self.combined.compile(loss='binary_crossentropy', optimizer=optimizer)

        self.log_path = os.path.join(self.results_path, 'logs')

        self.get_gradient_norm = get_gradient_norm_func(self.combined)

    def build_generator(self):
        """Construct the graph for the generator"""
        model = Sequential()

        dim1 = int(self.input_shape[0] // 4)
        dim2 = int(self.input_shape[1] // 4)
        dim3 = int(self.input_shape[2] // 4)

        model.add(Dense(128 * dim1 * dim2 * dim3, activation="relu", input_dim=self.latent_dim))
        model.add(Reshape((dim1, dim2, dim3, 128)))
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
        terrain = model(noise)

        return Model(noise, terrain)

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

        terrain = Input(shape=self.input_shape)
        validity = model(terrain)

        return Model(terrain, validity)

    def write_to_log(self, callback, names, logs, batch_number):
        """Write information to TensorBoard"""
        for name, value in zip(names, logs):
            summary = tf.Summary()
            summary_value = summary.value.add()
            summary_value.simple_value = value
            summary_value.tag = name
            callback.writer.add_summary(summary, batch_number)
            callback.writer.flush()

    def train(self, data_generator, epochs, save_interval=50, 
              gradient_norm=False, label_smoothing=False):
        """Train the GAN

        Parameters
        ----------
        data_generator : TerrainDataLoader
        epochs : int
        save_interval : int, optional
            Defaults to 50
        gradient_norm : bool, optional
            Defaults to False
            Whether or not to print the gradient norm for 
            the generator and discriminator. Including this
            is helpful to detect failures, but takes a lot
            longer to train
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


            # ---------------------
            #  Train Generator
            # ---------------------

            # Train the generator (wants discriminator to mistake images as real)
            g_loss = self.combined.train_on_batch(noise, valid_labels)

            self.write_to_log(callback, discriminator_names, d_loss, batch_number)
            self.write_to_log(callback, generator_names, [g_loss], batch_number)

            if gradient_norm:
                gradient_norm = self.get_gradient_norm([noise, valid_labels, np.ones(len(valid_labels))])
                self.write_to_log(callback, ['gradient norm'], gradient_norm, batch_number)
                print ("Epoch: %d Batch: %d/%d [D loss: %f, acc.: %.2f%%] [G loss: %f] [Gradient norm: %f]" % (num_epochs, batch_number % batches_in_epoch, batches_in_epoch, d_loss[0], 100*d_loss[1], g_loss, gradient_norm[0]))
            else:
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

    def generate_samples(self, save_dir, number_of_samples):
        """Generate a specific number of samples
        
        We're really lazy here since we're generating by batches.
        We will actually generate the first multiple of len(gen_batch)
        >= number_of_samples

        """
        num_generated = 0

        while num_generated < number_of_samples:
            num_generated += self.generate_batch(save_dir, start_idx=num_generated)
        return num_generated

    def generate_batch(self, save_dir, start_idx=0):
        """Generate and save a batch for the generator"""
        r, c = 5, 5

        noise = np.random.normal(0, 1, (r * c, self.latent_dim))
        gen_batch = self.generator.predict(noise)

        for i, gen in enumerate(gen_batch):
            gen = np.round(gen.flatten()).astype(np.int16)
            save_path = os.path.join(save_dir, f"gen-{start_idx + i}")
            np.savetxt(save_path, gen[None,:], fmt="%d", delimiter=',')
        return len(gen_batch)

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
