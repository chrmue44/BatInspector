//------------------------------------------------------------------------------
//  Copyright (C) 2017-2019 WavX, inc. (www.wavx.ca)
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

#include "Rcpp.h"
#include <algorithm>
#include <cmath>
#include <math.h>
#include <string>
#include <vector>
#include <numeric>
#include "fft.h"

FFT::FFT() {}

FFT::~FFT() { fftw_destroy_plan(m_plan); }

FFT::FFT(size_t fft_sz, FFT::WIN_TYPE win_type)
{
  m_fftSize = fft_sz;
  m_winType = win_type;
  set_window(win_type);
  set_plan(fft_sz);
}

void FFT::set_plan(const size_t &fft_sz)
{
  m_original.resize(fft_sz, 0);
  m_transformed.resize(fft_sz, 0);
  m_magnitude.resize(fft_sz / 2, 0);
  m_plan = fftw_plan_r2r_1d(fft_sz, &m_original[0], &m_transformed[0], FFTW_R2HC, FFTW_ESTIMATE);
  m_planInv = fftw_plan_r2r_1d(fft_sz, &m_original[0], &m_transformed[0], FFTW_HC2R, FFTW_ESTIMATE);
}

void FFT::set_window(const WIN_TYPE& win_type)
{
  m_window.resize(m_fftSize, 0);
  m_z = M_PI / (m_fftSize-1);

  switch(win_type)
  {
  case WIN_TYPE::BLACKMAN_HARRIS_4 :
    blackman_harris_4 (m_fftSize);
    break;
  case WIN_TYPE::BLACKMAN_HARRIS_7 :
    blackman_harris_7 (m_fftSize);
    break;
  case WIN_TYPE::HANN :
    hann (m_fftSize);
    break;
  case WIN_TYPE::NONE:
    none(m_fftSize);
    break;
  }

  double sum { std::accumulate(m_window.begin(), m_window.end(), 0.0) };
  m_normalise = 1. / sum;
}

// FFT windows
FFT::WIN_TYPE fft_win_str_to_enum(std::string s)
{
  std::transform(s.begin(), s.end(), s.begin(), ::tolower);
  FFT::WIN_TYPE win_type;

  if (s == "blackman4")
  {
    win_type = FFT::WIN_TYPE::BLACKMAN_HARRIS_4;
  }
  if (s == "blackman7")
  {
    win_type = FFT::WIN_TYPE::BLACKMAN_HARRIS_7;
  }

  if (s == "hann")
  {
    win_type = FFT::WIN_TYPE::HANN;
  }
  else
    win_type = FFT::WIN_TYPE::NONE;
  return win_type;
}

void FFT::blackman_harris_4 (size_t fft_sz)
{
  for (size_t i = 0; i < fft_sz; i++)
  {
    m_window[i] = 0.35875 - 0.48829 * std::cos(2*m_z*i) + \
      0.14128 * std::cos(4*m_z*i) -                   \
      0.01168 * std::cos(6*m_z*i);
  }
}


void FFT::blackman_harris_7 (size_t fft_sz)
{
  // doi:10.1109/icassp.2001.940309
  for (size_t i = 0; i < fft_sz; i++)
  {
    m_window[i] = 0.2712203606 - 0.4334446123 * std::cos(2*m_z*i) +      \
      0.21800412 * std::cos(4*m_z*i) - 0.0657853433 * std::cos(6*m_z*i) +   \
      0.0107618673 * std::cos(8*m_z*i) - 0.0007700127 * std::cos(10*m_z*i) + \
      0.00001368088 * std::cos(12*m_z*i);
  }
}

void FFT::hann (size_t fft_sz)
{
  for (size_t i = 0; i < fft_sz; i++)
  {
    m_window[i] = 0.5 * (1 - std::cos(2*m_z*i));
  }
  if (fft_sz >= 2)
  {
    m_window[0] = m_window[1];
    m_window[fft_sz - 1] = m_window[fft_sz - 2];
  }
 }

void FFT::none(size_t fft_sz)
{
  for (size_t i = 0; i < fft_sz; i++)
  {
    m_window[i] = 1.0;
  }
}


void FFT::implForwardInt(std::size_t seek, const std::vector<int>& samples)
{
  size_t N = samples.size();

  std::fill(m_original.begin(), m_original.end(), 0.0);
  std::fill(m_transformed.begin(), m_transformed.end(), 0.0);

  for (size_t i = 0; i < m_fftSize; i++, seek++)
  {
    if (seek < N)
    {
      m_original[i] = m_window[i] * samples[seek];
    }
  }

  fftw_execute(m_plan);
  size_t sk = m_fftSize;

  for (size_t i = 0; i < m_fftSize / 2; i++)
  {
    m_magnitude[i] = std::abs(std::complex<double>(m_transformed[i], m_transformed[--sk])) * m_normalise;
  }
}

void FFT::implForwardDouble(std::size_t seek, const std::vector<double>& samples)
{
  size_t N = samples.size();

  std::fill(m_original.begin(), m_original.end(), 0.0);
  std::fill(m_transformed.begin(), m_transformed.end(), 0.0);

  for (size_t i = 0; i < m_fftSize; i++, seek++)
  {
    if (seek < N)
    {
      m_original[i] = m_window[i] * samples[seek];
    }
  }

  fftw_execute(m_plan);
  size_t sk = m_fftSize;

  for (size_t i = 0; i < m_fftSize / 2; i++)
  {
    m_magnitude[i] = std::abs(std::complex<double>(m_transformed[i], m_transformed[--sk])) * m_normalise;
  }
}


void FFT::implReverse(std::size_t seek, const std::vector<double>& spectrum)
{
  size_t N = spectrum.size();

  std::fill(m_original.begin(), m_original.end(), 0.0);
  std::fill(m_transformed.begin(), m_transformed.end(), 0.0);

  for (size_t i = 0; i < m_fftSize; i++, seek++)
  {
    if (seek < N)
    {
      m_original[i] = spectrum[seek];
    }
  }

  fftw_execute(m_planInv);
  for (size_t i = 0; i < m_fftSize; i++, seek++)
  {
    m_transformed[i] = m_transformed[i] / m_window[i];
  }
}
