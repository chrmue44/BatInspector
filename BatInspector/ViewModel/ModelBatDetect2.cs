/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2023-04-04                                       
 *   Copyright (C) 2023: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;


namespace BatInspector
{
  public class ModelBatDetect2 : BaseModel
  {
    double _minProb = 0.5;
    public double MinProb {  get { return _minProb; } set { _minProb = value; } }

    public ModelBatDetect2(int index) : 
      base(index, enModel.BAT_DETECT2)
    {
    }

    public override int classify(Project prj)
    {
      string wavDir = prj.PrjDir + "/" +prj.WavSubDir;
      string annDir = prj.PrjDir + AppParams.ANNOTATION_SUBDIR;
      string args = AppParams.Inst.ModelRootPath + " " + wavDir + " " +
                   annDir + " " + _minProb.ToString(CultureInfo.InvariantCulture);
      string wrkDir = AppParams.Inst.ModelRootPath + "/" + AppParams.Inst.Models[this.Index].Dir;
      string cmd = wrkDir + "/" + AppParams.Inst.Models[this.Index].Script;
       int retVal = _proc.LaunchCommandLineApp(cmd, null, wrkDir, true, args, true, true);
      bool ok = createReportFromAnnotations(0.5, prj.SpeciesInfos, wavDir, annDir, prj.ReportName);
      if (ok)
      {
        cleanup(prj.PrjDir);
        prj.Analysis.read(prj.ReportName);
        prj.removeFilesNotInReport();
      }
      return retVal;
    }

    public override void train()
    {
      throw new NotImplementedException();
    }

    public bool createReportFromAnnotations(double minProb, List<SpeciesInfos> speciesInfos, string wavDir, string annDir, string reportName)
    {
      bool retVal = true;
      try
      {
        string colSpecies = AppParams.Inst.Models[this.Index].ReportColumn;
        Csv report = createReport(colSpecies);

        string[] files = Directory.GetFiles(annDir, "*.wav.csv", SearchOption.AllDirectories);
        foreach (string file in files)
        {
          // read infoFile
          string wavName = /*wavDir + "/" + */ Path.GetFileName(file).Replace(".csv", "");
          string infoName = wavDir + "/" + Path.GetFileName(file).Replace(".wav.csv", ".xml");
          string sampleRate = "?";
          string fileLen = "?";
          string recTime = "?";
          double lat = 0.0;
          double lon = 0.0;
          BatRecord info = ElekonInfoFile.read(infoName);
          if (info != null)
          {
            sampleRate = info.Samplerate.Replace(" Hz", ""); ;
            fileLen = info.Duration.Replace(" Sec", "");
            recTime = info.DateTime;
            ElekonInfoFile.parsePosition(info, out lat, out lon);
          }

          // read annontation for one wav file
          Csv csvAnn = new Csv();
          csvAnn.read(file, ",", true);
          string fileFeat = file.Replace("wav.csv", "wav_spec_features.csv");
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
              double fMaxAmp = csvFeat.getCellAsDouble(row, "max_power");
              report.setCell(repRow, Cols.F_MAX_AMP, fMaxAmp);
              duration = csvFeat.getCellAsDouble(row, "duration");
              callInterval = csvFeat.getCellAsDouble(row, "call_interval");
              if (callInterval < 0)
                callInterval = -0.001;
              bandwidth = csvFeat.getCellAsDouble(row, "bandwidth");
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
            report.setCell(repRow, Cols.SNR, -1.0);
            string latin = csvAnn.getCell(row, "class");
            double prob = csvAnn.getCellAsDouble(row, "class_prob");
            report.setCell(repRow, Cols.PROBABILITY, prob);
            string abbr = "";
            if (prob < minProb)
              abbr = "??PRO[";
            SpeciesInfos specInfo = SpeciesInfos.findLatin(latin, speciesInfos);
            if ((info != null) && (specInfo != null))
              abbr += specInfo.Abbreviation;
            if (prob < minProb)
              abbr += "]";
            report.setCell(repRow, colSpecies, abbr);
            report.setCell(repRow, Cols.SPECIES_MAN, "todo");
            report.setCell(repRow, Cols.REMARKS, "");
          }
        }
        string dir = Path.GetDirectoryName(reportName);
        if(!Directory.Exists(dir))
          Directory.CreateDirectory(dir);
        report.saveAs(reportName);
      }
      catch (Exception e) 
      {
        retVal = false;
        DebugLog.log("error creating report from model predicitons, " + e.ToString(), enLogType.ERROR);
      }
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

    void cleanup(string root)
    {
      removeDir(root, AppParams.ANNOTATION_SUBDIR);
    }
  }
}
