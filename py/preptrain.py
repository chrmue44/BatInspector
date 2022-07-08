import glob
import csv
import numpy as np
import time

### global variables
#speciesFile = "C:/Users/chrmu/bat/train/species.csv"
#pathTrainFiles = "C:/Users/chrmu/prj/BatInspector/mod/trn/dat/*_fft.npy"   # .._max.npy
checkFile = "C:/Users/chrmu/prj/BatInspector/mod/trn/check"
verbosity = True
maxCallLen = 330   # max length of a training sample
# TRAIN_PATH = "C:/Users/chrmu/prj/BatInspector/mod/trn/"

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

def speciesToOnehot(spec, size, listSpecies):
    """
    convert species name to one hot vector (global listSpecies has to be filled in advance)
    
    Arguments:
    spec - string containing a species
    size - size of the returned vector
    listSpecies - list containing a list of species to train
    
    Returns:
    onehot vector for species (if species is not found, then 
    """
    ret = np.zeros((size))
    try:
        index = listSpecies.index(spec)
        ret[index] = 1
    except ValueError as ve:
        ret[0] = 1 # the first element is 'other'
    return ret
def to_raw(string):
    return fr"{string}"
    
def genTrainingData(listSpecies, listTrainSamples, batchSize, fName, trainPath):
    """
    generate training data from training samples and save batches to files
    
    Arguments:
    listSpecies - list containing a list of species to train
    listTrainSamples - a list of files with training samples
    batchSize - size of one batch
    
    """
    length = len(listSpecies)
    x = np.load(listTrainSamples[0])
    m = len(listTrainSamples)
    bcnt = int(m / batchSize)
    if bcnt == 0:
        bcnt += 1
    Xtrain = np.zeros((batchSize, x.shape[0], maxCallLen))
    Ytrain = np.zeros((batchSize, length))
    
    # prepare test list
    fields = ['sample','batch','index','trunc']
    for spec in listSpecies:
        fields.append(spec)
    with open(checkFile + fName + ".csv", 'w', newline='') as f:
        writer = csv.writer(f, delimiter = ';')
        writer.writerow(fields)
        #loop over the list of training samples
        idx = 0
        b = 0
        for file in listTrainSamples:
            file = to_raw(file)
            x = np.load(file)   
            row = [file, str(b),str(idx)]
            trunc = False
            for spec in listSpecies:
                if spec.lower() in file.lower():
                    y = speciesToOnehot(spec, length, listSpecies)
                    break
                else:
                    y = speciesToOnehot('----', length, listSpecies)
            #add training sample and truncate if too long
            if x.shape[1] < maxCallLen:
                Xtrain[idx,:,0:x.shape[1]] = x
            else:
                Xtrain[idx,:,:] = x[:,0:maxCallLen]
                trunc = True
                print("x truncated",x.shape[1], file)

            if trunc:
                row.append('Trunc')
            else:
                row.append('-')
 
            #add y to info file
            for i in y:
                row.append(str(i))
            writer.writerow(row)
            
            Ytrain[idx,:] = y
            idx += 1
            if idx >= batchSize:
                np.save(trainPath + "X" + fName + f'{b:03d}' + ".npy", Xtrain)
                np.save(trainPath + "Y" + fName + f'{b:03d}' + ".npy", Ytrain)
                Xtrain = np.zeros((batchSize, x.shape[0], maxCallLen))
                Ytrain = np.zeros((batchSize, length))
                b += 1
                idx = 0
                print("processing batch ", b, "/", bcnt)
        #handle last block
        if idx > 0:
            np.save(trainPath + "X" + fName + f'{b:03d}' + ".npy", Xtrain)
            np.save(trainPath + "Y" + fName + f'{b:03d}' + ".npy", Ytrain)
            Xtrain = np.zeros((batchSize, x.shape[0], maxCallLen))
            Ytrain = np.zeros((batchSize, length))
            print("processing batch ", b, "/", bcnt)
            
    return
    
def prepareTrainingData(speciesFile, pathTrainFiles, outPath, batchSize):
    """
    consolidate all training samples to a train, dev and test set
    
    Arguments:
    speciesFile - csv file with a list of species to learn
    pathTrainFiles - a file filter for all the call data files to process
    outPath - target path for training data
    batchSize - batchSize for training data
    """
    listSpecies = readSpeciesInfo(speciesFile)
    listTrainSamples = glob.glob(pathTrainFiles)
    np.random.shuffle(listTrainSamples)
    trainSize = int(len(listTrainSamples) * 0.8)
    devSize = int(len(listTrainSamples) * 0.1)
    testSize = len(listTrainSamples) - devSize - trainSize 
    listDev = listTrainSamples[trainSize:trainSize + devSize]
    listTest = listTrainSamples[trainSize + devSize:]
    #if trainSize > 20000:
    #    trainSize = 20000
    listTrain = listTrainSamples[:trainSize]
    print("train size:", len(listTrain), " batch size: ", batchSize)
    print("dev size:", len(listDev))
    print("test size:", len(listTest))

    print("gen test set")
    genTrainingData(listSpecies, listTest, len(listTest), "test", outPath)
    print("gen dev set")
    genTrainingData(listSpecies, listDev, batchSize, "dev", outPath)
    print("gen train set")
    genTrainingData(listSpecies, listTrain, batchSize, "train", outPath)
    
def preparePrediction(speciesFile, pathFiles, outPath):
    """
    consolidate all data samples to a single set
    
    Arguments:
    speciesFile - csv file with a list of species to learn
    pathFiles - a file filter for all the call data files to process
    outPath - target path for training data
    """
    listSpecies = readSpeciesInfo(speciesFile)
    listSamples = glob.glob(pathFiles)
    size = int(len(listSamples))
    print("number of calls:", size)
    print("gen data set")
    genTrainingData(listSpecies, listSamples, size, "data", outPath)

