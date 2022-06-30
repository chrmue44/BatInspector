import sys
import getopt

def printHelp():
    print
    """
    batclass.py 
    -c convert recordings and prepare training data
    -d <dir> set directory path for recordings for training
    -s <specFile> set file containing list of species to learn
    -h print this help
    -t train model 
    -m <model> select model
    -p <fileList> predict
    """
def main(argv):
    env =	{
       "prepare": False,
       "train": False,
       "predict": False,
       "predictList": "",
       "dirRecordings": "",
       "specFile": "",
       "model": "rnnModel"
    }
    try:
        opts, args = getopt.getopt(argv,"hpd:s:t:")
    except getopt.GetoptError:
        printHelp()
        sys.exit(2)
        
    for opt, arg in opts:
        if opt == '-h':
            printHelp()
            sys.exit()
        elif opt == '-c':
            env["prepare"] = True
        elif opt == '-d'
            env["dirRecordings"] = arg
        elif opt == '-m':
            env["model"] = arg
        elif opt == '-s'
            env["specFile"] = arg
        elif opt == '-t'
            env["train"] = True
        elif opt == '-p'
            env["predict"] = True
            env["predictList"] = arg            
        elif opt in ("-i", "--ifile"):
            inputfile = arg
        elif opt in ("-o", "--ofile"):
         outputfile = arg

    
if __name__ == "__main__":
   main(sys.argv[1:])