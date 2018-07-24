# -*- coding: utf-8 -*-
"""
Data Loader for terrain generator
"""
import keras 
import threading
import numpy as np
import os

from keras import backend as K

def load_terrain(file_path, data_format='channels_last'):
    """Load the 16x16x16 terrain from the file path"""
    if data_format == 'channels_first':
        return np.loadtxt(file_path, delimiter=',').reshape(1,16,16,16)
    else:
        return np.loadtxt(file_path, delimiter=',').reshape(16,16,16,1)

class TerrainDataLoader(keras.utils.Sequence):
    """Data Loader for terrain files
    
    Parameters
    ----------
    directory : str
    batch_size : int, optional
        Defaults to 32
    shuffle : bool, optional
        Defaults to True
    terrain_shape : tuple, optional
        Defaults to (16, 16, 16)
    seed : Object, optional,
        Defaults to None

    """
    def __init__(self, directory,
                 batch_size=32, shuffle=True,
                 terrain_shape=(16, 16, 16),
                 seed=None):
        self.directory = directory
        self.batch_size = batch_size
        self._load_data()
        self.index_array = None
        self.shuffle = shuffle
        self.seed = seed
        self.total_batches_seen = 0
        self.lock = threading.Lock()
        self.index_generator = self._flow_index()

        self.data_format = K.image_data_format()
        if self.data_format == 'channels_first':
            self.terrain_shape = (1,) + terrain_shape
        else:
            self.terrain_shape = terrain_shape + (1,)


        print ("Found %d terrain files." % self.n)

        super(TerrainDataLoader, self).__init__()

    def _load_data(self):
        """Load data from directory"""
        file_paths = []
        for dirpath, dirnames, filenames in os.walk(self.directory):
            for filename in filenames:
                if not filename.endswith('.meta'):
                    file_paths.append(os.path.join(dirpath, filename))
        self.file_paths = file_paths
        self.n = len(file_paths)

    def __len__(self):
        return (self.n + self.batch_size - 1) // self.batch_size

    def on_epoch_end(self):
        self._set_index_array()

    def reset(self):
        self.batch_index = 0

    def _flow_index(self):
        self.reset()
        while 1:
            if self.seed is not None:
                np.random.seed(self.seed + self.total_batches_seen)
            if self.batch_index == 0:
                self._set_index_array()

            current_index = (self.batch_index * self.batch_size) % self.n
            if self.n > current_index + self.batch_size:
                self.batch_index += 1
            else:
                self.batch_index = 0

            self.total_batches_seen += 1
            yield self.index_array[current_index:
                                   current_index + self.batch_size]

    def _set_index_array(self):
        self.index_array = np.arange(self.n)
        if self.shuffle:
            self.index_array = np.random.permutation(self.n)

    def _get_batch_of_samples(self, index_array):
        batch_x = np.zeros(
                (len(index_array),) + self.terrain_shape,
                 dtype=K.floatx())
        for i, j in enumerate(index_array):
            fname = self.file_paths[j]
            terrain = load_terrain(os.path.join(self.directory, fname), self.data_format)
            batch_x[i] = terrain
        return batch_x

    def __getitem__(self, idx):
        if idx >= len(self):
            raise ValueError('Asked to retrieve element {idx}, '
                             'but the Sequence '
                             'has length {length}'.format(idx=idx,
                                                          length=len(self)))
        if self.seed is not None:
            np.random.seed(self.seed + self.total_batches_seen)
        self.total_batches_seen += 1
        if self.index_array is None:
            self._set_index_array()
        index_array = self.index_array[self.batch_size * idx:
                                       self.batch_size * (idx + 1)]
        batch = self._get_batch_of_samples(index_array)
        return batch

    def __iter__(self):
        return self

    def __next__(self, *args, **kwargs):
        with self.lock:
            index_array = next(self.index_generator)
        return self._get_batch_of_samples(index_array)

