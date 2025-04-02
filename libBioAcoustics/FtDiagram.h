#pragma once
#include "WavFile.h"
#include <vector>
#include "fft.h"
#include "ColorTable.h"

typedef  std::vector<double> tSpec;


class FtDiagram
{
 public:
   FtDiagram() {}
   virtual ~FtDiagram();
   int createPngFromWavPart(const char* name, double startTime, double EndTime, double fMin, double fMax, 
                 int32_t width, int32_t height, double gradientRange, ColorTable* pColorTable);
   int createPngFromWav(const char* name, int32_t width, int32_t height, double gradientRange, ColorTable* pColorTable);

private:
  int createImage(double startTime, double EndTime, int32_t width, int32_t height, double gradientRange, FFT::WIN_TYPE window = FFT::WIN_TYPE::HANN);
  tSpec* generateFft(int idx, int length, int fftWidth, double gradientRange, FFT::WIN_TYPE window = FFT::WIN_TYPE::HANN, bool logarithmic = true);
  int calculateBestFftSize(double tMin, double tMax);
  void discardSpectra();
  void calcMinAmplitude(bool logaritmic, double gradientRange);
  int saveToPng(const char* name, ColorTable* pColTable, int32_t width, int32_t height, double tMin, double tMax, double fMin, double fMax);


private:
  std::vector<tSpec*> _spec;
  WavFile _wav;
  double _maxAmplitude = -150.0;
  double _minAmplitude = 0.0;
};

