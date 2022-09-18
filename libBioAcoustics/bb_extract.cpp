//------------------------------------------------------------------------------
//  Copyright (C) 2012 Chris Scott (fbscds@gmail.com)
//  Copyright (C) 2017-2018 WavX, inc. (www.wavx.ca)
//  Copyright (C) 2022, Christian Mueller (christian (AT) chrmue (DOT) de)
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
#include <cmath>
#include "Rcpp.h"
#include "bb_extract.h"
#include "bb_kalman.h"
#include "bb_audio_event.h"
#include "bb_tools.h"
#include "linear_model.h"
#include "tools.h"

void extract_impl (Audio_Event &audio_event,
                   const size_t &sample_rate,
                   const size_t &fft_size,
                   const size_t &step,
                   std::vector<stEvData> &out,
                   const size_t &index,
                   const double KPE,
                   const double KME)
{
  Kalman freq_track (KPE, KME, audio_event.freq_track[0]);
  Kalman amp_track (KPE, KME, audio_event.amp_track[0]);
  Kalman harmonic_amp_track (KPE, KME, audio_event.harmonic_amp_track[0]);

  freq_track.p_error_prev = 0;
  freq_track.data.clear();
  freq_track.data.resize(audio_event.freq_track.size());
  freq_track.data[0] = freq_track.p_state_prev;

  amp_track.p_error_prev = 0;
  amp_track.data.clear();
  amp_track.data.resize(audio_event.amp_track.size());
  amp_track.data[0] = amp_track.p_state_prev;

  harmonic_amp_track.p_error_prev = 0;
  harmonic_amp_track.data.clear();
  harmonic_amp_track.data.resize(audio_event.harmonic_amp_track.size());
  harmonic_amp_track.data[0] = harmonic_amp_track.p_state_prev;


  for (int i = 1; i < audio_event.freq_track.size(); i++)
  {
    freq_track.impl(audio_event.freq_track[i]);
    freq_track.data[i] = freq_track.p_state_prev;

    amp_track.impl(audio_event.amp_track[i]);
    amp_track.data[i] = amp_track.p_state_prev;

    harmonic_amp_track.impl(audio_event.harmonic_amp_track[i]);
    harmonic_amp_track.data[i] = harmonic_amp_track.p_state_prev;
  }

  double bin2freq = (double)sample_rate / (double)(fft_size);

  std::transform(freq_track.data.begin(), freq_track.data.end(), freq_track.data.begin(), std::bind(std::multiplies<double>(), bin2freq, std::placeholders::_1));
  out[index].freq_track = freq_track.data;
  out[index].amp_track = amp_track.data;
  out[index].starting_time = audio_event.start;

  double to_ms = 1000 * step / (double)sample_rate;
  double duration = audio_event.duration * to_ms;

  out[index].duration = duration;
  out[index].snr = to_dB(audio_event.signal / no_zero(audio_event.noise));
  out[index].hd = to_dB(
  std::accumulate(harmonic_amp_track.data.begin(), harmonic_amp_track.data.end(), (double)0) /
      no_zero(std::accumulate(amp_track.data.begin(), amp_track.data.end(), (double)0)));

  std::vector<double> power_spectrum;
  size_t target_size = 512;

  if (audio_event.power_spectrum.size() < target_size)
  {
    power_spectrum = up_sample(audio_event.power_spectrum, target_size);
  }
  else
  {
    power_spectrum = audio_event.power_spectrum;
  }

  size_t peak = std::distance(power_spectrum.begin(), std::max_element(power_spectrum.begin(), power_spectrum.end()));
  out[index].bin_max_amp = quad_interp(power_spectrum, peak) * (double)sample_rate / (double)(target_size * 2);

  size_t pos_max_amp = std::distance(amp_track.data.begin(), std::max_element(amp_track.data.begin(), amp_track.data.end()));
  auto bin_max_freq = std::max_element(freq_track.data.begin(), freq_track.data.end());
  auto bin_min_freq = std::min_element(freq_track.data.begin(), freq_track.data.end());

  out[index].freq_max_amp = freq_track.data[pos_max_amp];

  out[index].freq_max = *bin_max_freq;
  out[index].freq_min = *bin_min_freq;
  double bandwidth = (*bin_max_freq - *bin_min_freq);
  out[index].bandwidth = bandwidth;
  out[index].freq_start = freq_track.data[0];
  out[index].freq_25 = freq_track.data[audio_event.duration / 4];
  out[index].freq_center = freq_track.data[audio_event.duration / 2];
  out[index].freq_75 = freq_track.data[audio_event.duration * 3 / 4];
  out[index].freq_end = freq_track.data.back();

  double slope = fft_size / 2 * 1000, y, z = 0;
  int j = 0, k = 0;

  size_t win_size = 5, freq_track_win_size = 0;
  std::vector<double> freq_track_win(win_size);
  auto insert_at = freq_track_win.begin();

  for (size_t i = 1; i < freq_track.data.size(); i++)
  {
    if (insert_at == freq_track_win.end())
    {
      insert_at = freq_track_win.begin();
    }

    y = std::abs(freq_track.data[i] - freq_track.data[i-1]);

    // Fknee
    if (z < y)
    {
      z = y;
      j = (int)i;
    }

    *insert_at = y;
    insert_at++;

    if (freq_track_win_size < win_size) freq_track_win_size++;

    double mean = 0;
    for (size_t j = 0; j < freq_track_win_size; j++)
    {
      mean += freq_track_win[j];
    }
    mean /= (double)freq_track_win_size;

    // Fc
    if (i > (freq_track.data.size() * 0.6) && mean < slope)
    {
      slope = mean;
      k = (int)i;
    }
  }

  out[index].freq_knee = freq_track.data[j];
  out[index].fc = freq_track.data[k]; // characteristic frequency
  out[index].freq_bw_knee_fc = freq_track.data[j] - freq_track.data[k];
  out[index].temp_bw_knee_fc = (j - k) * to_ms;
  out[index].pc_freq_max_amp = (double)pos_max_amp / audio_event.duration * 100;
  out[index].pc_freq_max = (double)std::distance(freq_track.data.begin(), bin_max_freq) / audio_event.duration * 100;
  out[index].pc_freq_min = (double)std::distance(freq_track.data.begin(), bin_min_freq) / audio_event.duration * 100;
  out[index].pc_knee = (double)j / audio_event.duration * 100;
  out[index].slope = bandwidth / duration;
  std::vector<int> zero2n (freq_track.data.size());
  std::iota(zero2n.begin(), zero2n.end(), 0);
  std::vector<double> lm_fit = linear_model(zero2n, freq_track.data);
  std::vector<double> tmp (freq_track.data.size());
  std::transform(freq_track.data.begin(), freq_track.data.end(), lm_fit.begin(), tmp.begin(), std::minus<double>());

  double curve_neg = 0, curve_pos_start = 0, curve_pos_end = 0;

  for(size_t i = 0; i < tmp.size(); i++)
  {
    if (tmp[i] > 0)
    {
      curve_neg += tmp[i];
    }
    else
    {
      if (i < freq_track.data.size() / 2)
      {
        curve_pos_start += tmp[i];
      }
      else
      {
        curve_pos_end += tmp[i];
      }
    }
  }

  out[index].curve_neg = curve_neg;
  out[index].curve_pos_start = curve_pos_start;
  out[index].curve_pos_end = curve_pos_end;
  out[index].mid_offset = (sample_at(lm_fit, .5) - sample_at(freq_track.data, .5));
  out[index].kalman_slope = (sample_at(lm_fit, 0) - sample_at(lm_fit, 1)) / duration;
  out[index].smoothness = smoothness(freq_track.data);

  out[index].event_start = audio_event.start + 1; // R index starts at 1
  out[index].event_end = audio_event.end + 1;     // R index starts at 1
}
