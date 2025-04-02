using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BatInspector
{
  public class ModelBattyB : BaseModel
  {
    const string PAR_SENSITIVITY = "Sensitivity";
    const string PAR_MIN_PROB = "minimal Probability";
    Project _prj;
    int _counter = 0;

    public ModelBattyB(int index) : base(index, enModel.BATTY_BIRD_NET, "BattyBirdNET")
    {
    }

    public override int classify(Project prj, bool removeEmptyFiles, bool cli = false)
    {
      _isBusy = true;
      _prj = prj;
      _cli = cli;
      _counter = 0;
      int retVal = 0;
      try
      {
        ModelParams pars = prj.AvailableModelParams[prj.SelectedModelIndex];
        string wavDir = Path.Combine(prj.PrjDir, prj.WavSubDir);
        string sens = pars.getPar(PAR_SENSITIVITY);
        string minProb = pars.getPar(PAR_MIN_PROB);
        string annDir = prj.getAnnotationDir();
        if (!Directory.Exists(annDir))
          Directory.CreateDirectory(annDir);
        string modPath = Path.IsPathRooted(AppParams.Inst.ModelRootPath) ?
                         AppParams.Inst.ModelRootPath :
                         Path.Combine(AppParams.AppDataPath, AppParams.Inst.ModelRootPath);
        string wrkDir = Path.Combine(modPath, prj.AvailableModelParams[this.Index].SubDir);
        string args = $"\"{wrkDir}\" \"{wavDir}\" \"{annDir}\" {sens} {minProb} {pars.DataSet}";
        string cmd = Path.Combine(wrkDir, prj.AvailableModelParams[this.Index].Script);
        retVal = _proc.launchCommandLineApp(cmd, outputDataHandler, wrkDir, true, args);
        if (retVal == 0)
        {
          bool ok = createReportFromAnnotations(0.5, App.Model.SpeciesInfos, wavDir, annDir, prj.getReportName(this.Index), enRepMode.REPLACE);
          if (ok)
          {
            //        cleanup(prj);
            prj.Analysis.read(prj.getReportName(this.Index), App.Model.DefaultModelParams);
            if (removeEmptyFiles)
              prj.removeFilesNotInReport();
          }
          else
            retVal = 2;
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


    public override int createReport(Project prj)
    {
      int retVal = 0;
      string wavDir = Path.Combine(prj.PrjDir, prj.WavSubDir);
      string annDir = prj.getAnnotationDir();
      bool ok = createReportFromAnnotations(0.5, App.Model.SpeciesInfos, wavDir, annDir, prj.getReportName(this.Index), enRepMode.REPLACE);
      if (ok)
      {
        //        cleanup(prj.PrjDir);
        prj.Analysis.read(prj.getReportName(this.Index), App.Model.DefaultModelParams);
        prj.removeFilesNotInReport();
      }
      else
        retVal = 2;
      return retVal;
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
        Cols.SPECIES,
        Cols.SPECIES_MAN,
        Cols.SAMPLERATE,
        Cols.FILE_LEN,
        Cols.DURATION,
        Cols.START_TIME,
        Cols.PROBABILITY,
        Cols.REMARKS
      };

      csv.initColNames(header, true);
      return csv;
    }


    public bool createReportFromAnnotations(double minProb, List<SpeciesInfos> speciesInfos, string wavDir, string annDir, string reportName, enRepMode mode)
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
          string[] files = Directory.GetFiles(annDir, "*.bat.results.csv", SearchOption.AllDirectories);
          if (files.Length == 0)
          {
            DebugLog.log("Not a single bat call found!", enLogType.WARNING);
            retVal = false;
          }
          else
          {
            foreach (string file in files)
            {
              // read infoFile
              string wavName = Path.GetFileName(file).ToLower().Replace(".bat.results.csv", ".wav");
              string infoName = wavDir + "/" + Path.GetFileName(file).ToLower().Replace(".bat.results.csv", ".xml");
              string sampleRate = "?";
              string fileLen = "?";
              string recTime = "?";
              double lat = 0.0;
              double lon = 0.0;
              double temperature = -20;
              double humidity = -1;
              BatRecord info = ElekonInfoFile.read(infoName);
              if (info != null)
              {
                sampleRate = info.Samplerate.Replace(" Hz", ""); ;
                fileLen = info.Duration.Replace(" Sec", "");
                recTime = info.DateTime;
                ElekonInfoFile.parsePosition(info, out lat, out lon);
                double.TryParse(info.Temparature, out temperature);
                double.TryParse(info.Humidity, out humidity);
              }

              // read annontation for one wav file
              Csv csvAnn = new Csv();
              csvAnn.read(file, ",", true);
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
                int id = row - 1;
                report.setCell(repRow, Cols.NR, id);
                double startTime = csvAnn.getCellAsDouble(row, "Start (s)");
                double endTime = csvAnn.getCellAsDouble(row, "End (s)");
                report.setCell(repRow, Cols.START_TIME, startTime, 3);
                double duration = endTime - startTime;
                report.setCell(repRow, Cols.DURATION, duration * 1000, 1);
                string latin = csvAnn.getCell(row, "Scientific name");

                double prob = csvAnn.getCellAsDouble(row, "Confidence");
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


    private void outputDataHandler(object sender, DataReceivedEventArgs ev)
    {
      if (ev.Data?.ToLower().IndexOf(AppParams.EXT_WAV) > 0)
      {
        int pos = ev.Data.IndexOf("Analyzing");
        if (pos >= 0)
        {
          _counter++;
          string msg = BatInspector.Properties.MyResources.ModelBatDetect2msgProcessing + _counter.ToString() + "/" + _prj.Records.Length.ToString();
          if (_cli)
            DebugLog.log(msg, enLogType.INFO);
          else
            App.Model.Status.Msg = msg;
        }
      }
      else if ((ev.Data?.ToLower().IndexOf("error") > 0) && (ev.Data?.ToLower().IndexOf("error.") < 0))
        DebugLog.log(ev.Data, enLogType.ERROR);
    }

    public override void train()
    {
      throw new NotImplementedException();
    }
  }
}
