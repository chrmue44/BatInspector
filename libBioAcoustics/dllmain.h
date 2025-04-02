#pragma once
#pragma once
#include <vector>
#include "bat_bioacoustics.h"


#ifdef LIBBIOACOUSTICS_EXPORTS
#define DLL_API __declspec(dllexport)
#else
#define DLL_API __declspec(dllimport)
#endif
namespace libBioAcoustics
{
    extern "C" DLL_API int  __cdecl threshold_detection(
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
    );

    extern "C" DLL_API int __cdecl  getFft(unsigned int size, FFT::WIN_TYPE win);

    extern "C" DLL_API void __cdecl  releaseMemory();

    extern "C" DLL_API stRetEvalData * __cdecl getEvalItem(int i);

    extern "C" DLL_API void __cdecl calcFftDouble(int handle, double* samples, double** spectrum);

    extern "C" DLL_API void __cdecl calcFftComplexOut(int handle, double* samples, double** spectrum);
    /*
     * calculate inverse fft with complex input
     */
    extern "C" DLL_API void __cdecl calcFftInverseComplex(int handle, double* spectrum, double** samples);

    /*
     * calculate inverse fft with real input
     */
    extern "C" DLL_API void __cdecl calcFftInverse(int handle, double* spectrum, double** samples);

    extern "C" DLL_API int getFftSize(int handle);

    extern "C" DLL_API int __cdecl makePngFromWavPart(const char* fileName, double startTime, double endTime, double fMin, double fMax, int width, int height, double gradientRange);
    
    extern "C" DLL_API int __cdecl makePngFromWav(const char* fileName, int width, int height, double gradientRange);

    extern "C" DLL_API void __cdecl setColorGradient(int* colorTable);
}