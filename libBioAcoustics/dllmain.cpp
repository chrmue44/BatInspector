// dllmain.cpp : Defines the entry point for the DLL application.
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>
#include <vector>
#include "dllmain.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

namespace libBioAcoustics
{
    stDetect* pReturn = nullptr;

    DLL_API void __cdecl releaseMemory()
    {
        if (pReturn != nullptr)
        {
            delete[] pReturn;
            pReturn = nullptr;
        }
    }

    DLL_API int __cdecl threshold_detection(
        short* audio_samples,
        int sample_count,
        int sample_rate,
        int threshold,
        double min_d,
        double max_d,
        double min_TBE,
        double max_TBE,
        double EDG,
        int LPF,
        int HPF,
        double dur_t,
        double snr_t,
        double angl_t,
        int FFT_size,
        double FFT_overlap,
        double start_t,
        double end_t,
        const size_t NWS,
        double KPE,
        double KME
    )
    {
        std::vector<int> audio_vector;
        audio_vector.resize(sample_count);
        for (int i = 0; i < sample_count; i++)
            audio_vector[i] = audio_samples[i];

        std::vector<stEvData>eval_data = threshold_detection_impl(audio_vector,
            sample_rate,
            threshold,
            min_d,
            max_d,
            min_TBE,
            max_TBE,
            EDG,
            LPF,
            HPF,
            dur_t,
            snr_t,
            angl_t,
            FFT_size,
            FFT_overlap,
            start_t,
            end_t,
            NWS,
            KPE,
            KME
        );
        size_t cnt = eval_data.size();
        releaseMemory();
        pReturn = new stDetect(cnt);
        for (size_t i = 0; i < cnt; i++)
        {
            pReturn->Dat[i].bandwidth = eval_data[i].bandwidth;
            pReturn->Dat[i].bin_max_amp = eval_data[i].bin_max_amp;
            pReturn->Dat[i].curve_neg = eval_data[i].curve_neg;
            pReturn->Dat[i].curve_pos_start = eval_data[i].curve_pos_start;
            pReturn->Dat[i].curve_pos_end = eval_data[i].curve_pos_end;
            pReturn->Dat[i].duration = eval_data[i].duration;
            pReturn->Dat[i].event_start = eval_data[i].event_start;
            pReturn->Dat[i].event_end = eval_data[i].event_end;
            pReturn->Dat[i].fc = eval_data[i].fc;
            pReturn->Dat[i].freq_25 = eval_data[i].freq_25;
            pReturn->Dat[i].freq_75 = eval_data[i].freq_75;
            pReturn->Dat[i].freq_bw_knee_fc = eval_data[i].freq_bw_knee_fc;
            pReturn->Dat[i].freq_center = eval_data[i].freq_center;
            pReturn->Dat[i].freq_end = eval_data[i].freq_end;
            pReturn->Dat[i].freq_knee = eval_data[i].freq_knee;
            pReturn->Dat[i].freq_max = eval_data[i].freq_max;
            pReturn->Dat[i].freq_max_amp = eval_data[i].freq_max_amp;
            pReturn->Dat[i].freq_min = eval_data[i].freq_min;
            pReturn->Dat[i].freq_start = eval_data[i].freq_start;
            pReturn->Dat[i].hd = eval_data[i].hd;
            pReturn->Dat[i].kalman_slope = eval_data[i].kalman_slope;
            pReturn->Dat[i].mid_offset = eval_data[i].mid_offset;
            pReturn->Dat[i].pc_freq_max = eval_data[i].pc_freq_max;
            pReturn->Dat[i].pc_freq_max_amp = eval_data[i].pc_freq_max_amp;
            pReturn->Dat[i].pc_freq_min = eval_data[i].pc_freq_min;
            pReturn->Dat[i].pc_knee = eval_data[i].pc_knee;
            pReturn->Dat[i].slope = eval_data[i].slope;
            pReturn->Dat[i].smoothness = eval_data[i].smoothness;
            pReturn->Dat[i].snr = eval_data[i].snr;
            pReturn->Dat[i].starting_time = eval_data[i].starting_time;
            pReturn->Dat[i].temp_bw_knee_fc = eval_data[i].temp_bw_knee_fc;
        }
        return (int)cnt;
    }

    DLL_API stRetEvalData* __cdecl  getEvalItem(int i)
    {
        if (pReturn != nullptr)
        {
            if (pReturn->Cnt > 0)
            {
              if ((i >= 0) && (i < pReturn->Cnt))
                return &pReturn->Dat[i];
            }
        }
        return nullptr;
    }
}