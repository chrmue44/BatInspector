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
from tempfile import NamedTemporaryFile
import shutil
from models import getModel


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
    x = np.load(dirName + '/' + fNameX)
    np.swapaxes(x,1,2)
    y = np.load(dirName + '/' + fNameY)
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
        

def createModel(mod, input_shape, rootDir, modPars, train = False):

    """
    create a model 
    
    Arguments:
    mod - a model (untrained)
    input_shape - tuple containing 
                    rows - number of rows in input data,
                    timesteps - number of timesteps/cols in input data
                    classes - number of classes in output vector
    rootDir - name of the directory containing model data no ending slash)   
    train - True train the model and save weights, False load weights
    returns model (trained)
    """
    model = mod(input_shape[0], input_shape[1], input_shape[2])
    print(model.summary())
    
    weightsFile = rootDir + '/' + modPars['dirWeights'] + '/' + model.name + ".h5"
    if os.path.isfile(weightsFile):
        if modPars['clean']:
            os.remove(weightsFile) 
        else:
            model.load_weights(weightsFile)
    else:
        print('file: ' + weightsFile+ ' not found, set parameter "train" to TRUE')
        train = True
    if train:
        print("read train data set...")
        batDir = rootDir + '/' + modPars['dirBatch']
        files_x = glob.glob(batDir +"/Xtrain*.npy")
        files_y = glob.glob(batDir +"/Ytrain*.npy")        
        trainGenerator = gen.DataGenerator(files_x, files_y, batchSize = modPars['batchSize'])
       
        print("read dev data set...")
        dev_x = glob.glob(batDir +"/Xdev*.npy")
        dev_y = glob.glob(batDir +"/Ydev*.npy")
        devGenerator = gen.DataGenerator(dev_x, dev_y, batchSize = modPars['batchSize'])
    
    opt = tf.keras.optimizers.Adam(learning_rate=modPars['lrnRate'])
    model.compile(
        loss= "categorical_crossentropy", #'mean_squared_logarithmic_error',
        optimizer=opt,   #"adam", 
        metrics=["accuracy"]
        )

    if train:
        checkpoint = tf.keras.callbacks.ModelCheckpoint(weightsFile,
            save_weights_only=True,
            save_best_only=True,
            verbose=1)
        logDir = rootDir + '/' + modPars['dirLogs']
        tensorboard = tf.keras.callbacks.TensorBoard(log_dir=logDir)

        #model.fit(train_x, train_y, validation_data=(dev_x, dev_y), batch_size=32, epochs=10)
        #model.fit(train, validation_data=dev, batch_size=BATCH_SIZE, callbacks=[checkpoint, tensorboard], epochs=epochs)
        model.fit(trainGenerator, validation_data=devGenerator,  callbacks=[checkpoint, tensorboard], 
        epochs=modPars['epochs'], batch_size=modPars['batchSize'])
        model.load_weights(weightsFile)
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
    good_labels = []
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
                bad_labels.append(item)
            else:
                item = (absIdx, pred_max[i], labels_max[i], pred_batch[i])
                good_labels.append(item)
            absIdx += 1
                
        all_labels = np.concatenate([all_labels, labels_max])
    print("\n*******************")
    print("accuracy score: {:.3f}".format(accuracy_score(all_labels, all_predictions)))
    return all_labels, all_predictions, bad_labels, good_labels


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
        reader = csv.DictReader(f, delimiter=';')
        count = 0
        for row in reader:
            ret.append({'sample':row['sample'],'index':row['index']})
    return ret


def evalLogData(errList, resultFileName, checkFileName, speciesFile):
    """
    create a evaluation file with information about the wrongly labeled data
    
    Arguments:

    errList - list with errors
    resultFileName - name of the csv file containing the result
    checkFileName - name of the csv file containing information about the data set

    """
    
    checkInfo = readCheckInfo(checkFileName)
    
    with open(resultFileName, 'w', newline='') as f:
        writer = csv.writer(f, delimiter=';')
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
                reject = '1'
            else:
                reject = '0'
            row = [imgName, species[realIdx], species[predIdx], reject]
            for p in prediction:
                row.append(p)
            writer.writerow(row)
    
def calcPrecision(r, cm):
    """
    calculate precision p = TP / ( TP + FP)
    
    Arguments:
    r - row in confusion matrix
    cm - confusion matrix
    
    Returns:
    prec - precision
    """
    tp = cm[r,r].numpy()
    fp = 0.0
    for i in range(cm.shape[1]):
        if i != r:
            fp += cm[r, i].numpy()
    prec = tp / (tp + fp)
    return prec

    
def showConfusionMatrix(speciesFile, dirName, modelName, labels, predictions):
    """
    create confusion matrix, write it to csv file and generate a picture
    
    Arguments:
    speciesFile - file name for csv file containing the labels
    dirName - directory to store the confusion matrix
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
    plt.savefig(dirName + "/confusion_" + modelName + ".png")

    print("cm.shape:",cm.shape)
    
    #write csv file
    with open(dirName + "/confusion_" + modelName + ".csv", 'w', newline='') as f:
        writer = csv.writer(f, delimiter=';')
        
        #write header
        row = ['  ']
        for c in range(len(listSpec)):
            row.append(listSpec[c])
        row.append('precision')
        writer.writerow(row)
        
        #write lines for predictions
        for r in range(len(listSpec)):
            row = [listSpec[r]]
            for c in range(cm.shape[1]):
                row.append(cm[r,c].numpy())
            row.append(calcPrecision(r, cm))
            writer.writerow(row)
            
        
 

def runModel(train, rootDir, speciesFile, checkFileName, modPars, showConfusion = True):
    """
    crate and run the model
    
    Arguments:
    train - True model will be trained, False: exeute prediction
    rootDir - root directory of model data
    speciesFile - name of the csv file containing the list of species
    checkFileName - name of the csv file containing information about the test set (created during prep data phase)
    modPars - model parameters (defined in batclass.py)
    showConfusion - True shows the confusion matrix
    """
    print("#### run model #####")
    tf.random.set_seed(42)

    # clean up log area
    logdir = rootDir + '/' + modPars['dirLogs']
    shutil.rmtree(logdir + '/train', ignore_errors=True)
    shutil.rmtree(logdir + '/validation', ignore_errors=True)

    print("reading test set...")
    dirBatch = rootDir + '/' + modPars['dirBatch']
    test = readDataset(dirBatch, "Xtest000.npy", "Ytest000.npy")
    rows, timeSteps, classes = getDimensions(test)
    test = test.batch(modPars['batchSize'], drop_remainder=True)
        
    input_shape = (rows, timeSteps, classes)
    mod = getModel(modPars['modelName'])    
#    model = createModel(mod, input_shape, modPars['dirBatch'], modPars['dirWeights'], train, modPars['epochs'], modPars['clean'], logdir)
    model = createModel(mod, input_shape, rootDir, modPars, train)

    all_labels, all_predictions, bad_labels, good_labels = predictOnBatches(model, test)
    baseName = rootDir + '/' + modPars['dirLogs'] + '/' +  modPars['modelName'] + '_' + modPars['logName']
    evalLogData(bad_labels, baseName + 'bad.csv', checkFileName, speciesFile)
    evalLogData(good_labels, baseName + 'good.csv', checkFileName, speciesFile)

    if showConfusion:
        dirName = rootDir + '/' + modPars['dirLogs']
        showConfusionMatrix(speciesFile, dirName, model.name, all_labels, all_predictions)



def predict(dataName, speciesFile, report, rootDir, minSnr, modPars):
    """
    crate the model and predict
    
    Arguments:
    train - True model will be trained
    speciesFile - name of the csv file containing the list of species
    report - name of the report 
    rootDir - root directory where the model file is stored
    minSnr - min SNR value to accept a prediction
    """
    print("#### predict #####")
    listSpec = readSpeciesInfo(speciesFile)

    print("reading data set...")
    x = np.load(dataName)
    np.swapaxes(x,1,2)
   
    rows, timeSteps, classes = x.shape[1], x.shape[2], len(listSpec)
    print (x.shape)
    input_shape = (rows, timeSteps, classes)
    mod = getModel(modPars['modelName'])    
    model = createModel(mod, input_shape, rootDir, modPars)    
    y = model.predict(x)
    
    #write predictions in report file
    fields = []
    print('generating report: ', report)
    with open(report, "r") as f:
        reader = csv.reader(f, delimiter =';')
        for row in reader:
            fields = row
            break
    print(fields)
    
    tempfile = NamedTemporaryFile(mode='w',  newline='', delete=False)
    with open(report,  newline='') as r, tempfile:
        reader = csv.DictReader(r, delimiter=';')
        writer = csv.DictWriter(tempfile, delimiter=';', fieldnames = fields)
        writer.writeheader()
        idx = 0
        for row in reader:
            snr = row['snr']
            iMax = np.argmax(y[idx])
            row['prob'] = y[idx, iMax]
            if (y[idx, iMax] < 0.5):
                row['Species'] = '??PRO[' + listSpec[iMax] + ']'
            elif (float(snr) < minSnr):
                row['Species'] = '??SNR[' + listSpec[iMax] + ']'
            else:
                row['Species'] = listSpec[iMax]
            spIdx = 0
            for species in listSpec:
                row[species] = y[idx, spIdx]
                spIdx += 1
            writer.writerow(row)
            idx += 1
    
    shutil.move(tempfile.name, report)
    

