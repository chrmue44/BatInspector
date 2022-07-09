import sys
import getopt
import audio
import time
import preptrain
import model
import tensorflow as tf

env = {
       'cut': False,
       'prepare': False,
       'prepPredict': False,
       'runModel': False,
       'predict': False,
       'predictList': '',
       'predictData': '',
       'dirRecordings': '',
       "callFile": "",
       'specFile': "",
       'model': 'rnnModel',
       'trainDir': "",
       'trainDataMask':'',
       'withImg': False,
       'withAxes': False,
       'infoTestFile' : '',
       'epochs': 10,
       'cleanModel': False,
       'train' : False,
       'dirModel': '',
       'minSnr': 12.0
       }

modPars = {
      'batchSize': 64,         #batch size for files training dataset
      'modName': 'rnn1Model',
      'lrnRate': 0.0005,
      'epochs': 10,
      'clean': True,
      'dirData' :'dat',        #sub directory for the prepared data files
      'dirBatch' : 'bat',      #sub directory for the batches
      
}

def printHelp():
    print
    """
    batclass.py 
    --axes create axes in images for each call
    --cut cut recordings in single wav (calls), img (spectrogram) and npy files (spectrogram)
    -d <dir> set directory path for recordings for training
    --dirModel <modelDir> specify the directory for model weights
    --data data (npy) containing the calls to predict (MUST match with 'PredictList')
    --prepTrain prepare train, dev and test set to train model
    --prepPredict prepare files for prediction
    --csvcalls <dataFile> csv file containing list of calls to process
    -g <infoTestFile> csv file containing information about test data set
    -h print this help
    --img create images for each call
    -m <model> select model
    -o <trainDir> specify root directory for training data
    --predict - predict calls
    --specFile <specFile> - set file containing list of species to learn    
    --run - run model
    --train - train model
    --clean - clean model (delete pre trained weights)
    
    Examples:
    cut all recordings listed in call file to wav-files containing 1 call:
    batclass.py -c -f "C:/Users/chrmu/bat/train/calls_nnoc.csv" -o "C:/Users/chrmu/prj/BatInspector/mod/trn/"
    
    collect all calls and consolidate them to training, dev and test datasets
    batclass.py -e -s "C:/Users/chrmu/bat/train/species.csv" -o "C:/Users/chrmu/prj/BatInspector/mod/trn/"
    
    """

def parseArguments(argv):
    try:
        opts, args = getopt.getopt(argv,"d:eg:ho:p:s:t", ['data=','dirModel=','predict','prepPredict','img','clean',
                                                          'check', 'run', 'train', 'specFile=', 'prepTrain','cut','axes','csvcalls='])
    except getopt.GetoptError:
        printHelp()
        sys.exit(2)
        
    for opt, arg in opts:
        if opt == '--axes':
            env['withAxes'] = True
        elif opt == '-h':
            printHelp()
            sys.exit()
        elif opt == '--cut':
            env['cut'] = True
        elif opt == '--clean':
            env['cleanModel'] = True
        elif opt == '--dirModel':
            env['dirModel'] = arg
        elif opt == '--data':
            env['predictData'] = arg     
        elif opt == '-d':
            env['dirRecordings'] = arg
        elif opt == '--prepTrain':
            env['prepare'] = True
        elif opt == '--prepPredict':
            env['prepPredict'] = True
        elif opt == '--csvcalls':
            env["callFile"] = arg
        elif opt == '-g':
            env['infoTestFile'] = arg
        elif opt == '--img':
            env['withImg'] = True
        elif opt == '-m':
            env["model"] = arg
        elif opt == '-o':
            env['trainDir'] = arg
        elif opt == '--specFile':
            env['specFile'] = arg
        elif opt == '--run':
            env['runModel'] = True
        elif opt == '--train':
            env['train'] = True
        elif opt == '--predict':        
            env['predict'] = True
            
            
    #logName = "C:/Users/chrmu/prj/BatInspector/mod/trn/log_pred_errs.csv"
    #checkFileName = "C:/Users/chrmu/prj/BatInspector/mod/trn/checktest.csv"

def execute():
    print("**** bat classification running ****")
    if env['cut']:
        print ("**** cutting audio files ****")
        print ("* call file:",env['callFile'])
        print ("*   out dir:",env['trainDir'])
        audio.processCalls(env['callFile'], env['trainDir'], withImg = env['withImg'], withAxes = env['withAxes'])
        
    if env['prepare']:
        outDir = env['trainDir']+'/batch/'
        print ("**** prepare training data ****")
        print ("*     species file:", env['specFile'])
        print ("* output directory:", outDir)
        print ("*       batch size:", env['batchSize'])
        print ("*******************************")        
        env['trainDataMask'] = env['trainDir'] + 'dat/*_fft.npy'
        preptrain.prepareTrainingData(env['specFile'], env['trainDataMask'], outDir, env['batchSize'])
    
    if env['prepPredict']:
        print ("**** prepare predict data ****")
        print ("*     species file:", env['specFile'])
        print ("* output directory:", env['trainDir'])
        env['trainDataMask'] = env['trainDir'] + 'dat/*_fft.npy'
        preptrain.preparePrediction(env['specFile'], env['trainDataMask'], env['trainDir'])
    
    if env['runModel']:
        print('********* run model ***********')
        print('*        train:', env['train'])
        print('*     dir name:', env['trainDir'])
        print('* species file:', env['specFile'])
        print('*    dir model:', env['dirModel'])
        print ("*******************************")        
        model.runModel(train = env['train'], dirName = env['trainDir'],dirModel = env['dirModel'],
                       speciesFile = env['specFile'], logName = env["trainDir"] + '//log_', 
                       checkFileName = env['infoTestFile'], epochs = env['epochs'], cleanMod = env['cleanModel'],
                       showConfusion = True)

    if env['predict']:
        print('********* predict calls ***********')
        print('*   list calls:', env['callFile'])
        print('*    data file:', env['predictData'])
        print('* species file:', env['specFile'])
        print('*    dir model:', env['dirModel'])
        
        model.predict(env['predictData'], env['specFile'], env['callFile'], env['dirModel'], minSnr = env['minSnr'])
        
if __name__ == "__main__":
    start_time = time.time()
    parseArguments(sys.argv[1:])
    execute()
    end_time = time.time()
    time_elapsed = (end_time - start_time)
    print("time elapsed:", time_elapsed)
