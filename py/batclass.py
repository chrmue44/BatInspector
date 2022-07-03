import sys
import getopt
import audio
import time
import preptrain
import model
import tensorflow as tf

env = {
       'batchSize': 64,     #batch size for files training dataset
       'cut': False,
       'prepare': False,
       'runModel': False,
       "predict": False,
       "predictList": "",
       "dirRecordings": "",
       "callFile": "",
       'specFile': "",
       'model': "rnnModel",
       'trainDir': "",
       'trainDataMask':'',
       'withImg': False,
       'withAxes': False,
       'infoTestFile' : "",
       'epochs': 10,
       'cleanModel': False,
       'train' : False
      }


def printHelp():
    print
    """
    batclass.py 
    -a create axes in images for each call
    -c cut recordings in single wav (calls), img (spectrogram) and npy files (spectrogram)
    -d <dir> set directory path for recordings for training
    -e prepare train, dev and test set to train model
    -f <dataFile> csv file containing list of calls to process
    -g <infoTestFile> csv file containing information about test data set
    -h print this help
    -i create images for each call
    -m <model> select model
    -o <trainDir> specify root directory for training data
    -p <fileList> predict
    --specFile <specFile> set file containing list of species to learn    
    --run run model
    --train train model
    --clean clean model (delete pre trained weights)
    
    Examples:
    cut all recordings listed in call file to wav-files containing 1 call:
    batclass.py -c -f "C:/Users/chrmu/bat/train/calls_nnoc.csv" -o "C:/Users/chrmu/prj/BatInspector/mod/trn/"
    
    collect all calls and consolidate them to training, dev and test datasets
    batclass.py -e -s "C:/Users/chrmu/bat/train/species.csv" -o "C:/Users/chrmu/prj/BatInspector/mod/trn/"
    
    """

def parseArguments(argv):
    try:
        opts, args = getopt.getopt(argv,"acd:ef:g:hio:p:s:t", ['clean','run', 'train', 'specFile:'])
    except getopt.GetoptError:
        printHelp()
        sys.exit(2)
        
    for opt, arg in opts:
        if opt == '-a':
            env['withAxes'] = True
        elif opt == '-h':
            printHelp()
            sys.exit()
        elif opt == '-c':
            env["cut"] = True
        elif opt == '--clean':
            env["cleanModel"] = True
        elif opt == '-d':
            env["dirRecordings"] = arg
        elif opt == '-e':
            env["prepare"] = True
        elif opt == '-f':
            env["callFile"] = arg
        elif opt == '-g':
            env['infoTestFile'] = arg
        elif opt == '-i':
            env['withImg'] = True
        elif opt == '-m':
            env["model"] = arg
        elif opt == '-o':
            env["trainDir"] = arg
        elif opt == '--specFile':
            env["specFile"] = arg
        elif opt == '--run':
            env['runModel'] = True
        elif opt == '--train':
            env['train'] = True
        elif opt == '-p':
            env['predict'] = True
            env['predictList'] = arg

    #logName = "C:/Users/chrmu/prj/BatInspector/mod/trn/log_pred_errs.csv"
    #checkFileName = "C:/Users/chrmu/prj/BatInspector/mod/trn/checktest.csv"

def execute():
    print("**** bat classification running ****")
    if env['cut']:
        audio.processCalls(env['callFile'], env['trainDir'], withImg = env['withImg'], withAxes = env['withAxes'])
        
    if env['prepare']:
        outDir = env['trainDir']+'batch/'
        print ("**** prepare training data ****")
        print ("* output directory:", outDir)
        print ("*       batch size:", env['batchSize'])
        print ("*******************************")        
        env['trainDataMask'] = env['trainDir'] + 'dat/*_fft.npy'
        preptrain.prepareTrainingData(env['specFile'], env['trainDataMask'], outDir, env['batchSize'])
    if env['runModel']:
        model.runModel(env["train"], env["trainDir"], env["specFile"], 
                       logName = env["trainDir"] + '//log_pred_errs.csv', 
                       checkFileName = env['infoTestFile'], epochs = env['epochs'], cleanMod = env['cleanModel'],
                       showConfusion = True)
        
if __name__ == "__main__":
    start_time = time.time()
    parseArguments(sys.argv[1:])
    execute()
    end_time = time.time()
    time_elapsed = (end_time - start_time)
    print("time elapsed:", time_elapsed)
