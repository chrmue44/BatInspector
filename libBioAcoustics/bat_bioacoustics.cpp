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
#include <algorithm>
#include <deque>
#include <vector>
#include "Rcpp.h"
#include "fft.h"
#include "bb_audio_event.h"
#include "bb_detect.h"
#include "bb_analyse.h"
#include "bb_extract.h"



std::vector<stEvData> threshold_detection_impl(
    const std::vector<int>& audio_samples,
    size_t sample_rate,
    size_t threshold,
    double min_d,
    double max_d,
    double min_TBE,
    double max_TBE,
    double EDG,
    size_t LPF,
    size_t HPF,
    double dur_t,
    double snr_t,
    double angl_t,
    size_t FFT_size,
    double FFT_overlap,
    double start_t,
    double end_t,
    const size_t NWS,
    double KPE,
    double KME
)
{
  std::deque<int> peak_locations;
  std::deque< std::vector<double> > background_noises;
  std::vector<stEvData> retVal;

  FFT fft(FFT_size, FFT::WIN_TYPE::BLACKMAN_HARRIS_7);
  fft.set_plan(FFT_size);
  detect_impl(audio_samples, peak_locations, background_noises, sample_rate, threshold, fft, LPF, HPF, dur_t, EDG, NWS);


  if (peak_locations.empty()) 
      return(retVal);

  size_t duration = 0;
  int end = 0;
  std::vector<Audio_Event> audio_events;

  int step = fft.getSize() * (1 - FFT_overlap);

  std::vector<size_t> to_rm;

  for (size_t i = 0; i < peak_locations.size(); i++)
  {
    if (peak_locations[i] < end) continue;

    Analyse analyse(audio_samples, LPF, HPF, sample_rate, step, start_t, end_t, angl_t, snr_t);
    Audio_Event audio_event = analyse.impl(FFT_size, peak_locations[i], background_noises[i], KPE, KME);

    double d = 1000 * ((double)FFT_size * (1 - FFT_overlap)) / (double)sample_rate * audio_event.duration;

    if(d < min_d || d > max_d){
      to_rm.push_back(i);
      continue;
    }

    if(audio_event.start < (int)end)
    {
      if(audio_event.duration > duration)
      {
        if(!audio_events.empty())
        {
          audio_events.pop_back();
          to_rm.push_back(i-1);
        }
      }
    }

    duration = audio_event.duration;
    end = audio_event.end;
    audio_events.push_back(audio_event);
  }

  // Check for cut signals
  size_t n_events = audio_events.size();
  std::vector<bool> del;
  del.resize(n_events, false);

  if(n_events >= 2)
  {
    for (size_t i = 1; i < n_events; i++)
    {
      if (
          ((audio_events[i].start - audio_events[i - 1].end) / (double)sample_rate * 1000) < min_TBE ||
          ((audio_events[i].start - audio_events[i - 1].end) / (double)sample_rate * 1000) > max_TBE
         )
      {
        del[i - 1] = true;
        del[i] = true;
      }
    }

    for (int i = n_events-1; i >= 0; i--)
    {
      if (del[i])
      {
        audio_events.erase(std::next(audio_events.begin(), i));
        del.erase(std::next(del.begin(), i));
      }
    }
  }
  
  n_events = audio_events.size();

  if (n_events == 0)
    return retVal;

  retVal.resize(n_events);
 
  for(size_t i = 0; i < n_events; i++)
  {
    extract_impl(audio_events[i], sample_rate, FFT_size, step, retVal, i, KPE, KME);
  }

  return retVal;

}


