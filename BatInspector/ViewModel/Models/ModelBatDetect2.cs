/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-04-04                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/


using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;


namespace BatInspector
{
  public enum enRepMode
  {
    REPLACE,
    APPEND
  }

  public class ModelBatDetect2 : BaseModel
  {
    const string MODEL_NAME = "BatDetect2";
    const string PAR_DETECTION_THRESHOLD = "Detection Threshold";
    public const string BD2_DEFAULT_MODEL = "Net2DFast_UK_same.pth.tar";
    Project _prj;
    string _listOfFiles;
    
    public ModelBatDetect2(int index) : 
      base(index, enModel.BAT_DETECT2, MODEL_NAME)
    {
    }


    public override int classify(Project prj, bool removeEmptyFiles, bool cli = false)
    {
      _isBusy = true;
      _prj = prj;
      _cli = cli;
      int retVal = 0;
      try
      {
        ModelParams pars = prj.AvailableModelParams[prj.SelectedModelIndex];
        string detTrsh = pars.getPar(PAR_DETECTION_THRESHOLD);
        string wavDir = Path.Combine(prj.PrjDir ,prj.WavSubDir);
        string annDir = prj.getAnnotationDir();
        string modPath = Path.IsPathRooted(AppParams.Inst.ModelRootPath) ?
                         AppParams.Inst.ModelRootPath  :
                         Path.Combine(AppParams.AppDataPath, AppParams.Inst.ModelRootPath);
        string wrkDir = Path.Combine(modPath, prj.AvailableModelParams[this.Index].SubDir);
        string args = $"\"{wrkDir}\" \"{wavDir}\" \"{annDir}\" {detTrsh} {pars.DataSet}";
        string cmd = Path.Combine(wrkDir, prj.AvailableModelParams[this.Index].Script);
        _listOfFiles = "";
        retVal = _proc.launchCommandLineApp(cmd, outputDataHandler, wrkDir, true, args);
        if (retVal == 0)
        {
          bool ok = createReportFromAnnotations(0.5, App.Model.SpeciesInfos, wavDir, annDir, prj.getReportName(this.Index), enRepMode.REPLACE,prj.MetaData);
          if (ok)
          {
    //        cleanup(prj);
            prj.Analysis.read(prj.getReportName(this.Index), App.Model.DefaultModelParams, prj.MetaData);
            if(removeEmptyFiles)
              prj.removeFilesNotInReport();
          }
          else
            retVal = 2;
        }
        else
        {
          string logName = wrkDir + "/files.txt";
          File.WriteAllText(logName, _listOfFiles);
          MessageBox.Show(BatInspector.Properties.MyResources.MsgErrorBd2, BatInspector.Properties.MyResources.Error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
          DebugLog.log(_proc.ErrData, enLogType.ERROR);
        }
      }
      catch 
      {
        retVal = 1;
      }
      finally
      {
        _isBusy = false;
      }

      return retVal;
    }

    public override void train()
    {
      throw new NotImplementedException();
    }

    public override int createReport(Project prj)
    {
      int retVal = 0;
      string wavDir = Path.Combine(prj.PrjDir, prj.WavSubDir);
      string annDir = Path.Combine(prj.PrjDir, prj.SelectedModelParams.SubDir, AppParams.ANNOTATION_SUBDIR);
      bool ok = createReportFromAnnotations(0.5, App.Model.SpeciesInfos, wavDir, annDir, prj.getReportName(this.Index), enRepMode.REPLACE, prj.MetaData);
      if (ok)
      {
        //        cleanup(prj.PrjDir);
        prj.Analysis.read(prj.getReportName(this.Index), App.Model.DefaultModelParams, prj.MetaData);
        prj.removeFilesNotInReport();
      }
      else
        retVal = 2;
      return retVal;
    }

    public bool createReportFromAnnotations(double minProb, List<SpeciesInfos> speciesInfos, string wavDir, string annDir, string reportName, enRepMode mode, enMetaData metaData)
    {
      bool retVal = true;
      try
      {
        Csv report;
        if (mode == enRepMode.REPLACE)
        {
          if (File.Exists(reportName))
            File.Delete(reportName);
          report = createReport(Cols.SPECIES);
        }
        else
        {
          report = new Csv();
          report.read(reportName);
        }
        if (!Directory.Exists(annDir))
        {
          DebugLog.log("Not a single bat call found!", enLogType.WARNING);
          retVal = false;
        }
        else
        {
          string[] files = Directory.GetFiles(annDir, "*.wav.csv", SearchOption.AllDirectories);
          if (files.Length == 0)
          {
            DebugLog.log("Not a single bat call found!", enLogType.WARNING);
            retVal = false;
          }
          else
          {
            DebugLog.log("start creating report", enLogType.INFO);
            int cnt = 0;
            foreach (string file in files)
            {
              // read infoFile
              string wavName = /*wavDir + "/" + */ Path.GetFileName(file).ToLower().Replace(".csv", "");
              string sampleRate = "?";
              string fileLen = "?";
              string recTime = "?";
              double lat = 0.0;
              double lon = 0.0;
              double temperature = -20;
              double humidity = -1;
              BatRecord info = PrjMetaData.retrieveMetaData(wavDir, wavName, metaData);
              WavFile wav = new WavFile();
              string fullName = Path.Combine(wavDir, wavName);
              int res = wav.readFile(fullName);
              if (info != null)
              {
                sampleRate = info.Samplerate.Replace(" Hz", ""); ;
                fileLen = info.Duration.Replace(" Sec", "");
                recTime = info.DateTime;
                PrjMetaData.parsePosition(info, out lat, out lon);
                double.TryParse(info.Temparature, out temperature);
                double.TryParse(info.Humidity, out humidity);
              }

              // read annontation for one wav file
              Csv csvAnn = new Csv();
              csvAnn.read(file, ",", true);
              string fileFeat = file.ToLower().Replace("wav.csv", "wav_spec_features.csv");
              Csv csvFeat = null;
              if (File.Exists(fileFeat))
              {
                csvFeat = new Csv();
                csvFeat.read(fileFeat, ",", true);
              }
              int rowcnt = csvAnn.RowCnt;
              for (int row = 2; row <= rowcnt; row++)
              {
                report.addRow();
                int repRow = report.RowCnt;
                report.setCell(repRow, Cols.SAMPLERATE, sampleRate);
                report.setCell(repRow, Cols.FILE_LEN, fileLen);
                report.setCell(repRow, Cols.REC_TIME, recTime);
                report.setCell(repRow, Cols.NAME, wavName);
                report.setCell(repRow, Cols.LAT, lat);
                report.setCell(repRow, Cols.LON, lon);
                report.setCell(repRow, Cols.TEMPERATURE, temperature);
                report.setCell(repRow, Cols.HUMIDITY, humidity);
                int id = csvAnn.getCellAsInt(row, "id");
                report.setCell(repRow, Cols.NR, id + 1);
                double startTime = csvAnn.getCellAsDouble(row, "start_time");
                report.setCell(repRow, Cols.START_TIME, startTime, 3);
                double fMin = csvAnn.getCellAsDouble(row, "low_freq");
                report.setCell(repRow, Cols.F_MIN, fMin, 1);
                double fMax = csvAnn.getCellAsDouble(row, "high_freq");
                report.setCell(repRow, Cols.F_MAX, fMax, 1);
                double duration = -1;
                double callInterval = -0.001;
                double bandwidth;
                if (csvFeat != null)
                {
                  double fMaxAmp = csvFeat.getCellAsDouble(row, "max_power_bb");
                  report.setCell(repRow, Cols.F_MAX_AMP, fMaxAmp);
                  duration = csvFeat.getCellAsDouble(row, "duration");
                  callInterval = csvFeat.getCellAsDouble(row, "call_interval");
                  if (callInterval < 0)
                    callInterval = -0.001;
                  bandwidth = csvFeat.getCellAsDouble(row, "bandwidth");
                  fMin = csvFeat.getCellAsDouble(row, "low_freq_bb");
                  report.setCell(repRow, Cols.F_MIN, fMin, 1);
                  fMax = csvFeat.getCellAsDouble(row, "high_freq_bb");
                  report.setCell(repRow, Cols.F_MAX, fMax, 1);
                }
                else
                {
                  double endTime = csvAnn.getCellAsDouble(row, "end_time");
                  duration = (endTime - startTime);
                  bandwidth = fMax - fMin;
                }
                report.setCell(repRow, Cols.BANDWIDTH, bandwidth);
                report.setCell(repRow, Cols.CALL_INTERVALL, callInterval * 1000, 1);
                report.setCell(repRow, Cols.DURATION, duration * 1000, 1);
                double snr = res == 0 ? wav.calcSnr(startTime, startTime + duration) : -1;
                report.setCell(repRow, Cols.SNR, snr);
                string latin = translate(csvAnn.getCell(row, "class"));

                double prob = csvAnn.getCellAsDouble(row, "class_prob");
                report.setCell(repRow, Cols.PROBABILITY, prob);
                string abbr = "";
                if (prob < minProb)
                  abbr = "??PRO[";
                SpeciesInfos specInfo = SpeciesInfos.findLatin(latin, speciesInfos);
                if ((info != null) && (specInfo != null))
                  abbr += specInfo.Abbreviation;
                else
                  abbr += latin;
                if (prob < minProb)
                  abbr += "]";
                report.setCell(repRow, Cols.SPECIES, abbr);
                report.setCell(repRow, Cols.SPECIES_MAN, "todo");
                report.setCell(repRow, Cols.REMARKS, "");

                cnt++;
                if((cnt % 20) == 0)
                  DebugLog.log($"processed {cnt} files", enLogType.INFO);

              }
            }
          }
          string dir = Path.GetDirectoryName(reportName);
          if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
          report.saveAs(reportName);
        }
      }
      catch (Exception e) 
      {
        retVal = false;
        DebugLog.log("error creating report from model predicitons, " + e.ToString(), enLogType.ERROR);
      }

      return retVal;
    }


    /// <summary>
    /// add a sql row to BatDetect2 report
    /// </summary>
    /// <param name="csv"></param>
    /// <param name="sqlRow"></param>
    public static void addReportRow(Csv csv, sqlRow sqlRow)
    {
      csv.addRow();
      addFieldToRow(csv, sqlRow, Cols.SAMPLERATE, DBBAT.SAMPLE_RATE);
      addFieldToRow(csv, sqlRow, Cols.FILE_LEN, DBBAT.FILE_LENGTH);
      addFieldToRow(csv, sqlRow, Cols.REC_TIME, DBBAT.RECORDING_TIME);
      addFieldToRow(csv, sqlRow, Cols.NAME, DBBAT.WAV_FILE_NAME);
      addFieldToRow(csv, sqlRow, Cols.LAT, DBBAT.LAT);
      addFieldToRow(csv, sqlRow, Cols.LON, DBBAT.LON);
      addFieldToRow(csv, sqlRow, Cols.TEMPERATURE, DBBAT.TEMP);
      addFieldToRow(csv, sqlRow, Cols.HUMIDITY, DBBAT.HUMI);
      addFieldToRow(csv, sqlRow, Cols.NR, DBBAT.CALLNR);
      addFieldToRow(csv, sqlRow, Cols.START_TIME, DBBAT.START_TIME);
      addFieldToRow(csv, sqlRow, Cols.F_MIN, DBBAT.FMIN);
      addFieldToRow(csv, sqlRow, Cols.F_MAX, DBBAT.FMAX);
      addFieldToRow(csv, sqlRow, Cols.F_MAX_AMP, DBBAT.FMAXAMP);      
      addFieldToRow(csv, sqlRow, Cols.BANDWIDTH, DBBAT.BWIDTH);
      addFieldToRow(csv, sqlRow, Cols.CALL_INTERVALL, DBBAT.CALL_DST);
      addFieldToRow(csv, sqlRow, Cols.DURATION, DBBAT.CALL_LEN);
      addFieldToRow(csv, sqlRow, Cols.SNR, DBBAT.SNR);
      addFieldToRow(csv, sqlRow, Cols.PROBABILITY, DBBAT.PROB);
      addFieldToRow(csv, sqlRow, Cols.SPECIES, DBBAT.SPEC_AUTO);
      addFieldToRow(csv, sqlRow, Cols.SPECIES_MAN, DBBAT.SPEC_MAN);
      addFieldToRow(csv, sqlRow, Cols.REMARKS, DBBAT.REM);
    }


    /// <summary>
    /// translate species name to official form
    /// </summary>
    /// <param name="spec"></param>
    /// <returns></returns>
    private string translate(string spec)
    {
      if (spec == "Barbastellus barbastellus")
        return "Barbastella barbastellus";
      else if (spec == "Myotis natteri")
        return "Myotis nattereri";
      else if (spec == "Eptesicus nilssoni")
        return "Eptesicus nilssonii";
      else
        return spec;
    }

    public static Csv createReport(string colSpecies)
    {
      Csv csv = new Csv();
      csv.clear();
      csv.addRow();
      string[] header =
      {
        Cols.NAME,
        Cols.REC_TIME,
        Cols.LAT,
        Cols.LON,
        Cols.TEMPERATURE,
        Cols.HUMIDITY,
        Cols.NR,
        colSpecies,
        Cols.SPECIES_MAN,
        Cols.SAMPLERATE,
        Cols.FILE_LEN,
        Cols.F_MAX_AMP,
        Cols.F_MIN,
        Cols.F_MAX,
        Cols.DURATION,
        Cols.CALL_INTERVALL,
        Cols.START_TIME,
        Cols.BANDWIDTH,
        Cols.PROBABILITY,
        Cols.SNR,
        Cols.REMARKS
      };

      csv.initColNames(header, true);
      return csv;
    }

    private void outputDataHandler(object sender, DataReceivedEventArgs ev)
    {
      if (ev.Data?.ToLower().IndexOf(AppParams.EXT_WAV) > 0)
      {
        int pos = ev.Data.IndexOf(' ');
        if (pos > 0)
        {
          string intStr = ev.Data.Substring(0, pos);
          if (int.TryParse(intStr, out int val) && (_prj != null) && _prj.Ok)
          {
            val++;
            _listOfFiles += ev.Data + "\n";
            DebugLog.log(ev.Data, enLogType.INFO);
            string msg = BatInspector.Properties.MyResources.ModelBatDetect2msgProcessing + val.ToString() + "/" + _prj.Records.Length.ToString();
            if (_cli)
              DebugLog.log(msg, enLogType.INFO);
            else
              App.Model.Status.Msg = msg;
          }
        }
      }
      else if (((ev.Data?.ToLower().IndexOf("error") > 0) || (ev.Data?.ToLower().IndexOf("Error") > 0)) && (ev.Data?.ToLower().IndexOf("error.") < 0))
        DebugLog.log(ev.Data, enLogType.ERROR);
    }


    void cleanup(Project prj)
    {
      string dir = prj.getAnnotationDir();
      if (Directory.Exists(dir))
        Directory.Delete(dir, true);
    }
  }
}
