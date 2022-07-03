import numpy as np
import os
import shutil
import tensorflow as tf
import glob
import csv
import seaborn as sns
import matplotlib.pyplot as plt
from sklearn.metrics import accuracy_score, confusion_matrix
import generator as gen

BATCH_SIZE = 64

def clean_logs(data_dir):
    logs_dir = os.path.join(data_dir, "logs")
    shutil.rmtree(logs_dir, ignore_errors=True)
    return logs_dir


def readSpeciesInfo(csvFile):
    """
    read species list from csv file
    
    Arguments:
    csvFile - name of the csv file containing the species list
    
    Returns:
    ret - list of species
    """
    ret = ['----']
    with open(csvFile,  newline='') as f:
        reader = csv.DictReader(f, delimiter=';')
        count = 0
        for row in reader:
            ret.append(row['species'])
    return ret


def readDataset(dirName, fNameX, fNameY):
    """
    read data from data file
    
    Returns:
    dset tf.Dataset :np.array (a,b) data, np.array (1,c) result
    """
    x = np.load(dirName + fNameX)
    np.swapaxes(x,1,2)
    y = np.load(dirName + fNameY)
    dset = tf.data.Dataset.from_tensor_slices((x, y))
    return dset

def getDimensions(data):
    """
    get dimensions of test data and result in a dataset (before calling Dataset.batch()
    
    Arguments:
    data - dataset containing (test_data, results)
    
    Returns:
    rows - number of rows in input data
    timesteps - number of timesteps/cols in input data
    classes - number of classes in output vector
    """
    it = iter(data)
    elem = next(it)
    rows = elem[0].shape[0]
    timeSteps = elem[0].shape[1]
    classes = elem[1].shape[0]
    print ("rows, timeSteps, classes:",rows, timeSteps, classes)
    return rows, timeSteps,classes

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

    
    

def createModel(mod, input_shape, dirName, train, epochs, cleanMod, logs_dir):
    """
    create a model 
    
    Arguments:
    mod - a model (untrained)
    input_shape - tuple containing 
                    rows - number of rows in input data,
                    timesteps - number of timesteps/cols in input data
                    classes - number of classes in output vector
    train - True train the model and save weights, False load weights
    epochs - numbr of epochs to train
    cleanMod - True delete pretrained weights
    logs_dir - directory name for log file
    returns model (trained)
    """
    model = mod(input_shape[0], input_shape[1], input_shape[2])
    print(model.summary())
    weightsFile = model.name + ".h5"
    if os.path.isfile(weightsFile):
        if cleanMod:
            os.remove(weightsFile) 
        else:
            model.load_weights(weightsFile)
    else:
        train = True
    if train:
        print("read train data set...")
        files_x = glob.glob(dirName +"/Xtrain*.npy")
        files_y = glob.glob(dirName +"/Ytrain*.npy")        
        trainGenerator = gen.DataGenerator(files_x, files_y, batchSize = BATCH_SIZE)
       
        print("read dev data set...")
        dev_x = glob.glob(dirName +"/Xdev*.npy")
        dev_y = glob.glob(dirName +"/Ydev*.npy")
        devGenerator = gen.DataGenerator(dev_x, dev_y, batchSize = BATCH_SIZE)
    
    opt = tf.keras.optimizers.Adam(learning_rate=0.001)
    model.compile(
        loss= "categorical_crossentropy", #'mean_squared_logarithmic_error',
        optimizer=opt,   #"adam", 
        metrics=["accuracy"]
        )

    checkpoint = tf.keras.callbacks.ModelCheckpoint(weightsFile,
        save_weights_only=True,
        save_best_only=True,
        verbose=1)

    tensorboard = tf.keras.callbacks.TensorBoard(log_dir=logs_dir)

    if train:
    #model.fit(train_x, train_y, validation_data=(dev_x, dev_y), batch_size=32, epochs=10)
    #model.fit(train, validation_data=dev, batch_size=BATCH_SIZE, callbacks=[checkpoint, tensorboard], epochs=epochs)
        model.fit(trainGenerator, validation_data=devGenerator,  callbacks=[checkpoint, tensorboard], epochs=epochs, batch_size=BATCH_SIZE)

    return model

def predictOnBatches(model, data):
    """
    predict 
    
    Arguments:
    model - the model (trained)
    data - dataset containing test data and true labels
    
    Returns:
    all_labels - 1D vector with true labels (as index)
    all_predictions - 1D vector with predictions (as index)
    bad_labels - list containing tuples with information about bad_labels:
    (index in dataset data, predicted class, real class, array with predicted probabilities for each class)
    """
    all_predictions = np.array([])
    all_labels = np.array([])
    bad_labels = []
    absIdx = 0
    for test_batch in data:
        inputs_b, labels_b = test_batch
        pred_batch = model.predict(inputs_b)
        pred_max = np.argmax(pred_batch, axis = 1)
        all_predictions = np.concatenate([all_predictions, pred_max])
        labels_max = np.argmax(labels_b, axis = 1)
        for i in range(len(labels_b)):
            if pred_max[i] != labels_max[i]:
                item = (absIdx, pred_max[i], labels_max[i], pred_batch[i])
#                print ("bad:", item)
                bad_labels.append(item)
            absIdx += 1
                
        all_labels = np.concatenate([all_labels, labels_max])
    print("\n*******************")
    print("accuracy score: {:.3f}".format(accuracy_score(all_labels, all_predictions)))
    return all_labels, all_predictions, bad_labels


def readCheckInfo(csvFile):
    """
    read datafile for dataset from csv file
    
    Arguments:
    csvFile - name of the csv file
    Returns:
    ret - list of check data
    """
    ret = []
    with open(csvFile,  newline='') as f:
        reader = csv.DictReader(f, delimiter=',')
        count = 0
        for row in reader:
            ret.append({'sample':row['sample'],'index':row['index']})
    return ret

def evalErrorData(errList, resultFileName, checkFileName, speciesFile):
    """
    create a evaluation file with information about the wrongly labeled data
    
    Arguments:

    errList - list with errors
    resultFileName - name of the csv file containing the result
    checkFileName - name of the csv file containing information about the data set

    """
    
    checkInfo = readCheckInfo(checkFileName)
    
    with open(resultFileName, 'w', newline='') as f:
        writer = csv.writer(f)
        #create header
        species = readSpeciesInfo(speciesFile)
        fields = ['call','realSpec','errSpec','reject']
        for s in species:
            fields.append(s)
        writer.writerow(fields)
    
        for i in range(len(errList)):
            errItem = errList[i]
            absIdx = errItem[0]
            predIdx = errItem[1]
            realIdx = errItem[2]
            prediction = errItem[3]
            imgName = checkInfo[absIdx]['sample'].replace('/dat', '/img')
            imgName = imgName.replace('_fft.npy','.png')
            if prediction[predIdx] < 0.5:
                reject = 'reject'
            else:
                reject = '-'
            row = [imgName, species[realIdx], species[predIdx], reject]
            for p in prediction:
                row.append(p)
            writer.writerow(row)
    
def showConfusionMatrix(speciesFile, labels, predictions):
    """
    show confusion matrix
    
    Arguments:
    speciesFile - file name for csv file containing the labels
    labels - true labels 1D array
    predictions - predicted values 1D array
    """
    listSpec = readSpeciesInfo(speciesFile)
    cm = tf.math.confusion_matrix(labels, predictions)
    plt.figure(figsize=(10, 8))
    sns.heatmap(cm,
            xticklabels=listSpec,
            yticklabels=listSpec,
            annot=True, fmt='g')
    plt.xlabel('Prediction')
    plt.ylabel('Label')
    #plt.show()
    plt.savefig("confusion.png")
  

def runModel(train, dirName, speciesFile, logName, checkFileName, epochs, cleanMod, showConfusion = True):
    """
    crate and run the model
    
    Arguments:
    train - True model will be trained
    speciesFile - name of the csv file containing the list of species
    logName - name of the logfile created for the erroneous predictions
    checkFileName - name of the csv file containing information about the test set (created during prep data phase)
    showConfusion - True shows the confusion matrix
    """
    print("#### run model #####")
    tf.random.set_seed(42)

    # clean up log area
    data_dir = "./data"
    logs_dir = clean_logs(data_dir)

    print("reading test set...")
    test = readDataset(dirName, "Xtest000.npy", "Ytest000.npy")
    rows, timeSteps, classes = getDimensions(test)
    test = test.batch(BATCH_SIZE, drop_remainder=True)
    input_shape = (rows, timeSteps, classes)
    model = createModel(rnn1Model, input_shape, dirName, train, epochs, cleanMod, logs_dir)

    all_labels, all_predictions, bad_labels = predictOnBatches(model, test)
    evalErrorData(bad_labels, logName, checkFileName, speciesFile)

    if showConfusion:
        showConfusionMatrix(speciesFile, all_labels, all_predictions)
