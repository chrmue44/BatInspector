import tensorflow as tf


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
    return l[modelName]
