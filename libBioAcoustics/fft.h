//------------------------------------------------------------------------------
//  Copyright (C) 2017-2018 WavX, inc. (www.wavx.ca)
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with This program. If not, see <https://www.gnu.org/licenses/>.
//------------------------------------------------------------------------------

#ifndef FFT_H
#define FFT_H

#include <algorithm>
#include <cmath>
#include <complex>
#include <string>
#include <stddef.h>
#include <vector>
#include "fftw3.h"
#include "Rcpp.h"

// [[Rcpp::plugins(cpp11)]]

class FFT
{
public:
  enum class WIN_TYPE {
    BLACKMAN_HARRIS_4 = 0,
    BLACKMAN_HARRIS_7 = 1,
    HANN = 2,
    NONE = 3
  };

  FFT(size_t fft_sz, WIN_TYPE win_type);
  ~FFT();
  FFT();

  std::vector<double> m_magnitude, m_original, m_transformed;

  void implForwardInt(std::size_t seek, const std::vector<int> &samples);
  void implForwardDouble(std::size_t seek, const std::vector<double>& samples);
  void implReverse(std::size_t seek, const std::vector<double>& samples);
  void set_plan(const size_t &fft_sz);
  void set_window(const FFT::WIN_TYPE& win_type);
  double getNormFactor() { return m_normalise; }
  std::size_t getSize() { return m_fftSize; }
  WIN_TYPE getWinType() { return m_winType; }
  bool inUse() { return m_inUse; }
  void lock() { m_inUse = true; }
  void unlock() { m_inUse = false; }


private:
  double m_normalise, m_z;
  std::vector<double> m_window;
  std::size_t m_fftSize;
  fftw_plan m_plan;
  fftw_plan m_planInv;
  WIN_TYPE m_winType;
  bool m_inUse = false;
  void blackman_harris_4 (const size_t fft_sz);
  void blackman_harris_7 (const size_t fft_sz);
  void hann (const size_t fft_sz);
  void none(const size_t fft_sz);
};



FFT::WIN_TYPE fft_win_str_to_enum(std::string s);

#endif // FFT_H
