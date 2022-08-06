import sys
import getopt
import audio
import time
import preptrain
import exec_model as mod
import tensorflow as tf

env = {
       'cut': False,
       'prepare': False,
       'prepPredict': False,
       'runModel': False,
       'predict': False,
       'predictList': '',
       'predictData': '',
       'callFile': "",
       'specFile': "",
       'rootDir': "",
       'trainDataMask':'',
       'infoTestFile' : '',
       'train' : False,
       'minSnr': 8.0,
       'resample': False,
       'outFile':'',
       'sampleRate': 312500,
       'stretchFactor' : 1,
       'dataDir' : '',
       'inDirMask': '',
       }

modPars = {
      'batchSize': 64,           #batch size for files training dataset
      'modelName':'rnn6aModel',   #type of model to run
      'lrnRate': 0.00002,
      'epochs': 30,
      'clean': False,
      'dirWav' :'wav',           #sub directory to store single call wav files
      'dirImg' : 'img',          #sub directory to store generated spectrograms of single calls      
      'dirData' :'dat',          #sub directory for the prepared data files
      'dirBatch' : 'bat',        #sub directory for the batches
      'dirWeights' : 'wgt',      #sub directory to store the weights file     
      'dirLogs': 'log',          #sub directory for log files   
      'logName': 'log_'          #base name for generated log files for good and bad samples during test of model
}

def printModelPars(modPars):
    print('##### model parameters #######')
    print('    batch size:', modPars['batchSize'])
    print('    model name:', modPars['modelName'])
    print(' learning rate:', modPars['lrnRate'])
    print('        epochs:', modPars['epochs'])
    print('         clean:', modPars['clean'])
    print('##############################')

audioPars = {
       'denoise': 'threshold',   #type of denoising: 'threshold', 'energy'
       'threshold': 0.005,       #threshold value (amplitude) for denoising type threshold
       'energy':  0.95,          #threshold value (percentage) for denoising type energy
       'withImg': False,         #create image files for each call
       'withAxes': False,        #include axes in image file
       'saveWav': True,          #create wav file for each call (must be true at the moment)
       'scaleType': 'lin',       #scale type for spectrogram
       'mrgBefore': 10,          #added time before call [ms] when cutting out single calls
       'mrgAfter': 20            #added timetime after call [ms] when cutting out single calls
}
def printHelp():
    print
    """
    batclass.py 
    --axes create axes in images for each call
    --cut cut recordings in single wav (calls), img (spectrogram) and npy files (spectrogram)
    --data data (npy) containing the calls to predict (MUST match with 'PredictList')
    --dataDir root directory of data to predict
    --epochs specify nr of epochs to train
    --prepTrain prepare train, dev and test set to train model
    --prepPredict prepare files for prediction
    --csvcalls <dataFile> csv file containing list of calls to process
    -g <infoTestFile> csv file containing information about test data set
    -h print this help
    --wav save extracted calls as wav files
    --img create images for each call
    --model <model> select model
    --root <rootDir> specify root directory for training data
    --predict - predict calls
    --specFile <specFile> - set file containing list of species to learn    
    --run - run model
    --train - train model
    --clean - clean model (delete pre trained weights)
    --resample <inputDirMask> resample to new sample rate
    --sampleRate <sampleRate> new rate for --resample
    --outFile <outfile>
    --stretch <factor> stretch factor during resampling operation
    --minSnr <snr> min value for SNR to accept prediction of recording
    Examples:
    cut all recordings listed in call file to wav-files containing 1 call:
    
    collect all calls and consolidate them to training, dev and test datasets
     
    """

def parseArguments(argv):
    try:
        opts, args = getopt.getopt(argv,"d:eg:hp:s:t", ['data=','predict','prepPredict','img','clean',
                                                        'root=', 'check', 'run', 'train', 'specFile=', 'prepTrain',
                                                        'wav','model=','cut','axes','csvcalls=','epochs=', 'minSnr=',
                                                        'resample=','sampleRate=', 'outFile=', 'dataDir=', 'stretch='])
    except getopt.GetoptError:
        printHelp()
        sys.exit(2)
    print('arguments:')    
    for opt, arg in opts:
        print('opt:', opt, ', arg:', arg)
        if opt == '--axes':
            audioPars['withAxes'] = True
        elif opt == '-h':
            printHelp()
            sys.exit()
        elif opt == '--cut':
            env['cut'] = True
        elif opt == '--clean':
            modPars['clean'] = True
        elif opt == '--data':
            env['predictData'] = arg     
        elif opt == '--prepTrain':
            env['prepare'] = True
        elif opt == '--prepPredict':
            env['prepPredict'] = True
        elif opt == '--csvcalls':
            env["callFile"] = arg
        elif opt == '-g':
            env['infoTestFile'] = arg
        elif opt == '--img':
            audioPars['withImg'] = True
        elif opt == '--model':
            modPars['modelName'] = arg
        elif opt == '--root':
            env['rootDir'] = arg
        elif opt == '--specFile':
            env['specFile'] = arg
        elif opt == '--run':
            env['runModel'] = True
        elif opt == '--train':
            env['train'] = True
        elif opt == '--predict':        
            env['predict'] = True
        elif opt == '--wav':
            env['saveWav'] = True
        elif opt == '--resample':
            env['resample'] = True
            env['inDirMask'] = arg
        elif opt == '--resample':
            env['sampleRate'] = int(arg)
        elif opt == '--outFile':
            env['outFile'] = arg
        elif opt == '--epochs':
            modPars['epochs'] = int(arg)
        elif opt == '--dataDir':
            env['dataDir'] = arg
        elif opt == '--stretch':
            env['stretch'] = int(arg)
        elif opt == '--minSnr':
            env['minSnr'] = float(arg)
    #logName = "C:/Users/chrmu/prj/BatInspector/mod/trn/log_pred_errs.csv"
    #checkFileName = "C:/Users/chrmu/prj/BatInspector/mod/trn/checktest.csv"

def execute():
    print("**** bat classification running ****")
    if env['resample']:
        print('********* resample WAV file ***********')
        print('*          input:', env['inDirMask'])      
        print('*    sample rate:', env['sampleRate'])
        print('* stretch factor:', env['stretchFactor'])
        audio.resampleWavFiles(env['inDirMask'], env['sampleRate'], env['stretchFactor'])

    if env['cut']:
        print ("**** cutting audio files ****")
        print ("* call file:", env['callFile'])
        print ("*   out dir:", env['dataDir'])
        audio.processCalls(env['callFile'], env['dataDir'], modPars, audioPars)
        
    if env['prepare']:
        outDir = env['rootDir'] + '/' + modPars['dirBatch'] 
        print ("**** prepare training data ****")
        print ("*     species file:", env['specFile'])
        print ("* output directory:", outDir)
        print ("*       batch size:", modPars['batchSize'])
        print ("*******************************")        
        env['trainDataMask'] = env['rootDir'] + '/' + modPars['dirData'] + '/*_fft.npy'
        logDir = env['rootDir'] + '/' + modPars['dirLogs']
        preptrain.prepareTrainingData(env['specFile'], logDir, env['trainDataMask'], outDir, modPars['batchSize'])
    
    if env['prepPredict']:
        print ("**** prepare predict data ****")
        print ("*     species file:", env['specFile'])
        if env['train']:
            env['trainDataMask'] = env['rootDir'] + '/' + modPars['dirData'] +'/*_fft.npy'
            print ("* output directory:", env['rootDir'])
        else:
            env['trainDataMask'] = env['dataDir'] + '/' + modPars['dirData'] +'/*_fft.npy'
            print ("* output directory:", env['dataDir'])

        logDir = env['dataDir'] + '/' + modPars['dirLogs']
        preptrain.preparePrediction(env['specFile'], logDir, env['trainDataMask'], env['dataDir'])
    
    if env['runModel']:
        print('********* run model ***********')
        print('*        train:', env['train'])
        print('*     dir name:', env['rootDir'])
        print('* species file:', env['specFile'])
        print ("*******************************")
        printModelPars(modPars)        
        mod.runModel(train = env['train'], rootDir = env['rootDir'],speciesFile = env['specFile'],
                       checkFileName = env['infoTestFile'], modPars = modPars, showConfusion = True)
                       
    if env['predict']:
        print('********* predict calls ***********')
        print('*   list calls:', env['callFile'])
        print('*    data file:', env['predictData'])
        print('* species file:', env['specFile'])
        print('*     min. SNR:', env['minSnr'])
        mod.predict(env['predictData'], env['specFile'], env['callFile'], 
                    rootDir = env['rootDir'], minSnr = env['minSnr'], modPars = modPars)
    

if __name__ == "__main__":
    start_time = time.time()
    parseArguments(sys.argv[1:])
    execute()
    end_time = time.time()
    time_elapsed = (end_time - start_time)
    print("time elapsed:", time_elapsed)
