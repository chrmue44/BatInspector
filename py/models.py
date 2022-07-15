import tensorflow as tf

def identity_block(x, filter):
    # copy tensor to variable called x_skip
    x_skip = x
    # Layer 1
    x = tf.keras.layers.Conv2D(filter, (3,3), padding = 'same')(x)
    x = tf.keras.layers.BatchNormalization(axis=3)(x)
    x = tf.keras.layers.Activation('relu')(x)
    # Layer 2
    x = tf.keras.layers.Conv2D(filter, (3,3), padding = 'same')(x)
    x = tf.keras.layers.BatchNormalization(axis=3)(x)
    # Add Residue
    x = tf.keras.layers.Add()([x, x_skip])     
    x = tf.keras.layers.Activation('relu')(x)
    return x
	
def convolutional_block(x, filter):
    # copy tensor to variable called x_skip
    x_skip = x
    # Layer 1
    x = tf.keras.layers.Conv2D(filter, (3,3), padding = 'same', strides = (2,2))(x)
    x = tf.keras.layers.BatchNormalization(axis=3)(x)
    x = tf.keras.layers.Activation('relu')(x)
    # Layer 2
    x = tf.keras.layers.Conv2D(filter, (3,3), padding = 'same')(x)
    x = tf.keras.layers.BatchNormalization(axis=3)(x)
    # Processing Residue with conv(1,1)
    x_skip = tf.keras.layers.Conv2D(filter, (1,1), strides = (2,2))(x_skip)
    # Add Residue
    x = tf.keras.layers.Add()([x, x_skip])     
    x = tf.keras.layers.Activation('relu')(x)
    return x

def resNet34Model(rows, timeSteps, classes):
#def resNet34(shape = (32, 32, 3), classes = 10):
    x_input = tf.keras.Input(shape=(rows, timeSteps))
    x = tf.keras.layers.Reshape((rows, timeSteps, 1))(x_input)
    # Step 1 (Setup Input Layer)
#    x_input = tf.keras.layers.Input(shape)
    x = tf.keras.layers.ZeroPadding2D((3, 3))(x)
    # Step 2 (Initial Conv layer along with maxPool)
    x = tf.keras.layers.Conv2D(64, kernel_size=7, strides=2, padding='same')(x)
    x = tf.keras.layers.BatchNormalization()(x)
    x = tf.keras.layers.Activation('relu')(x)
    x = tf.keras.layers.MaxPool2D(pool_size=3, strides=2, padding='same')(x)
    # Define size of sub-blocks and initial filter size
    block_layers = [3, 4, 6, 3]
    filter_size = 64
    # Step 3 Add the Resnet Blocks
    for i in range(4):
        if i == 0:
            # For sub-block 1 Residual/Convolutional block not needed
            for j in range(block_layers[i]):
                x = identity_block(x, filter_size)
        else:
            # One Residual/Convolutional Block followed by Identity blocks
            # The filter size will go on increasing by a factor of 2
            filter_size = filter_size*2
            x = convolutional_block(x, filter_size)
            for j in range(block_layers[i] - 1):
                x = identity_block(x, filter_size)
    # Step 4 End Dense Network
    x = tf.keras.layers.AveragePooling2D((2,2), padding = 'same')(x)
    x = tf.keras.layers.Flatten()(x)
    x = tf.keras.layers.Dense(512, activation = 'relu')(x)
    x = tf.keras.layers.Dense(classes, activation = 'softmax')(x)
    model = tf.keras.models.Model(inputs = x_input, outputs = x, name = "ResNet34")
    model._name = "resNet34Model"
    return model
    
    
def cnnModel(rows, timeSteps, classes):
    """
    resize(32,32): 10 Epochs 81% on test data
    resize(64,128): 10 Epochs 87.5% on test data
    
    """
    model = tf.keras.Sequential()
    model._name = "cnnModel"
    inputs = tf.keras.Input(shape=(rows, timeSteps))
    x = tf.keras.layers.Reshape((rows, timeSteps, 1))(inputs)
    # Downsample the input.
    x = tf.keras.layers.Resizing(64, 128)(x)
    # Normalize.
    #norm_layer,
    x = tf.keras.layers.Conv2D(32, 3, activation='relu')(x)
    x = tf.keras.layers.Conv2D(64, 3, activation='relu')(x)
    x = tf.keras.layers.MaxPooling2D()(x)
    x = tf.keras.layers.Dropout(0.25)(x)
    x = tf.keras.layers.Flatten()(x)
    x = tf.keras.layers.Dense(128, activation='relu')(x)
    x = tf.keras.layers.Dropout(0.5)(x)
    out = tf.keras.layers.Dense(classes, activation="softmax")(x)
    model = tf.keras.Model(inputs=inputs, outputs=out, name="cnnModel")
    return model


def flatModel(rows, timeSteps, classes):
    """
    256, 256, 128: 5 epochs, 86.5% accurracy on test data 
    256, 128, 64: 5 epochs, 86% accurracy on test data 
    128, 64: 7 epochs, 85% accurracy on test data     
    """
    model = tf.keras.Sequential()
    model._name = "flatModel"
    model.add(tf.keras.Input(shape=(rows, timeSteps)))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.Dense(128,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model

def flat2Model(rows, timeSteps, classes):
    """
    ???
    """
    model = tf.keras.Sequential()
    model._name = "flat2Model"
    model.add(tf.keras.Input(shape=(rows, timeSteps)))
    model.add(tf.keras.layers.Resizing(64, 128))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dropout(0.2))
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dropout(0.2))
    model.add(tf.keras.layers.Dense(128,  activation="relu"))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dropout(0.2))
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model

def lstmModel(rows, timeSteps, classes):
    """
    bad performance: accurracy 0.26 not increasing with epochs
    """
    model = tf.keras.Sequential()
    model._name = "rnnModel"
    model.add(tf.keras.layers.LSTM(units = 128, input_shape=(rows, timeSteps)))
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model

def rnnModel(rows, timeSteps, classes):
    """
    32 units best performance: 10 epochs, 88% accurracy on test data
    64 units best performance: 9 epochs, 87.6% accurracy on test data
    128 units best performance: 8 epochs, 88% accurracy on test data
    """
    model = tf.keras.Sequential()
    model._name = "rnnModel"
    model.add(tf.keras.layers.GRU(64, input_shape=(rows, timeSteps), return_sequences=True))
    model.add(tf.keras.layers.Flatten())
#    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model

def rnn1Model(rows, timeSteps, classes):
    """
    12 Epochs, 87,7% (90.2% with train set 01.07.22)
    """
    model = tf.keras.Sequential()
    model._name = "rnn1Model"
    model.add(tf.keras.layers.GRU(128, input_shape=(rows, timeSteps), return_sequences=True))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dense(128,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model

def rnn1aModel(rows, timeSteps, classes):
    """
    10 Epochs (91.3% with train set 01.07.22), dropout 0.5
    2nd drop out or batchNormalizatino does not help
    """
    model = tf.keras.Sequential()
    model._name = "rnn1aModel"
    model.add(tf.keras.layers.GRU(128, input_shape=(rows, timeSteps), return_sequences=True))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dropout(rate=0.5))                                  
    model.add(tf.keras.layers.Dense(128,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model


def rnn2Model(rows, timeSteps, classes):
    """
    64 units best performance: 5 epochs, 84.3% accurracy on test data
    """
    model = tf.keras.Sequential()
    model._name = "rnn2Model"
    model.add(tf.keras.layers.GRU(64, input_shape=(rows, timeSteps), return_sequences=True))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dropout(rate=0.5))                                  
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dropout(rate=0.5))                                  
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dropout(rate=0.5))                                  
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model

def rnn2aModel(rows, timeSteps, classes):
    """
    64 units best performance: 14 epochs, 86.3% accurracy on test data
    """
    model = tf.keras.Sequential()
    model._name = "rnn2aModel"
    model.add(tf.keras.layers.GRU(64, input_shape=(rows, timeSteps), return_sequences=True))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dropout(rate=0.5))                                  
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model


def rnn3Model(rows, timeSteps, classes):
    """
    best performance: 13 epochs, 87.3% accurracy on test data
    """
    model = tf.keras.Sequential()
    model._name = "rnn3Model"
    model.add(tf.keras.layers.Conv1D(input_shape = (rows, timeSteps), filters=196,kernel_size=15,strides=4))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Activation("relu"))
    model.add(tf.keras.layers.Dropout(rate=0.8))                                  
    model.add(tf.keras.layers.GRU(32, input_shape=(rows, timeSteps), return_sequences=True))
    model.add(tf.keras.layers.Flatten())
#    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model


def rnn4Model(rows, timeSteps, classes):
    """
    best performance: 23 epochs, 84.7% accurracy on test data
    """
    model = tf.keras.Sequential()
    model._name = "rnn4Model"
    model.add(tf.keras.layers.Conv1D(input_shape = (rows, timeSteps), filters=196,kernel_size=15,strides=4))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Activation("relu"))
    model.add(tf.keras.layers.Dropout(rate=0.8))                                  
    model.add(tf.keras.layers.GRU(64, return_sequences=True))
    model.add(tf.keras.layers.Dropout(rate=0.8))
    model.add(tf.keras.layers.BatchNormalization())                               
    model.add(tf.keras.layers.GRU(units=64, return_sequences =True))
    model.add(tf.keras.layers.Dropout(rate=0.8))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model


def rnn5Model(rows, timeSteps, classes):
    """
    best performance: 23 epochs, 84.7% accurracy on test data
    """
    model = tf.keras.Sequential()
    model._name = "rnn5Model"
    model.add(tf.keras.layers.Conv1D(input_shape = (rows, timeSteps), filters=196,kernel_size=15,strides=4))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Activation("relu"))
    model.add(tf.keras.layers.Dropout(rate=0.5))                                  
    model.add(tf.keras.layers.GRU(128, return_sequences=True))
    model.add(tf.keras.layers.Dropout(rate=0.5))
    model.add(tf.keras.layers.BatchNormalization())                               
    model.add(tf.keras.layers.GRU(units=128, return_sequences =True))
    model.add(tf.keras.layers.Dropout(rate=0.5))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dense(128,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model
    
def getModel(modelName):
    l = {}
    l['cnnModel'] = cnnModel
    l['flatModel'] = flatModel
    l['flat2Model'] = flat2Model
    l['lstmModel'] = lstmModel
    l['rnn1Model'] = rnn1Model
    l['rnn1aModel'] = rnn1aModel
    l['rnn2Model'] = rnn2Model
    l['rnn2aModel'] = rnn2aModel
    l['rnn3Model'] = rnn3Model
    l['rnn4Model'] = rnn4Model
    l['rnn5Model'] = rnn5Model
    l['resNet34Model'] = resNet34Model
    return l[modelName]
