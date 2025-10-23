#pragma once
#include <vector>
#include "bb_extract.h"

/*
Feature 	Unit 	Description
starting_time 	sec 	Location of the audio event in the recording
duration 	ms 	Duration of the audio event
freq_max_amp 	Hz 	Frequency of the maximum energy of the audio event
freq_max 	Hz 	Highest frequency of the audio event
freq_min 	Hz 	Lowest frequency of the audio event
bandwidth 	Hz 	Difference between the highest (freq_max) and lowest (freq_min) frequencies
freq_start 	Hz 	Frequency at the start of the audio event
freq_center 	Hz 	Frequency at the half of the audio event
freq_end 	Hz 	Frequency at the end of the audio event
freq_knee 	Hz 	Frequency at which the slope is the steepest (knee)
freq_c 	Hz 	Frequency at which the slope is the flatest (caracteristic frequency)
freq_bw_knee_fc 	Hz 	Frequency bandwith between the knee and caracteristic frequency
bin_max_energy 	Hz 	Frequency at the maximum of energy where the slope is the flatest
pc_freq_max_amp 	Hz 	Location of the frequency with the maximum of energy
pc_freq_min 	% 	Location of the minimum frequency
pc_fmax 	% 	Location of the maximum frequency
pc_knee 	% 	Location of the frequency at which the slope is the steepest
temp_bw_knee_fc 	% 	Temporal bandwith between the knee and caracteristic frequency
slope 	ms 	Raw slope estimate (frequency bandwith against duration)
kalman_slope 	Hz / ms 	Smoothed slope estimate after Kalman filtering
curve_pos_start 	Hz / ms 	Slope estimate at the begining of the audio event
curve_pos_end 	Hz / ms 	Slope estimate at the end of the audio event
curve_neg 	Hz / ms 	Slope negative antropy
mid_offset 	dB 	Mid-offset
snr 	dB 	Signal to noise ratio
harmonic_distortion 	dB 	Level of harmonic distortion
smoothness 		Time / frequency regularity*/
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
