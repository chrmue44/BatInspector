from scipy.io import wavfile
import sys
import noisereduce as nr
# load data
wavName = sys.argv[1]
outName = sys.argv[2]
rate, data = wavfile.read(wavName)
# perform noise reduction
reduced_noise = nr.reduce_noise(y=data, sr=rate, freq_mask_smooth_hz=1000)
wavfile.write(outName, rate, reduced_noise)