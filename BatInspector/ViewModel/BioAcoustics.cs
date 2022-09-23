using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
    HANN = 2
  }

  public class BioAcoustics
  {

    static Csv createReport()
    {
      Csv csv = new Csv();
      csv.clear();
      csv.addRow();
      string header = "name;nr;Species;sampleRate;FileLen;freq_max_amp;freq_min;freq_max;freq_knee;duration;start;bandwidth;freq_start;freq_25;freq_center;freq_75;freq_end;fc;freq_bw_knee_fc;bin_max_amp;pc_freq_max_amp;pc_freq_max;pc_freq_min;pc_knee;temp_bw_knee_fc;slope;kalman_slope;curve_neg;curve_pos_start;curve_pos_end;mid_offset;smoothness;snr;SpeciesMan;prob;remarks";
      csv.initColNames(header, true);
      return csv;
    }

    public static void analyzeFiles(string reportName, string path)
    {
      string[] files = Directory.GetFiles(path, "*.wav");
      Csv csv = createReport();
      foreach (string fName in files)
      {
        int sampleRate;
        double duration;
        ThresholdDetectItem[] items = analyzeCalls(fName, out sampleRate, out duration);
        if (items.Length == 0)
        {
          File.Delete(fName);
          string png = fName.Replace(".wav", ".png");
          if (File.Exists(png))
            File.Delete(png);
          string xml = fName.Replace(".wav", ".xml");
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

    static void addToReport(Csv csv, string fName, int samplingRate, double duration, ThresholdDetectItem[] items)
    {
      for(int i = 0; i < items.Length; i++)
      {
        csv.addRow();
        int row = csv.RowCnt;
        csv.setCell(row, "name", fName);
        csv.setCell(row, "nr", i + 1);
        csv.setCell(row, "Species", "----");
        csv.setCell(row, "sampleRate", samplingRate);
        csv.setCell(row, "FileLen", duration);
        csv.setCell(row, "freq_max_amp", items[i].freq_max_amp);
        csv.setCell(row, "freq_min", items[i].freq_min);
        csv.setCell(row, "freq_max", items[i].freq_max);
        csv.setCell(row, "freq_knee", items[i].freq_knee);
        csv.setCell(row, "duration", items[i].duration);
        csv.setCell(row, "start", (double)items[i].start_time/samplingRate);
        csv.setCell(row, "bandwidth", items[i].bandwidth);
        csv.setCell(row, "freq_start", items[i].freq_start);
        csv.setCell(row, "freq_25", items[i].freq_25);
        csv.setCell(row, "freq_center", items[i].freq_center);
        csv.setCell(row, "freq_75", items[i].freq_75);
        csv.setCell(row, "freq_end", items[i].freq_end);
        csv.setCell(row, "fc", items[i].fc);
        csv.setCell(row, "freq_bw_knee_fc", items[i].freq_bw_knee_fc);
        csv.setCell(row, "bin_max_amp", items[i].bin_max_amp);
        csv.setCell(row, "pc_freq_max_amp", items[i].pc_freq_max_amp);
        csv.setCell(row, "pc_freq_max", items[i].pc_freq_max);
        csv.setCell(row, "pc_freq_min", items[i].pc_freq_min);
        csv.setCell(row, "pc_knee", items[i].pc_knee);
        csv.setCell(row, "temp_bw_knee_fc", items[i].temp_bw_knee_fc);
        csv.setCell(row, "slope", items[i].slope);
        csv.setCell(row, "kalman_slope", items[i].kalman_slope);
        csv.setCell(row, "curve_neg", items[i].curve_neg);
        csv.setCell(row, "curve_pos_start", items[i].curve_pos_start);
        csv.setCell(row, "curve_pos_end", items[i].curve_pos_end);
        csv.setCell(row, "mid_offset", items[i].mid_offset);
        csv.setCell(row, "smoothness", items[i].smoothness);
        csv.setCell(row, "snr", items[i].snr);
        csv.setCell(row, "SpeciesMan", "todo");
        csv.setCell(row, "prob", "");
        csv.setCell(row, "remarks", "");
      }
    }

    public static ThresholdDetectItem[]  analyzeCalls(string wavFile, out int sampleRate, out double duration)
    {
      WavFile wav = new WavFile();
      int evCount = 0;
      ThresholdDetectItem[] items = new ThresholdDetectItem[evCount];
      sampleRate = 0;
      duration = 0;

      int ret = wav.readFile(wavFile);

      if (ret == 0)
      {
        sampleRate = (int)wav.SamplingRate;
        duration = wav.AudioSamples.Length / wav.SamplingRate;
        evCount = threshold_detection(
         wav.AudioSamples, wav.AudioSamples.Length, (int)wav.SamplingRate,
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

    public static double[] calculateFft(int size, enWIN_TYPE window, int[] samples)
    {
      double[] retVal = new double[size / 2];
      IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(retVal));
      calcFftInt(size, window, samples, ref buffer);
      Marshal.Copy(buffer, retVal, 0, size);
      Marshal.FreeCoTaskMem(buffer);
      return retVal;
    }

    public static double[] calculateFft(int size, enWIN_TYPE window, double[] samples)
    {
      double[] retVal = new double[size / 2];
      IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(retVal[0]) * size/2 + 100);
      calcFftDouble(size, window, samples, ref buffer);
      Marshal.Copy(buffer, retVal, 0, size/2);
      Marshal.FreeCoTaskMem(buffer);
      return retVal;
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
    static extern void calcFftInt(int size, enWIN_TYPE win, int[] samples, ref IntPtr spectrum);

    [DllImport("libBioAcoustics.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern void calcFftDouble(int size, enWIN_TYPE win, double[] samples, ref IntPtr spectrum);
  }
}
