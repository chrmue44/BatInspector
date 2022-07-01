import sys
import getopt
import audio
import time
import preptrain

env = {
       'batchSize': 2048,     #batch size for files training dataset
       'cut': False,
       'prepare': False,
       'train': False,
       "predict": False,
       "predictList": "",
       "dirRecordings": "",
       "callFile": "",
       "specFile": "",
       "model": "rnnModel",
       "trainDir": "",
       'trainDataMask':''
      }


def printHelp():
    print
    """
    batclass.py 
    -c cut recordings in single wav (calls), img (spectrogram) and npy files (spectrogram)
    -d <dir> set directory path for recordings for training
    -e prepare train, dev and test set to train model
    -f <dataFile> csv file containing list of calls to process
    -h print this help
    -m <model> select model
    -o <trainDir> specify root directory for training data
    -p <fileList> predict
    -s <specFile> set file containing list of species to learn    
    -t train model 
    
    Examples:
    cut all recordings listed in call file to wav-files containing 1 call:
    batclass.py -c -f "C:/Users/chrmu/bat/train/calls_nnoc.csv" -o "C:/Users/chrmu/prj/BatInspector/mod/trn/"
    
    collect all calls and consolidate them to training, dev and test datasets
    batclass.py -e -s "C:/Users/chrmu/bat/train/species.csv" -o "C:/Users/chrmu/prj/BatInspector/mod/trn/"
    
    """

def parseArguments(argv):
    try:
        opts, args = getopt.getopt(argv,"cd:ef:gho:p:s:t")
    except getopt.GetoptError:
        printHelp()
        sys.exit(2)
        
    for opt, arg in opts:
        if opt == '-h':
            printHelp()
            sys.exit()
        elif opt == '-c':
            env["cut"] = True
        elif opt == '-d':
            env["dirRecordings"] = arg
        elif opt == '-e':
            env["prepare"] = True
        elif opt == '-f':
            env["callFile"] = arg
        elif opt == '-m':
            env["model"] = arg
        elif opt == '-o':
            env["trainDir"] = arg
        elif opt == '-s':
            env["specFile"] = arg
        elif opt == '-t':
            env["train"] = True
        elif opt == '-p':
            env['predict'] = True
            env['predictList'] = arg


def execute():
    if env['cut']:
        audio.processCalls(env['callFile'], env['trainDir'], 'npy')
    if env['prepare']:
        env['trainDataMask'] = env['trainDir'] + 'dat/*_fft.npy'
        preptrain.prepareTrainingData(env['specFile'], env['trainDataMask'], env['trainDir'])
#    if env['train']:
        
if __name__ == "__main__":
    start_time = time.time()
    parseArguments(sys.argv[1:])
    execute()
    end_time = time.time()
    time_elapsed = (end_time - start_time)
    print("time elapsed:", time_elapsed)
