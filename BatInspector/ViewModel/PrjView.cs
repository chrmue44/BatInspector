﻿using BatInspector.Controls;
using BatInspector.Forms;
using BatInspector.Properties;
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BatInspector
{


  public class ReportItemBd2
  {
    string _remarks;
    bool _changed = false;

    public ReportItemBd2(AnalysisFile file, AnalysisCall call, enModel modelType)
    {

      FileName = file.Name;
      int callNr = call.getInt(Cols.NR);
      CallNr = callNr.ToString();
      if (callNr < 2)
        Remarks = file.getString(Cols.REMARKS);
      _changed = false;
      Row = call.ReportRow;
      Duration = call.getDouble(Cols.DURATION).ToString("0.#", CultureInfo.InvariantCulture);
      Snr = call.getDouble(Cols.SNR).ToString("0.#", CultureInfo.InvariantCulture);
      StartTime = call.getString(Cols.START_TIME);
      SpeciesAuto = call.getString(Cols.SPECIES);
      Probability = call.getDouble(Cols.PROBABILITY).ToString("0.###", CultureInfo.InvariantCulture);
      Latitude = call.getDouble(Cols.LAT).ToString("0.#######", CultureInfo.InvariantCulture);
      Longitude = call.getDouble(Cols.LON).ToString("0.#######", CultureInfo.InvariantCulture);
      Temperature = call.getDouble(Cols.TEMPERATURE).ToString("0.#", CultureInfo.InvariantCulture);
      Humidity = call.getDouble(Cols.HUMIDITY).ToString("0.#", CultureInfo.InvariantCulture);
      //  Snr = call.getDouble(Cols.SNR).ToString();
      SpeciesMan = call.getString(Cols.SPECIES_MAN);
      if (modelType != enModel.BATTY_BIRD_NET)
      {
        FreqMin = (call.getDouble(Cols.F_MIN) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
        FreqMax = (call.getDouble(Cols.F_MAX) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
        FreqMaxAmp = (call.getDouble(Cols.F_MAX_AMP) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
      }
    }

    public int Row { get; set; }
    public string FileName { get; set; }
    public string CallNr { get; set; }
    public string StartTime { get; set; }
    public string FreqMin { get; set; }
    public string FreqMax { get; set; }
    public string FreqMaxAmp { get; set; }
    public string Duration { get; set; }
    public string Snr { get; set; }
    public string SpeciesAuto { get; set; }
    public string SpeciesMan { get; set; }
    public string Probability { get; set; }

    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string Temperature { get; set; }
    public string Humidity { get; set; }
    public string Remarks
    {
      get { return _remarks; }
      set
      {
        _remarks = value;
        _changed = true;
      }
    }

    public void resetChanged()
    {
      _changed = false;
    }

    public static ReportItemBd2 find(string filename, List<ReportItemBd2> list)
    {
      foreach (ReportItemBd2 r in list)
      {
        if (r.FileName == filename)
          return r;
      }
      return null;
    }
  }


  public class PrjView
  {
    List<ReportItemBd2> _report = new List<ReportItemBd2>();
    List<string> _showWavFiles = new List<string>();
    Pool<Sonogram> _sonograms = new Pool<Sonogram>(AppParams.CNT_WAV_CONTROLS + 2);

    public List<ReportItemBd2> ListView { get { return _report; } }
    public List<string> VisibleFiles { get { return _showWavFiles; } }

    public Project Prj { get; set; }

    public Query Query { get; set; }
    public int StartIdx { get; set; } = 0;
    public PrjView()
    {
    }


    public Sonogram createSonogram(string id)
    {
      return _sonograms.get(id);
    }

    public void buildListOfVisibles(bool selectedOnly)
    {
      _showWavFiles.Clear();
      PrjRecord[] list = getRecords();
      foreach (PrjRecord rec in list)
      {
        if (!selectedOnly || (selectedOnly && rec.Selected))
          _showWavFiles.Add(rec.File);
      }
    }

    public void addFile(AnalysisFile file, enModel modelType)
    {
      foreach (AnalysisCall c in file.Calls)
      {
        addReportItem(file, c, modelType);
      }
    }

    public bool populateList(Filter filter, FilterItem filterItem)
    {
      PrjRecord[] recList = getRecords();
      Analysis analysis = getAnalysis();
      if (recList != null)
      {
        _report.Clear();

        foreach (PrjRecord rec in recList)
        {
          string wavFile = rec.File;
          AnalysisFile f = analysis.find(wavFile);
          if (f != null)
          {
            for (int c = 0; c < f.Calls.Count; c++)
            {
              bool res = true;
              if (filter != null)
                res = filter.apply(filterItem, f.Calls[c]);
              if (res)
              {
                ReportItemBd2 it = new ReportItemBd2(f, f.Calls[c], analysis.ModelType);
                _report.Add(it);
              }
            }
          }
        }
      }
      return true;
    }

    public void addReportItem(AnalysisFile file, AnalysisCall call, enModel modelType)
    {
      if (_report != null)
      {
        ReportItemBd2 item = new ReportItemBd2(file, call, modelType);
        _report.Add(item);
      }
    }





    public void createPngFile(PrjRecord rec, bool fromQuery, ColorTable colorTable)
    {
      string wavName = fromQuery ? Path.Combine(Query.DestDir, rec.File) : Path.Combine(Prj.PrjDir, Prj.WavSubDir, rec.File);
      Sonogram tmp = _sonograms.get(rec.Name);
      if (tmp != null)
      {
        tmp.createFtImageFromWavFile(wavName, AppParams.FFT_WIDTH, colorTable);
        if ((tmp.Image == null) && (Prj != null))
          DebugLog.log($"unable to create PNG filefor : {wavName}", enLogType.ERROR);
      }
      tmp.release();
    }


    public void createPngFiles(ColorTable colorTable)
    {
      if ((Prj != null))
      {
        foreach (PrjRecord rec in Prj.Records)
        {
          string pngName = Path.Combine(Prj.PrjDir, Prj.WavSubDir, Path.GetFileNameWithoutExtension(rec.File) + AppParams.EXT_IMG);
          if (!File.Exists(pngName))
          {
            lock (rec)
            {
              createPngFile(rec, false, colorTable);
            }
          }
        }
      }
    }


    public void updateReport(Csv csv)
    {
      foreach (ReportItemBd2 item in _report)
      {
        int row = item.Row;
        item.Longitude = csv.getCell(row, Cols.LON);
        item.Latitude = csv.getCell(row, Cols.LAT);
        item.FreqMax = csv.getCell(row, Cols.F_MAX);
        item.FreqMin = csv.getCell(row, Cols.F_MIN);
        item.Snr = csv.getCell(row, Cols.SNR);
        item.Duration = csv.getCell(row, Cols.DURATION);
        item.CallNr = csv.getCell(row, Cols.NR);
        item.FileName = csv.getCell(row, Cols.NAME);
        item.FreqMaxAmp = csv.getCell(row, Cols.F_MAX_AMP);
        item.Probability = csv.getCell(row, Cols.PROBABILITY);
        item.Remarks = csv.getCell(row, Cols.REMARKS);
        item.StartTime = csv.getCell(row, Cols.START_TIME);
        item.SpeciesAuto = csv.getCell(row, Cols.SPECIES);
        item.SpeciesMan = csv.getCell(row, Cols.SPECIES_MAN);
      }
    }

    private PrjRecord[] getRecords()
    {
      PrjRecord[] retVal = new PrjRecord[0];
      if ((Prj != null) && (Prj.Ok))
      {
        retVal = Prj.Records;
      }
      else if (Query != null)
      {
        retVal = Query.Records;
      }
      return retVal;
    }

    private Analysis getAnalysis()
    {
      Analysis retVal = null;
      if ((Prj != null) && (Prj.Ok))
      {
        retVal = Prj.Analysis;
      }
      else if (Query != null)
      {
        retVal = Query.Analysis;
      }
      return retVal;
    }
  }

  public class Sonogram : IPool
  {
    BitmapImage _bImg = null;
    Bitmap _bmp = null;
    dlgRelease _dlgRelease;
    MemoryStream _memory = new MemoryStream();

    public BitmapImage Image { get { return _bImg; } }

    public void setCallBack(dlgRelease dlg)
    {
       _dlgRelease = dlg;
    }

    public void release()
    {
      if (_dlgRelease!= null)
        _dlgRelease(this);
    }

    /*
    BitmapImage Convert(Bitmap bitmap)
    {
      var bitmapImage = new BitmapImage();
      try
      {
        using (MemoryStream memory = new MemoryStream())
        {
//          memory.Position = 0;
          bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
          bitmapImage.BeginInit();
          bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
          bitmapImage.StreamSource = memory;
  //        bitmapImage.CacheOption = BitmapCacheOption.None;
          bitmapImage.EndInit();
          bitmapImage.Freeze();
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("error creating PNG: " + ex.ToString(), enLogType.ERROR);
      }
      return bitmapImage;
    }
    */


    BitmapImage Convert(Bitmap bitmap)
    {
      var bitmapImage = new BitmapImage();
      try
      {
        lock (_memory)
        {
          _memory.Position = 0;
          _memory.SetLength(0);
          bitmap.Save(_memory, System.Drawing.Imaging.ImageFormat.Png);
          bitmapImage.BeginInit();
          bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
          bitmapImage.StreamSource = _memory;
          //        bitmapImage.CacheOption = BitmapCacheOption.None;
          bitmapImage.EndInit();
          bitmapImage.Freeze();
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("error creating PNG: " + ex.ToString(), enLogType.ERROR);
      }
      return bitmapImage;
    }


    public void createFtImageFromWavFile(string wavName, int fftWidth, ColorTable colorTable)
    {
      string pngName = "";
      try
      {
        pngName = wavName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
        if (File.Exists(pngName))
          _bmp = new Bitmap(pngName);
        else
        {
          Waterfall wf = new Waterfall(wavName, colorTable, fftWidth);
          if (wf.Ok)
          {
            wf.generateFtDiagram(0, (double)wf.Audio.Samples.Length / wf.SamplingRate, AppParams.Inst.WaterfallWidth);
            _bmp = wf.generateFtPicture(0, wf.Duration, 0, wf.SamplingRate / 2000);
            _bmp.Save(pngName);
          }
          else
            DebugLog.log("File '" + wavName + "'does not exist, removed from project and report", enLogType.WARNING);
        }
        if (_bmp != null)
          _bImg = Convert(_bmp);
      }
      catch (Exception ex)
      {
        DebugLog.log("error creating png " + pngName + ": " + ex.ToString(), enLogType.ERROR);
      }
    }


    public void createZoomViewFt(double tStart, double tEnd, double fMin, double fMax)
    {
      _bmp = App.Model.ZoomView.Waterfall.generateFtPicture(tStart, tEnd, fMin, fMax);
      if (_bmp != null)
        _bImg = Convert(_bmp);
      else
      {
        _bImg = null;
        DebugLog.log("error creating zoom view", enLogType.ERROR);
      }
    }

    public void createZoomViewXt(double aMin, double aMax, double tStart, double tEnd)
    {
      _bmp = App.Model.ZoomView.Waterfall.generateXtPicture(aMin, aMax, tStart, tEnd);
      if (_bmp != null)
        _bImg = Convert(_bmp);
      else
      {
        _bImg = null;
        DebugLog.log("error creating zoom view", enLogType.ERROR);
      }
    }
  }
}
