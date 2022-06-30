#import audio
#import audio
import glob
import csv
import numpy as np
import time

### global variables
speciesFile = "C:/Users/chrmu/bat/train/species.csv"
pathTrainFiles = "C:/Users/chrmu/prj/BatInspector/mod/trn/dat/*_fft.npy"   # .._max.npy
checkFile = "C:/Users/chrmu/prj/BatInspector/mod/trn/check"
verbosity = True
maxCallLen = 330   # max length of a training sample
TRAIN_PATH = "C:/Users/chrmu/prj/BatInspector/mod/trn/"
batchSize = 2048

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

def speciesToOnehot(spec, size):
    """
    convert species name to one hot vector (global listSpecies has to be filled in advance)
    
    Arguments:
    spec - string containing a species
    size - size of the returned vector
    
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
    
def genTrainingData(listSpecies, listTrainSamples, batchSize, fName):
    """
    generate training data from training samples and save batches to files
    
    Arguments:
    listSpecies - name of csv file containing a list of specues to train
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
        writer = csv.writer(f)
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
                    y = speciesToOnehot(spec, length)
                    break
                else:
                    y = speciesToOnehot('----', length)
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
                np.save(TRAIN_PATH + "X" + fName + f'{b:03d}' + ".npy", Xtrain)
                np.save(TRAIN_PATH + "Y" + fName + f'{b:03d}' + ".npy", Ytrain)
                Xtrain = np.zeros((batchSize, x.shape[0], maxCallLen))
                Ytrain = np.zeros((batchSize, length))
                b += 1
                idx = 0
                print("processing batch ", b, "/", bcnt)
        #handle last block
        if idx > 0:
            np.save(TRAIN_PATH + "X" + fName + f'{b:03d}' + ".npy", Xtrain)
            np.save(TRAIN_PATH + "Y" + fName + f'{b:03d}' + ".npy", Ytrain)
            Xtrain = np.zeros((batchSize, x.shape[0], maxCallLen))
            Ytrain = np.zeros((batchSize, length))
            print("processing batch ", b, "/", bcnt)
            
    return
    
#### main

start_time = time. time()
#processCalls(dataFile, callFileDir, dataFormat, verbose = verbose, withImg = withImg)
listSpecies = readSpeciesInfo(speciesFile)
listTrainSamples = glob.glob(pathTrainFiles)
np.random.shuffle(listTrainSamples)
trainSize = int(len(listTrainSamples) * 0.8)
devSize = int(len(listTrainSamples) * 0.1)
testSize = len(listTrainSamples) - devSize - trainSize 
listDev = listTrainSamples[trainSize:trainSize + devSize]
listTest = listTrainSamples[trainSize + devSize:]
if trainSize > 20000:
    trainSize = 20000
listTrain = listTrainSamples[:trainSize]
print("train size:", len(listTrain))
print("dev size:", len(listDev))
print("test size:", len(listTest))

print("gen test set")
genTrainingData(listSpecies, listTest, len(listTest), "test")
print("gen dev set")
genTrainingData(listSpecies, listDev, len(listDev), "dev")
print("gen train set")
genTrainingData(listSpecies, listTrain, len(listTrain), "train")
end_time = time.time()
time_elapsed = (end_time - start_time)
print("generation of test data finished")
print("time elapsed:", time_elapsed, "s")
