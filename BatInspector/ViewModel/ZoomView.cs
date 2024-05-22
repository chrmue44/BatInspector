/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Controls;
using libParser;
using libScripter;
using System;
using System.IO;
using System.Threading;

namespace BatInspector
{
  public class RulerData
  {
    double _min = 0;
    double _max = 100;
    public double Min { get { return _min; } }
    public double Max { get { return _max; } }

    public bool check(double x)
    {
      return ((Min <= x) && (x <= Max));
    }

    public void setRange(double min, double max)
    {
      _min = Math.Min(min, max);
      _max = Math.Max(min, max);
    }

    public void inc(double percent, double max)
    {
      double delta = _max - Min;
      _min += delta * percent;
      _max += delta * percent;
      if(Max > max)
      {
        _max = max;
        _min = Max - delta;
      }
    }
    public void decr(double percent, double min)
    {
      double delta = _max - Min;
      _min -= delta * percent;
      _max -= delta * percent;
      if (Min < min)
      {
        _min = min;
        _max = _min + delta;
      }
    }

    public void limits(double min, double max)
    {
      if (_max > max)
        _max = max;
      if (_min < min)
        _min = min;
    }
  }

  public class Cursor
  {
    double _freq = 0;
    double _time = 0;
    bool _visible = false;

    public double Freq { get { return _freq; } }
    public double Time { get { return _time; } }
    public bool Visible { get { return _visible; } }

    public void set(double t, double f, bool visible)
    {
      _freq = f;
      _time = t;
      _visible = visible;
    }

    public void hide()
    {
      _visible = false;
    }
  }

  public class ZoomView
  {
    RulerData _rulerDataT;
    RulerData _rulerDataF;
    RulerData _rulerDataA;
    Cursor _cursor1;
    Cursor _cursor2;
    Spectrum _spectrum;
    Waterfall _wf = null;
    ColorTable _colorTable;
    BatRecord _fileInfo;
    AnalysisFile _analysis;
    ProcessRunner _proc;
    string _tmp1Wav;
    string _tmp2Wav;
    string _wrkDir;
    ModelState _modelState;

    public ZoomView(ColorTable colorTable, ProcessRunner pr, ModelState modelState)
    {
      _colorTable = colorTable;
      _rulerDataT = new RulerData();
      _rulerDataF = new RulerData();
      _rulerDataA = new RulerData();
      _cursor1 = new Cursor();
      _cursor2 = new Cursor();
      _spectrum = new Spectrum();
      _fileInfo = new BatRecord();
      _proc = pr;
      _analysis = null;
      _modelState = modelState;
      _wrkDir = Path.Combine(AppParams.AppDataPath, AppParams.Inst.ModelRootPath,
                             AppParams.Inst.Models[0].Dir);

      _tmp1Wav = Path.Combine(_wrkDir, "temp1.wav");
      _tmp2Wav = Path.Combine(_wrkDir, "temp2.wav");

    }


    public RulerData RulerDataT { get { return _rulerDataT; } }
    public RulerData RulerDataF { get { return _rulerDataF; } }
    public RulerData RulerDataA { get { return _rulerDataA; } }
    public Cursor Cursor1 { get { return _cursor1; } }
    public Cursor Cursor2 { get { return _cursor2; } }
    public Spectrum Spectrum { get { return _spectrum; } }
    public BatRecord FileInfo {  get { return  _fileInfo; } }

    public Waterfall Waterfall {  get { return _wf; } }

    public AnalysisFile Analysis { get { return _analysis; } set { _analysis = value; } }

    public int SelectedCallIdx { get; set; }
    public bool RefreshZoomImg { get; set; } = false;

    public void initWaterfallDiagram(string wavName)
    {
      _wf = new Waterfall(wavName, _colorTable);
      string infoName = wavName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_INFO);
      _fileInfo = ElekonInfoFile.read(infoName);
    }

    public void zoomInV()
    {
      double max = (_rulerDataF.Max - _rulerDataF.Min) / 2 + _rulerDataF.Min;
      double min = _rulerDataF.Min;
      _rulerDataF.setRange(min, max);
    }
    public void zoomOutV()
    {
      double max = (_rulerDataF.Max - _rulerDataF.Min) * 2 + _rulerDataF.Min;
      double min = _rulerDataF.Min;
      _rulerDataF.setRange(min, max);
    }

    public void zoomInH()
    {
      double max, min;
      if (AppParams.Inst.ZoomType == enZoomType.LEFT)
      {
        max = (_rulerDataT.Max - _rulerDataT.Min) / 2 + _rulerDataT.Min;
        min = _rulerDataT.Min;
      }
      else
      {
        max = _rulerDataT.Max - (_rulerDataT.Max - _rulerDataT.Min) / 4;
        min = _rulerDataT.Min + (_rulerDataT.Max - _rulerDataT.Min) / 4;
      }
      _rulerDataT.setRange(min, max);
    }
    public void zoomOutH()
    {
      double max, min;
      if (AppParams.Inst.ZoomType == enZoomType.LEFT)
      {
        max = (_rulerDataT.Max - _rulerDataT.Min) * 2 + _rulerDataT.Min;
        min = _rulerDataT.Min;
      }
      else
      {
        max = _rulerDataT.Max + (_rulerDataT.Max - _rulerDataT.Min) / 4;
        min = _rulerDataT.Min - (_rulerDataT.Max - _rulerDataT.Min) / 4;
      }
      _rulerDataT.setRange(min, max);
    }

    public bool moveLeft()
    {
      bool retVal = false;
      if (_rulerDataT.Min > 0)
      {
        retVal = true;
        _rulerDataT.decr(0.25, 0);
      }
      return retVal;
    }

    public bool moveRight(double max)
    {
      bool retVal = false;
      if (_rulerDataT.Max < max)
      {
        retVal = true;
        _rulerDataT.inc(0.25, max);
      }
      return retVal;
    }

    public bool moveUp(double max)
    {
      bool retVal = false;
      if (_rulerDataF.Max < max)
      {
        retVal = true;
        _rulerDataF.inc(0.25, max);
      }
      return retVal;
    }

    public bool moveDown(double min)
    {
      bool retVal = false;
      if (_rulerDataF.Min > min)
      {
        retVal = true;
        _rulerDataF.decr(0.25, min);
      }
      return retVal;
    }



    public void findMaxAmplitude()
    {
      if(_wf != null)
      {
        int iMin = (int)(_rulerDataT.Min * _wf.SamplingRate);
        int iMax = (int)(_rulerDataT.Max * _wf.SamplingRate);
        double max = 0;
        for(int i = iMin; i < iMax; i++)
        {
          double abs = Math.Abs(_wf.Audio.Samples[i]);
          if (max < abs)
            max = abs;
        }
        _rulerDataA.setRange(-max, max);
      }
    }

    public void applyBandpass(double fMin, double fMax)
    {
      _wf.Audio.FftForward();
      _wf.Audio.bandpass(fMin, fMax);
      _wf.Audio.FftBackward();
    }

    public void removeSection(double tMin, double tMax)
    {
      _wf.Audio.removeSection(tMin, tMax);
    }

    public void normalize()
    {
      _wf.Audio.normalize();
    }

    public void reduceNoise()
    {
 /*     _wf.Audio.FftForward();
      _wf.Audio.reduceNoise(-15);
      _wf.Audio.FftBackward();
*/
      try
      {
        _modelState.State = enAppState.TOOL_RUNNING;
        _modelState.Msg = BatInspector.Properties.MyResources.ZoomViewMsgNoiseReduction;       
        _wf.Audio.saveAs(_tmp1Wav, "");
        string exe = Path.Combine(_wrkDir,AppParams.CMD_REDUCE_NOISE);
        string args = _tmp1Wav + " " + _tmp2Wav;
        _proc.launchCommandLineApp(exe, null, _wrkDir, false, args, false, false, onExitNoiseReduction);
      }
      catch (Exception ex)
      {
        DebugLog.log("error noise reduction: " + ex.ToString(), enLogType.ERROR);
        onExitNoiseReduction(null, null);
      }
    }


    private void onExitNoiseReduction(object sender, EventArgs e)
    {
      _wf.Audio.readWav(_tmp2Wav, true);
      _modelState.State = enAppState.IDLE;
      if (File.Exists(_tmp1Wav))
        File.Delete(_tmp1Wav);
      if (File.Exists(_tmp2Wav))
        File.Delete(_tmp2Wav);
      RefreshZoomImg = true;
    }


    public void undoChanges()
    {
      _wf.Audio.undo();
    }

   

    public static string getDestPathForOriginal(string wavName, string wavSubDir)
    {
      string retVal;
      string srcPath = Path.GetDirectoryName(wavName);
      if (wavSubDir != "")
        retVal = srcPath.Replace(wavSubDir, AppParams.DIR_ORIG);
      else
        retVal = Path.Combine(srcPath, AppParams.DIR_ORIG);
      return retVal;
    }

    public static void saveWavBackup(string wavName, string wavSubDir)
    {
      string dstPath = getDestPathForOriginal(wavName, wavSubDir);
      if(!Directory.Exists(dstPath)) 
        Directory.CreateDirectory(dstPath);
      string dstFile = Path.Combine(dstPath, Path.GetFileName(wavName));
      if (!File.Exists(dstFile))
        File.Copy(wavName, dstFile);
    }


    public static void saveWavBackup(string wavName)
    {
      string dstPath = "";
      string srcPath = Path.GetDirectoryName(wavName);
      if (srcPath.IndexOf(AppParams.DIR_WAVS) >= 0)
        dstPath = srcPath.Replace(AppParams.DIR_WAVS, AppParams.DIR_ORIG);
      else
        dstPath = Path.Combine(srcPath, AppParams.DIR_ORIG);
      if (!Directory.Exists(dstPath))
        Directory.CreateDirectory(dstPath);
      string dstFile = Path.Combine(dstPath, Path.GetFileName(wavName));
      if (!File.Exists(dstFile))
        File.Copy(wavName, dstFile);
    }
  }
}