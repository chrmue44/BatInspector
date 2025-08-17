#include <cstdint>
#include <cmath>
#include <fstream>

#include "FtDiagram.h"
#include "dllmain.h"
#include "fft.h"
#include "ColorTable.h"
#include"lodepng.h"

FtDiagram::~FtDiagram()
{
  discardSpectra();
}

void FtDiagram::discardSpectra()
{
  for (int i = 0; i < _spec.size(); i++)
    delete _spec[i];
  _spec.clear();
}

bool replace(std::string& str, const std::string& from, const std::string& to) 
{
  size_t start_pos = str.find(from);
  if (start_pos == std::string::npos)
    return false;
  str.replace(start_pos, from.length(), to);
  return true;
}

int FtDiagram::createPngFromWav(const char* name, int32_t width, int32_t height, double gradientRange, ColorTable* pColTable)
{
  int retVal = _wav.readFile(name);
  if (retVal != 0)
    return retVal;

  double startTime = 0.0;
  double EndTime = (double)_wav.getSampleCnt() / _wav.getSampleRate();
  double fMin = 0.0;
  double fMax = (double)_wav.getSampleRate() / 2000;

//  createImage(startTime, EndTime, width, height, gradientRange);
  std::string pngName = name;
  replace(pngName, ".wav", ".png");
  replace(pngName, ".WAV", ".png");
  createPngFromWavPart(name, pngName.c_str(), startTime, EndTime, fMin, fMax, width, height, gradientRange, pColTable);
//    saveToPng(pngName.c_str(), pColTable, width, height, startTime, EndTime, fMin, fMax);
  return retVal;
}


int FtDiagram::createPngFromWavPart(const char* wavName, const char* pngName, double startTime, double EndTime, double fMin, double fMax, 
                         int32_t width, int32_t height, 
                         double gradientRange, ColorTable* pColTable)
{
  int retVal = _wav.readFile(wavName);
  if (retVal != 0)
    return retVal;

  createImage(startTime, EndTime, width, height, gradientRange);
  saveToPng(pngName, pColTable, width, height, startTime, EndTime, fMin, fMax);
  return retVal;
}

int FtDiagram::saveToPng(const char* name, ColorTable* pColTable, int32_t width, int32_t height, double tMin, double tMax, double fMin, double fMax)
{
  int retVal = 0;

  int fftBinCnt = calculateBestFftSize(tMin, tMax) / 2;

  std::vector<unsigned char> image;
  image.resize(width * height * 4);

  for (uint16_t y = 0; y < height; y++)
  {
    double f = (fMax - fMin) * y / fftBinCnt + fMin;
    int idxFreq = (int)(f * 2000 / (double)_wav.getSampleRate() * fftBinCnt);
    for (uint16_t x = 0; x < width; x++)
    {
      int idxSpec = (int)((double)_spec.size() / (double)width * (double)x);

      if (_spec[idxSpec] != nullptr)
      {
        if (idxFreq >= _spec[idxSpec]->size())
          idxFreq = _spec[idxSpec]->size() - 1;
        tSpec* spec = _spec[idxSpec];
        double val = (*spec)[idxFreq];
        stColorChans col = pColTable->getColor(val, _minAmplitude, _maxAmplitude);
        uint16_t ys = fftBinCnt - 1 - y;
        image[4 * width * ys + 4 * x + 0] = col.red;
        image[4 * width * ys + 4 * x + 1] = col.green;
        image[4 * width * ys + 4 * x + 2] = col.blue;
        image[4 * width * ys + 4 * x + 3] = 255;
//        bmp.setPixel(x, fftBinCnt - 1 - y, col);
      }
    }
  }

  retVal = lodepng::encode(name, image, width, height);
  return retVal;
}

int FtDiagram::createImage(double startTime, double EndTime,  int32_t width, int32_t height, double gradientRange, FFT::WIN_TYPE window)
{
  int retVal = 0;
  if (_wav.isOpen())
  {
    int fftSize =  calculateBestFftSize(startTime, EndTime);
    int idxStart = (int)(startTime * _wav.getSampleRate());
    if (idxStart > _wav.getSampleCnt())
      idxStart = _wav.getSampleCnt() - 1;
    int idxEnd = (int)(EndTime * _wav.getSampleRate());
    if (idxEnd > _wav.getSampleCnt())
      idxEnd = _wav.getSampleCnt() - 1;
    int step = (idxEnd - idxStart) / (int)width;
    if (step == 0)
      step = 1;
    if (step > fftSize)
      step = fftSize;
    _maxAmplitude = -120.0;

    int max = (int)(idxEnd - idxStart) / (int)step;
    discardSpectra();
    for (int i = 0; i < max; i++)
      _spec.push_back(nullptr);

    //Parallel.For(0, max, i = >
    for (int i = 0; i < max; i++)
    {
      int idx = idxStart + i * step;
      if (idx >= 0)
      {
        tSpec* sp = generateFft(idx, fftSize, height, gradientRange, window );
        _spec[i] = sp;
      }
    }
    //    );
  }
  else
    retVal = 1;
  return retVal;
}


tSpec* FtDiagram::generateFft(int idx, int length, int fftWidth, double gradientRange, FFT::WIN_TYPE window, bool logarithmic)
{
  int zeroPadding = 0; // NOTE: Zero Padding
  if (idx + length >  _wav.getSampleCnt())
    length = _wav.getSampleCnt() - idx - 1;

  if (length <= 0)
    return nullptr;

  zeroPadding = fftWidth - length;
  double* inputSignal = new double[length];
  for (int i = 0; i < length; i++)
    inputSignal[i] = (double)_wav.getSamples()[idx++] / 32768;

  tSpec* lmSpectrum = new tSpec(length, 0.0);
  double wScaleFactor = 1.0;
  double* pData = &((*lmSpectrum)[0]);

  int handle = libBioAcoustics::getFft(length, window);
  libBioAcoustics::calcFftDouble(handle, inputSignal, &pData);
  delete[] inputSignal;
  double s = 0;

  for (int i = 0; i < length; i++)
  {
    if ((*lmSpectrum)[i] > s)
    {
      if (logarithmic)
      {
        (*lmSpectrum)[i] = 20*log10((*lmSpectrum)[i]) * wScaleFactor;
        if ((*lmSpectrum)[i] > _maxAmplitude)
          _maxAmplitude = (*lmSpectrum)[i];
      }
      else
      {
        (*lmSpectrum)[i] *= 100;
        if ((*lmSpectrum)[i] > _maxAmplitude)
          _maxAmplitude = (*lmSpectrum)[i];
      }
    }
    else
      (*lmSpectrum)[i] = logarithmic ? -100 : 0;
  }
  calcMinAmplitude(logarithmic, gradientRange);
  // Properly scale the spectrum for the added window
//      lmSpectrum = DSP.Math.Multiply(lmSpectrum, wScaleFactor);

  return lmSpectrum;
}


void FtDiagram::calcMinAmplitude(bool logaritmic, double gradientRange)
{
  if (logaritmic)
    _minAmplitude = _maxAmplitude - gradientRange;
  else
    _minAmplitude = _maxAmplitude / pow(10, gradientRange / 20);
}

int FtDiagram::calculateBestFftSize(double tMin, double tMax)
{
  int fftSize = 1024;
  double dt = tMax - tMin;
  //      if (dt < 0.07)
  //        fftSize = 256;
  //      else 
  if (dt < 0.15)
    fftSize = 512;
  return fftSize;
}
