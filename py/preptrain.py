import audio
import glob
import csv
import numpy as np

### global variables
speciesFile = "C:/Users/chrmu/bat/train/species.csv"
pathTrainFiles = "C:/Users/chrmu/prj/BatInspector/mod/trn/dat/*_max.npy"   # .._max.npy
checkFile = "C:/Users/chrmu/prj/BatInspector/mod/trn/check.csv"
verbosity = True
maxCallLen = 330   # max length of a training sample
xtrainFile = "C:/Users/chrmu/prj/BatInspector/mod/trn/xtrain.npy"
ytrainFile = "C:/Users/chrmu/prj/BatInspector/mod/trn/ytrain.npy"

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
    
def genTrainingData(listSpecies, listTrainSamples):
    """
    generate training data from training samples
    
    Arguments:
    listSpecies - name of csv file containing a list of specues to train
    listTrainSamples - a list of files with training samples
    """
    length = len(listSpecies)
    x = np.load(listTrainSamples[0])
    m = len(listTrainSamples)
    Xtrain = np.zeros((m, x.shape[0], maxCallLen))
    Ytrain = np.zeros((m, length))
    
    # prepare test list
    fields = ['sample', 'index','trunc']
    for spec in listSpecies:
        fields.append(spec)
    with open(checkFile, 'w', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(fields)
        #loop over the list of training samples
        idx = 0
        for file in listTrainSamples:
            file = to_raw(file)
            x = np.load(file)   
            row = [file, str(idx)]
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
            if idx % 200 == 0:
                print("processing sample ", idx)
    return Xtrain, Ytrain
    
#### main
listSpecies = readSpeciesInfo(speciesFile)
listTrainSamples = glob.glob(pathTrainFiles)
Xtrain,Ytrain = genTrainingData(listSpecies, listTrainSamples)
np.save(xtrainFile, Xtrain, allow_pickle = False)
np.save(ytrainFile, Ytrain, allow_pickle = False)
print("generation of test data finished")