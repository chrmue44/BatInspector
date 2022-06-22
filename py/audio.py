import matplotlib.pyplot as plt
from scipy.io import wavfile
import os
from pydub import AudioSegment
import csv
import numpy as np

dataFile = "C:/Users/chrmu/bat/train/test/calls.csv"
callFileDir = "C:/Users/chrmu/prj/BatInspector/mod/trn/"


mrgBefore = 10   #time before call [ms]
mrgAfter = 20    #time after call [ms]
startTime = 102  #start time call [ms]
duration = 25    #duration call [ms]

def extractPart(srcFile, dstFile, start, end):
    """
    extract a part from a wav file and save it as a file
    
    Arguments:
    srcFile - name of the source file (full path )
    dstFile - name of the destination file (full path)
    start - start time from the beginning of srcFile [ms]
    end - end time from the beginning of srcFile [ms]
    """
    recording = AudioSegment.from_wav(srcFile)
    call = recording[start:end]
    call.export(dstFile, format="wav")

def graph_spectrogram(file, withAxes = False):
    """
    Calculate and plot spectrogram for a wav audio file
    
    Arguments:
    wavFile: name of the wavFile
    imgFile: name of the image file to generate
    withAxes: True: generate freq and time axes
    """
    wavFile = file + ".wav"
    imgFile = file + ".png"
    rate, data = get_wav_info(wavFile)
    nfft = 512 # Length of each window segment
    noverlap = 240 # Overlap between windows
    nchannels = data.ndim
    plt.clf()
    if not withAxes:
        plt.axes().set_axis_off()
        fig,ax = plt.subplots(1)
        fig.subplots_adjust(left=0,right=1,bottom=0,top=1)
    if nchannels == 1:
        pxx, freqs, bins, im = plt.specgram(data, nfft, rate, noverlap = noverlap)
    elif nchannels == 2:
        pxx, freqs, bins, im = plt.specgram(data[:,0], nfft, fs, noverlap = noverlap)
    plt.savefig(imgFile)
    return pxx

def get_wav_info(wav_file):
    rate, data = wavfile.read(wav_file)
    return rate, data

def readFileInfos(row, dir):
    """
    read all informations needed to extract a call
   
    Arguments:
    row: dictionary containing one row of csv file
    dir: output directory 
    Returns:
    wavFile - name of wav file to extract a call
    callfile - name of call file without extension
    start - start time for call extraction
    end - end time for call extraction
    """
    
    nr = int(row['nr'])
    startStr = row['start']
    pos = startStr.rfind(':')
    startStr = startStr[pos+1:]
    startTime = float(startStr) * 1000.0
    duration = float(row['duration'])
    wavFile = row['name']
    pos = wavFile.rfind('/')
    callFile = dir + wavFile[pos+1:-4] + "_call" + f'{int(nr):03d}' 
            
    start = startTime - mrgBefore
    end = startTime + duration + mrgAfter
    
    return wavFile, callFile, start, end


def processCalls(csvFile, outDir, verbose = False):
    """
    process all calls in the csv file:
    generate wav file and img file
    Arguments:
    csvFile: name of the csv (;-delimited) file containing the call informations
    outDir: directory to output all the wav and png files
    verbose: True - print information about each detected call
    """
    with open(csvFile,  newline='') as f:
        reader = csv.DictReader(f, delimiter=';')
        for row in reader:
            wavFile, callFile, start, end = readFileInfos(row, outDir)
            if verbose:
                print("extract from:", wavFile)
                print("generate: ",callFile + ".wav")
                print("start:", start," end:",end)
            extractPart(wavFile, callFile+".wav", start, end)
            data = graph_spectrogram(callFile)
            np.save(callFile+".npy",data)
            if verbose:
                print("spectrogram shape: ", data.shape, " type:", type(data))

#main
processCalls(dataFile, callFileDir, True)
