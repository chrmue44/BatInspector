#pragma once
#include <vector>
#include "bb_extract.h"

struct stRetEvalData
{
    double duration;
    double freq_max_amp;
    double freq_max;
    double freq_min;
    double bandwidth;
    double freq_start;
    double freq_25;
    double freq_center;
    double freq_75;
    double freq_end;
    double freq_knee;
    double fc;
    double freq_bw_knee_fc;
    double bin_max_amp;
    double pc_freq_max_amp;
    double pc_freq_max;
    double pc_freq_min;
    double pc_knee;
    double temp_bw_knee_fc;
    double slope;
    double kalman_slope;
    double curve_neg;
    double curve_pos_start;
    double curve_pos_end;
    double mid_offset;
    double snr;
    double hd;
    double smoothness;
    int event_start;
    int event_end;
    int starting_time;
};


struct stDetect
{
    size_t Cnt;
    stRetEvalData* Dat;
public:
    stDetect(size_t cnt) : Cnt(cnt)
    {
        Dat = new stRetEvalData[Cnt];
    }

    virtual ~stDetect()
    {
        delete[] Dat;
    }
};

// [[Rcpp::export]]
//Rcpp::List 
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
);
