/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2024-09-01                                       
 *   Copyright (C) 2025: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using libParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

namespace BatInspector
{

  public class ReportItemBirdNet
  {
    string _remarks;
    bool _changed = false;
    public int Row { get; set; }
    public string FileName { get; set; }
    public string CallNr { get; set; }
    public string StartTime { get; set; }
    public string Duration { get; set; }
    public string SpeciesAuto { get; set; }
    public string SpeciesMan { get; set; }
    public string SpeciesLatin { get; set; }
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

    public ReportItemBirdNet(AnalysisFile file, AnalysisCall call)
    {
      FileName = file.Name;
      int callNr = call.getInt(Cols.NR);
      CallNr = callNr.ToString();
      if (callNr < 2)
        Remarks = file.getString(Cols.REMARKS);
      _changed = false;
      Row = call.ReportRow;
      Duration = call.getDouble(Cols.DURATION).ToString("0.#", CultureInfo.InvariantCulture);
      //Snr = call.getDouble(Cols.SNR).ToString("0.#", CultureInfo.InvariantCulture);
      StartTime = call.getString(Cols.START_TIME);
      SpeciesAuto = call.getString(Cols.SPECIES);
      SpeciesLatin = call.getString(Cols.SPEC_LATIN);
      Probability = call.getDouble(Cols.PROBABILITY).ToString("0.###", CultureInfo.InvariantCulture);
      Latitude = call.getDouble(Cols.LAT).ToString("0.#######", CultureInfo.InvariantCulture);
      Longitude = call.getDouble(Cols.LON).ToString("0.#######", CultureInfo.InvariantCulture);
      Temperature = call.getDouble(Cols.TEMPERATURE).ToString("0.#", CultureInfo.InvariantCulture);
      Humidity = call.getDouble(Cols.HUMIDITY).ToString("0.#", CultureInfo.InvariantCulture);
      //  Snr = call.getDouble(Cols.SNR).ToString();
      SpeciesMan = call.getString(Cols.SPECIES_MAN);
    }
  }

  public class ReportItemBattyBirdNet
  {
    string _remarks;
    bool _changed = false;
    public int Row { get; set; }
    public string FileName { get; set; }
    public string CallNr { get; set; }
    public string StartTime { get; set; }
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

    public ReportItemBattyBirdNet(AnalysisFile file, AnalysisCall call)
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
    }
  }


  public class ReportItemBd2
  {
    string _remarks;
    bool _changed = false;

    public ReportItemBd2(AnalysisFile file, AnalysisCall call)
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
  }


  public class PrjView
  {
    enModel _modelType = enModel.BAT_DETECT2;
    List<ReportItemBd2> _reportBd2 = null;
    List<ReportItemBirdNet> _reportBirdNet = null;
    List<ReportItemBattyBirdNet> _reportBattyBirdNet = null;
    Thread _pngThread = null;

    public PrjView()
    {
    }

    public void initReport()
    {
      if (Prj != null)
        _modelType = Prj.SelectedModelParams.Type;

      switch (_modelType)
      {
        case enModel.BAT_DETECT2:
          _reportBd2 = new List<ReportItemBd2>();
          break;
        case enModel.BATTY_BIRD_NET:
          _reportBattyBirdNet = new List<ReportItemBattyBirdNet>();
          break;
        case enModel.BIRDNET:
          _reportBirdNet = new List<ReportItemBirdNet>();
          break;
      }
    }

    List<string> _showWavFiles = new List<string>();
    Pool<Sonogram> _sonograms = new Pool<Sonogram>(AppParams.CNT_WAV_CONTROLS + 2);
    bool _stopCreatingPngs = false;

    public List<string> VisibleFiles { get { return _showWavFiles; } }

    public Project Prj { get; set; }

    public Query Query { get; set; }
    public int StartIdx { get; set; } = 0;


    public IEnumerable getListSource()
    {
      IEnumerable retVal = null;
      switch (_modelType)
      {
        case enModel.BAT_DETECT2:
        case enModel.rnn6aModel:
        case enModel.resNet34Model:
          retVal = _reportBd2 as IEnumerable;
          break;
        case enModel.BATTY_BIRD_NET:
          retVal = _reportBattyBirdNet as IEnumerable;
          break;

        case enModel.BIRDNET:
          retVal = _reportBirdNet as IEnumerable;
          break;
      }
      return retVal;
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
        switch (_modelType)
        {
          case enModel.BAT_DETECT2:
          case enModel.rnn6aModel:
          case enModel.resNet34Model:
            _reportBd2.Clear();
            break;
          
            case enModel.BATTY_BIRD_NET:
          _reportBattyBirdNet.Clear();
            break;

          case enModel.BIRDNET:
          _reportBirdNet.Clear();
            break;
        }

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
                switch (_modelType)
                {
                  case enModel.BAT_DETECT2:
                  case enModel.resNet34Model:
                  case enModel.rnn6aModel:
                    ReportItemBd2 it = new ReportItemBd2(f, f.Calls[c]);
                    _reportBd2.Add(it);
                    break;
                  case enModel.BATTY_BIRD_NET:
                    ReportItemBattyBirdNet itbb = new ReportItemBattyBirdNet(f, f.Calls[c]);
                    _reportBattyBirdNet.Add(itbb);
                    break;

                  case enModel.BIRDNET:
                    ReportItemBirdNet itb = new ReportItemBirdNet(f, f.Calls[c]);
                    _reportBirdNet.Add(itb);
                    break;
                }
              }
            }
          }
        }
      }
      return true;
    }

    public void addReportItem(AnalysisFile file, AnalysisCall call, enModel modelType)
    {
      switch (_modelType)
      {
        case enModel.BAT_DETECT2:
        case enModel.resNet34Model:
        case enModel.rnn6aModel:
          if (_reportBd2 != null)
          {
            ReportItemBd2 item = new ReportItemBd2(file, call);
            _reportBd2.Add(item);
          }
          break;
        case enModel.BATTY_BIRD_NET:
          if (_reportBattyBirdNet != null)
          {
            ReportItemBattyBirdNet item = new ReportItemBattyBirdNet(file, call);
            _reportBattyBirdNet.Add(item);
          }
          break;

        case enModel.BIRDNET:
          if (_reportBirdNet != null)
          {
            ReportItemBirdNet item = new ReportItemBirdNet(file, call);
            _reportBirdNet.Add(item);
          }
          break;
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
        tmp.release();
      }
    }

    public void startCreatingPngFiles()
    {
      _pngThread = new Thread(createPngImages);
      DebugLog.log("generation of missing pngs started", enLogType.INFO);
      _pngThread.Start();
    }

    public void stopCreatingPngFiles()
    {
      if ((_pngThread != null) && (_pngThread.IsAlive))
      {
        _stopCreatingPngs = true;
        DebugLog.log("abort background generation of PNGs", enLogType.INFO);
        _pngThread.Join(300);
      }
    }

    private void createPngImages()
    {
      App.Model.View.createPngFiles(App.Model.ColorTable);
      DebugLog.log("generation of PNGs finished", enLogType.INFO);
    }

    private void createPngFiles(ColorTable colorTable)
    {
      if ((Prj != null))
      {
        foreach (PrjRecord rec in Prj.Records)
        {
          if(_stopCreatingPngs)
          {
            _stopCreatingPngs = false;
            break;
          }
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


 /*   public void updateReport(Csv csv)
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
    } */

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
    dlgRelease _dlgRelease;

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
        using (MemoryStream memory = new MemoryStream())
        {
          memory.Position = 0;
          memory.SetLength(0);
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



    public void createFtImageFromWavFile(string wavName, int fftWidth, ColorTable colorTable)
    {
/*
      string pngName = wavName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
      try
      {
        if(!File.Exists(pngName) && File.Exists(wavName))
          BioAcoustics.createPngFromWav(wavName, (int)AppParams.Inst.WaterfallWidth, 512, AppParams.Inst.GradientRange);
        _bmp = new Bitmap(pngName);
        if (_bmp != null)
          _bImg = Convert(_bmp);
      }
      catch (Exception ex)
      {
        DebugLog.log($"error creating png form {wavName} : {ex.ToString()}", enLogType.ERROR);
      } */
    
      string pngName = "";
      try
      {
        Bitmap bmp = null;
          pngName = wavName.ToLower().Replace(AppParams.EXT_WAV, AppParams.EXT_IMG);
          if (File.Exists(pngName))
            bmp = new Bitmap(pngName);
          else
          {
            Waterfall wf = new Waterfall(wavName, colorTable, fftWidth);
            if (wf.Ok)
            {
              wf.generateFtDiagram(0, (double)wf.Audio.Samples.Length / wf.SamplingRate, AppParams.Inst.WaterfallWidth);
              bmp = wf.generateFtPicture(0, wf.Duration, 0, wf.SamplingRate / 2000);
              bmp.Save(pngName);
            }
            else
              DebugLog.log("File '" + wavName + "'does not exist, removed from project and report", enLogType.WARNING);
          }
          if (bmp != null)
            _bImg = Convert(bmp);
      }
      catch (Exception ex)
      {
        DebugLog.log("error creating png " + pngName + ": " + ex.ToString(), enLogType.ERROR);
      }
    }


    public void createZoomViewFt(double tStart, double tEnd, double fMin, double fMax)
    {
      using (Bitmap bmp = App.Model.ZoomView.Waterfall.generateFtPicture(tStart, tEnd, fMin, fMax))
      {
        if (bmp != null)
          _bImg = Convert(bmp);
        else
        {
          _bImg = null;
          DebugLog.log("error creating zoom view", enLogType.ERROR);
        }
      }
    }

    public void createZoomViewXt(double aMin, double aMax, double tStart, double tEnd)
    {
      using (Bitmap bmp = App.Model.ZoomView.Waterfall.generateXtPicture(aMin, aMax, tStart, tEnd))
      {
        if (bmp != null)
          _bImg = Convert(bmp);
        else
        {
          _bImg = null;
          DebugLog.log("error creating zoom view", enLogType.ERROR);
        }
      }
    }
  }
}
