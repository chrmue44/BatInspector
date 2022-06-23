import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')
from scipy.io import wavfile
import os
from pydub import AudioSegment
import csv
import numpy as np
import time

dataFile = "C:/Users/chrmu/bat/train/calls.csv"
callFileDir = "C:/Users/chrmu/prj/BatInspector/mod/trn/"


mrgBefore = 10    #time before call [ms]
mrgAfter = 20     #time after call [ms]
threshold = 0.005 #threshold value for noise canceling
scaleType = 'lin' #scaling of spectrogram ('log', 'lin')
verbose = False   #show more infos on console
withAxes = False  #show axes in pngs
dataFormat = 'npy' # format to save arrays: 'csv, 'npy'
withImg = False

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
    if start < 0:
        start = 0
    if end > recording.duration_seconds * 1000:
        end = recording.duration_seconds * 1000 - 1
    call = recording[start:end]
    call.export(dstFile, format="wav")
    return call

def spectrogramFromAudio(seg):
    """
    createsa spectrogram 
    
    Arguments:
    seg - audio segment
    """
    freq, time, amp = seg.spectrogram(window_length_samples=256, overlap=0.75)
    return amp
    
def graph_spectrogram(wavFile, imgFile, withAxes = False, scale = 'log'):
    """
    Calculate and plot spectrogram for a wav audio file
    
    Arguments:
    wavFile: name of the wavFile
    imgFile: name of the image file to generate
    withAxes: True: generate freq and time axes
    
    Returns:
    data - a two dimensional array containing the spectrogram
    """
    rate, data = get_wav_info(wavFile)
    nfft = 256 # Length of each window segment
    noverlap = 256 - 64 # Overlap between windows
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
    if scale == 'log':
        pxx = np.log10(pxx)
    return pxx, freqs, bins

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
    callFile = dir +"wav/"+ wavFile[pos+1:-4] + "_call_" + f'{int(nr):03d}' + ".wav" 
    imgFile = dir +"img/"+ wavFile[pos+1:-4] + "_call_" + f'{int(nr):03d}' + ".png" 
    datFile = dir +"dat/"+ wavFile[pos+1:-4] + "_call_" + f'{int(nr):03d}' 
            
    start = startTime - mrgBefore
    end = startTime + duration + mrgAfter
    
    return wavFile, imgFile, callFile, datFile, start, end

def denoise(data,threshold):
    """
    normalize and remove all the data below a threshold
    
    Arguments:
    data - the data
    threshold - threshold valaue
    """
    n = np.amax(data)
    if(n != 0):
        data /= n
    clean_id = data > threshold
    clean = data * clean_id
    return clean

def createImage(data, imgFile, duration, withAxes = False):
    """
    create an image from fft data
    
    Arguments:
    data - fft data 
    imgFile -  name of the img file to create
    duration - duration of the file [ms]
    """
    plt.clf()
    if not withAxes:
        plt.axis('off') 
    #else:
    #    fig, ax = plt.subplots()
    #    ax.set_xlim(left = 0, right = duration)

    plt.imshow(data, cmap='jet', origin='lower', aspect='equal')  #'auto'
    plt.savefig(imgFile, bbox_inches='tight',pad_inches = 0)
    plt.ioff()
    plt.close('all')

def showInfos(wavFile, callFile, start, end, size):
    print("extract from:", wavFile)
    print("generate: ",callFile + ".wav")
    print("start:", start," end:",end)
    print("size: ", size)

def getMaxima(d, f, b, fname, format = 'npy'):
    """
    get maximimum amplitude for each bin and save results to file
    
    Arguments:
    d - 2d array with fft data
    f - 1d array with freq values
    b - 1d array with time bins
    fname - name of dat file to generate (without extension)
    format - file format for data file
    """
    cols = b.shape[0]
    ret = np.zeros((3, cols))
    for c in range(cols):
        max = np.amax(d[:,c])
        ret[0,c] = b[c]
        ret[1,c] = max
        if max > 0:
            fidx = np.argmax(d[:,c])
            ret[2,c] = f[fidx]
        else:
            ret[2,c] = 0
    if format == "csv":
        np.savetxt(fname + "_max.csv",ret, delimiter=';')
    else:
        np.save(fname + "_max.npy",ret)
        
def AudioSegmentToRawData(signal):
    samples = signal.get_array_of_samples()
    if samples.typecode != 'h':
        raise exceptions.SignalProcessingException('Unsupported samples type')
    return np.array(signal.get_array_of_samples(), np.int16)
    
def detectHardClipping(signal, threshold=2):
    """Detects hard clipping.
    Hard clipping is simply detected by counting samples that touch either the
    lower or upper bound too many times in a row (according to |threshold|).
    The presence of a single sequence of samples meeting such property is enough
    to label the signal as hard clipped.
    Args:
      signal: AudioSegment instance.
      threshold: minimum number of samples at full-scale in a row.
    Returns:
      True if hard clipping is detect, False otherwise.
    """
    if signal.channels != 1:
      raise NotImplementedError('mutliple-channel clipping not implemented')
    if signal.sample_width != 2:  # Note that signal.sample_width is in bytes.
      raise exceptions.SignalProcessingException(
          'hard-clipping detection only supported for 16 bit samples')
    samples = AudioSegmentToRawData(signal)
    # Detect adjacent clipped samples.
    samples_type_info = np.iinfo(samples.dtype)
    mask_min = samples == samples_type_info.min
    mask_max = samples == samples_type_info.max
    
    def HasLongSequence(vector, min_legth=threshold):
        """Returns True if there are one or more long sequences of True flags."""
        seq_length = 0
        for b in vector:
            seq_length = seq_length + 1 if b else 0
            if seq_length >= min_legth:
                return True
        return False
    return HasLongSequence(mask_min) or HasLongSequence(mask_max)   
        
def processCalls(csvFile, outDir, format = "npy", verbose = False, withImg = False):
    """
    process all calls in the csv file:
    generate wav file and img file
    Arguments:
    csvFile: name of the csv (;-delimited) file containing the call informations
    outDir: directory to output all the wav and png files
    format - file format "csv, "npy"
    verbose: True - print information about each detected call
    """
    with open(csvFile,  newline='') as f:
        reader = csv.DictReader(f, delimiter=';')
        count = 0
        for row in reader:
            wavFile, imgFile, callFile, datFile, start, end = readFileInfos(row, outDir)
            seg = extractPart(wavFile, callFile, start, end)
            if detectHardClipping(seg):
                print("clipping detected in file:", callFile)
                continue
            data, freq, bins = graph_spectrogram(callFile, imgFile, scale = scaleType, withAxes = withAxes)
            data_denoised = denoise(data, threshold)
            getMaxima(data_denoised, freq, bins, datFile, format = format)
            if withImg:
                createImage(data_denoised, imgFile, end - start, withAxes = withAxes)
            if format == "csv":
                np.savetxt(datFile + "_fft.csv",data_denoised, delimiter=';')
            else:
                np.save(datFile + "_fft.npy",data_denoised)
            if verbose:
                showInfos(wavFile, callFile, start, end, data.size)
           
            count += 1
            if(count % 50) == 1:
                print("nr. of processed calls:", count)

##############
#   main
##############
start_time = time. time()
processCalls(dataFile, callFileDir, dataFormat, verbose = verbose, withImg = withImg)
end_time = time.time()
time_elapsed = (end_time - start_time)
print("time elapsed:", time_elapsed)
