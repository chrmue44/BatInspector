/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using libScripter;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;


namespace BatInspector
{

  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct ThresholdDetectItem
  {
    public double duration;
    public double freq_max_amp;
    public double freq_max;
    public double freq_min;
    public double bandwidth;
    public double freq_start;
    public double freq_25;
    public double freq_center;
    public double freq_75;
    public double freq_end;
    public double freq_knee;
    public double fc;
    public double freq_bw_knee_fc;
    public double bin_max_amp;
    public double pc_freq_max_amp;
    public double pc_freq_max;
    public double pc_freq_min;
    public double pc_knee;
    public double temp_bw_knee_fc;
    public double slope;
    public double kalman_slope;
    public double curve_neg;
    public double curve_pos_start;
    public double curve_pos_end;
    public double mid_offset;
    public double snr;
    public double hd;
    public double smoothness;
    public int event_start;
    public int event_end;
    public int start_time;
  }

  

  public enum enWIN_TYPE
  {
    BLACKMAN_HARRIS_4 = 0,
    BLACKMAN_HARRIS_7 = 1,
    HANN = 2,
    NONE = 3,
  }

  public class BioAcoustics
  {
    static object _locker = new object();
    static object _pngLocker = new object();

    [HandleProcessCorruptedStateExceptions]
   // [SecurityCritical]
    public static void analyzeFiles(string reportName, string path)
    {
      string[] files = Directory.GetFiles(path, "*.wav");
      Csv csv = ModelCmuTsa.createReport();
      foreach (string fName in files)
      {
        ThresholdDetectItem[] items = analyzeCalls(fName, out int sampleRate, out double duration);
        if (items.Length == 0)
        {
          File.Delete(fName);
          string png = fName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
          if (File.Exists(png))
            File.Delete(png);
          string xml = fName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
          if (File.Exists(xml))
            File.Delete(xml);
          DebugLog.log(fName + " deleted, no calls", enLogType.INFO);
        }
        else
        {
          addToReport(csv, fName, sampleRate, duration, items);
          DebugLog.log(fName + ": " + items.Length.ToString() + " calls", enLogType.INFO);
        }
      }
      csv.saveAs(reportName);
    }

    [HandleProcessCorruptedStateExceptions]
   // [SecurityCritical]
    static void addToReport(Csv csv, string fName, int samplingRate, double duration, ThresholdDetectItem[] items)
    {
      for (int i = 0; i < items.Length; i++)
      {
        csv.addRow();
        int row = csv.RowCnt;
        csv.setCell(row, Cols.NAME, fName);
        csv.setCell(row, Cols.NR, i + 1);
        csv.setCell(row, Cols.SPECIES, "----");
        csv.setCell(row, Cols.SAMPLERATE, samplingRate);
        csv.setCell(row, Cols.FILE_LEN, duration);
        csv.setCell(row, Cols.F_MAX_AMP, items[i].freq_max_amp);
        csv.setCell(row, Cols.F_MIN, items[i].freq_min);
        csv.setCell(row, Cols.F_MAX, items[i].freq_max);
        csv.setCell(row, Cols.F_KNEE, items[i].freq_knee);
        csv.setCell(row, Cols.DURATION, items[i].duration);
        csv.setCell(row, Cols.START_TIME, (double)items[i].start_time / samplingRate);
        csv.setCell(row, Cols.BANDWIDTH, items[i].bandwidth);
        csv.setCell(row, Cols.F_START, items[i].freq_start);
        csv.setCell(row, Cols.F_25, items[i].freq_25);
        csv.setCell(row, Cols.F_CENTER, items[i].freq_center);
        csv.setCell(row, Cols.F_75, items[i].freq_75);
        csv.setCell(row, Cols.F_END, items[i].freq_end);
        csv.setCell(row, Cols.FC, items[i].fc);
        csv.setCell(row, Cols.F_BW_KNEE_FC, items[i].freq_bw_knee_fc);
        csv.setCell(row, Cols.BIN_MAX_AMP, items[i].bin_max_amp);
        csv.setCell(row, Cols.PC_F_MAX_AMP, items[i].pc_freq_max_amp);
        csv.setCell(row, Cols.PC_F_MAX, items[i].pc_freq_max);
        csv.setCell(row, Cols.PC_F_MIN, items[i].pc_freq_min);
        csv.setCell(row, Cols.PC_KNEE, items[i].pc_knee);
        csv.setCell(row, Cols.TEMP_BW_KNEE_FC, items[i].temp_bw_knee_fc);
        csv.setCell(row, Cols.SLOPE, items[i].slope);
        csv.setCell(row, Cols.KALMAN_SLOPE, items[i].kalman_slope);
        csv.setCell(row, Cols.CURVE_NEG, items[i].curve_neg);
        csv.setCell(row, Cols.CURVE_POS_START, items[i].curve_pos_start);
        csv.setCell(row, Cols.CURVE_POS_END, items[i].curve_pos_end);
        csv.setCell(row, Cols.MID_OFFSET, items[i].mid_offset);
        csv.setCell(row, Cols.SMOTTHNESS, items[i].smoothness);
        csv.setCell(row, Cols.SNR, items[i].snr);
        csv.setCell(row, Cols.SPECIES_MAN, "todo");
        csv.setCell(row, Cols.PROBABILITY, "");
        csv.setCell(row, Cols.REMARKS, "");
      }
    }

    [HandleProcessCorruptedStateExceptions]
   // [SecurityCritical]
    public static ThresholdDetectItem[] analyzeCalls(string wavFile, out int sampleRate, out double duration)
    {
      WavFile wav = new WavFile();
      int evCount = 0;
      ThresholdDetectItem[] items = new ThresholdDetectItem[evCount];
      sampleRate = 0;
      duration = 0;

      int ret = wav.readFile(wavFile);

      if (ret == 0)
      {
        sampleRate = (int)wav.FormatChunk.Frequency;
        duration = wav.AudioSamples.Length / wav.FormatChunk.Frequency;
        evCount = threshold_detection(
         wav.AudioSamples, wav.AudioSamples.Length, (int)wav.FormatChunk.Frequency,
          5,   // threshold
          1.5, // min_d
          80,  // max_d
          30, //min_TBE
          1e100,  //max_TBE
          0.996,  //EDG
          120000, //LPF
          10000,  //HPF
          80,    //dur_t
          5,     //snr_t
          125,   //angl_t
          256,   //FFT_size
          0.875,   //FFT_overlap
          30,      // start_t
          35,      // end_t
          100,     // NWS
          1e-5,    //KPE
          1e-4     //KME
          );
        items = new ThresholdDetectItem[evCount];
      }
      for (int i = 0; i < evCount; i++)
      {
        IntPtr Ptr = getEvalItem(i);
        unsafe
        {
          ThresholdDetectItem* pItem = (ThresholdDetectItem*)Ptr;
          if (pItem != null)
            items[i] = *pItem;
        }
      }
      releaseMemory();
      return items;
    }

    //https://learn.microsoft.com/de-de/dotnet/framework/interop/marshalling-different-types-of-arrays
    [HandleProcessCorruptedStateExceptions]
   // [SecurityCritical]
    public static double[] calculateFft(int size, enWIN_TYPE window, int[] samples)
    {
      lock (_locker)
      {
        double[] retVal = new double[size / 2];
        try
        {
          int handle = getFft((uint)size, window);
          IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(retVal));
          calcFftInt(handle, samples, ref buffer);
          Marshal.Copy(buffer, retVal, 0, size);
          Marshal.FreeCoTaskMem(buffer);
        }
        catch (Exception ex)
        {
          DebugLog.log("calculateFft: " + ex.ToString(), enLogType.ERROR);
        }
        return retVal;
      }
    }

    [HandleProcessCorruptedStateExceptions]
   // [SecurityCritical]
    public static double[] calculateFft(int handle, double[] samples)
    {
      lock (_locker)
      {
        int size = getFftSize(handle);
        double[] retVal = new double[size / 2];
        if (size > 2)
        {
          try
          {
            IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(retVal[0]) * size / 2 + 100);
            calcFftDouble(handle, samples, ref buffer);
            Marshal.Copy(buffer, retVal, 0, size / 2);
            Marshal.FreeCoTaskMem(buffer);
          }
          catch (Exception ex)
          {
            DebugLog.log("calculateFft: " + ex.ToString(), enLogType.ERROR);
          }
        }
        else
          DebugLog.log($"calculateFft: size < 2, handle: {handle}", enLogType.ERROR);
        return retVal;
      }
    }

    [HandleProcessCorruptedStateExceptions]
//    [SecurityCritical]
    public static double[] calculateFftComplexOut(int handle, double[] samples)
    {
      lock (_locker)
      {
        int size = getFftSize(handle);
        double[] retVal = new double[size];
        try
        {
          IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(retVal[0]) * size + 100);
          calcFftComplexOut(handle, samples, ref buffer);
          Marshal.Copy(buffer, retVal, 0, size);
          Marshal.FreeCoTaskMem(buffer);
        }
        catch (Exception ex)
        {
          DebugLog.log("calculateFftComplexOut: " + ex.ToString(), enLogType.ERROR);
        }
        return retVal;
      }
    }

    [HandleProcessCorruptedStateExceptions]
   // [SecurityCritical]
    public static double[] calculateFftReversed(int handle, double[] spec)
    {
      lock (_locker)
      {
        int size = getFftSize(handle);
        double[] retVal = new double[size];
        try
        {
          IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(retVal[0]) * size + 100);
          calcFftInverseComplex(handle, spec, ref buffer);
          Marshal.Copy(buffer, retVal, 0, size);
          Marshal.FreeCoTaskMem(buffer);
        }
        catch (Exception ex)
        {
          DebugLog.log("calculateFftComplexOut: " + ex.ToString(), enLogType.ERROR);
        }
        return retVal;
      }
    }

    /// <summary>
    /// apply a bandpass filter to a complex spectrum
    /// </summary>
    /// <param name="spectrum">ref to array containing the spectrum</param>
    /// <param name="fMin">min frequency of passband [Hz]</param>
    /// <param name="fMax">max frequency of passband [Hz]</param>
    /// <param name="samplingRate">sampling rate [Hz]</param>
    [HandleProcessCorruptedStateExceptions]
   // [SecurityCritical]
    public static void applyBandpassFilterComplex(ref double[] spectrum, double fMin, double fMax, uint samplingRate)
    {
      lock (_locker)
      {
        try
        {
          int sk = spectrum.Length;
          int iFmin = (int)(fMin / samplingRate * spectrum.Length);
          int iFmax = (int)(fMax / samplingRate * spectrum.Length);
          for (int i = 0; i < spectrum.Length / 2; i++)
          {
            --sk;
            if (i < iFmin)
            {
              spectrum[i] = 0;
              spectrum[sk] = 0;
            }
            else if (i > iFmax)
            {
              spectrum[i] = 0;
              spectrum[sk] = 0;
            }
          }
        }
        catch (Exception ex)
        {
          DebugLog.log("applyBandpassFilterComplex: " + ex.ToString(), enLogType.ERROR);
        }
      }
    }

    [HandleProcessCorruptedStateExceptions]
    public static int createPngFromWavPart(string name, double tStart, double tEnd, double fMin, double fMax, int width, int height, double gradientRange)
    {
      return makePngFromWavPart(name, tStart, tEnd, fMin, fMax, width, height, gradientRange);
    }

    [HandleProcessCorruptedStateExceptions]
    public static int createPngFromWav(string name, int width, int height, double gradientRange)
    {
      lock (_pngLocker)
      {
        return makePngFromWav(name, width, height, gradientRange);
      }
    }

    [HandleProcessCorruptedStateExceptions]
    public static void setColorTable()
    {
      lock (_locker)
      {
        int max = AppParams.Inst.ColorGradientRed.Count;
        int[] colors = new int[3 * max * 2];
        int idx = 0;
        for (int i = 0; i < max; i++)
        {
          colors[idx++] = AppParams.Inst.ColorGradientRed[i].Color;
          colors[idx++] = AppParams.Inst.ColorGradientRed[i].Value;
        }
        for (int i = 0; i < max; i++)
        {
          colors[idx++] = AppParams.Inst.ColorGradientGreen[i].Color;
          colors[idx++] = AppParams.Inst.ColorGradientGreen[i].Value;
        }
        for (int i = 0; i < max; i++)
        {
          colors[idx++] = AppParams.Inst.ColorGradientBlue[i].Color;
          colors[idx++] = AppParams.Inst.ColorGradientBlue[i].Value;
        }
        setColorGradient(colors);
      }
    }

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int threshold_detection(
    short[] audio_samples,
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
    int NWS,
    double KPE,
    double KME);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    //    static public extern ThreshholdDetectItem getEvalItem(int i, int j);
    static extern IntPtr getEvalItem(int i);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern void releaseMemory();

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern void calcFftInt(int handle, int[] samples, ref IntPtr spectrum);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern int getFft(uint size, enWIN_TYPE win);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern void calcFftDouble(int handle, double[] samples, ref IntPtr spectrum);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern void calcFftComplexOut(int handle, double[] samples, ref IntPtr spectrum);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern void calcFftInverse(int handle, double[] spectrum, ref IntPtr samples);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern void calcFftInverseComplex(int handle, double[] spectrum, ref IntPtr samples);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern int getFftSize(int handle);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern int getFtImage(short[] samples, int sampleCnt, string pngName, int fftWidth, int waterFallWidth, int[] colorTable, int colorTableLen);
    //    static public extern int getFtImage(string wavName, string pngName, int fftWidth, int waterFallWidth, int[] colorTable, int colorTableLen);


    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern int makePngFromWavPart(string fileName, double startTime, double endTime, double fMin, double fMax, int width, int height, double gradientRange);
    
    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern int makePngFromWav(string fileName, int width, int height, double gradientRange);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static public extern void  setColorGradient(int[] colorTable);
  }
}
