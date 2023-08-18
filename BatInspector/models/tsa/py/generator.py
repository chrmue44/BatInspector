import numpy as np
import keras

#from https://stanford.edu/~shervine/blog/keras-how-to-generate-data-on-the-fly

class DataGenerator(keras.utils.Sequence):
    'Generates data for Keras'
    def __init__(self, list_IDsX, list_IDsY, batchSize=32, dim=(32,32,32), 
                 shuffle=True):
        'Initialization'
        self.dim = dim
        self.batch_size = batchSize
        self.list_IDsX = list_IDsX
        self.list_IDsY = list_IDsY
        self.rows = dim[0]
        self.timeSteps = dim[1]
        self.channels = dim[2]
        self.shuffle = shuffle
        self.on_epoch_end()

    def __len__(self):
        'Denotes the number of batches per epoch'
        return int(np.floor(len(self.list_IDsX)))

    def __getitem__(self, index):
        'Generate one batch of data'
        # Generate indexes of the batch
        #indexes = self.indexes[index*self.batch_size:(index+1)*self.batch_size]
        #index = self.indexes[index]
        # Find list of IDs
        IDX = self.list_IDsX[index]
        IDY = self.list_IDsY[index]

        # Generate data
        X, y = self.__data_generation(IDX, IDY)

        return X, y

    def on_epoch_end(self):
        'Updates indexes after each epoch'
        self.indexes = np.arange(len(self.list_IDsX))
        if self.shuffle == True:
            np.random.shuffle(self.indexes)

    def __data_generation(self, IDX, IDY):
        'Generates data containing batch_size samples' # X : (n_samples, *dim, n_channels)
        # Initialization
        #X = np.empty((self.batch_size, self.row, self.timeSteps))
        #y = np.empty((self.batch_size, self.channels), dtype=float32)

        # Generate data
        fileX = IDX
        fileY = IDY
        x = np.load(fileX)
        np.swapaxes(x,1,2)
        y = np.load(fileY)

        return x, y