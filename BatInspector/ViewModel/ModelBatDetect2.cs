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

using libScripter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Media.Animation;

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
      string wavDir = prj.PrjDir + prj.WavSubDir;
      string annDir = prj.PrjDir + AppParams.ANNOTATION_SUBDIR;
      string args = wavDir + " " + annDir + " " + _minProb.ToString(CultureInfo.InvariantCulture);
      string wrkDir = AppParams.Inst.RootPath + "/" + AppParams.Inst.Models[this.Index].Dir;
      string cmd = AppParams.Inst.RootPath + "/" + AppParams.Inst.Models[this.Index].Script;
      int retVal = _proc.LaunchCommandLineApp(cmd, null,  wrkDir, true, args, true, true);
      string reportName = prj.PrjDir + "/" + AppParams.Inst.Models[this.Index].ReportName;
      createReportFromAnnotations(0.5, prj.SpeciesInfos, wavDir, annDir, reportName);
      cleanup(prj.PrjDir);
      return retVal;
    }

    public override void train()
    {
      throw new NotImplementedException();
    }

    public void createReportFromAnnotations(double minProb, List<SpeciesInfos> speciesInfos, string wavDir, string annDir, string reportName)
    {
      Csv report = new Csv();
      string colSpecies = AppParams.Inst.Models[this.Index].ReportColumn;
      string[] header = { "name", "recTime", "nr", colSpecies, "sampleRate", "FileLen", "freq_max_amp","freq_min", "freq_max", "duration","callInterval","start","SpeciesMan","prob","remarks"};
    
      report.initColNames(header, true);

      string[] files = Directory.GetFiles(annDir, "*.wav.csv", SearchOption.AllDirectories);
      foreach (string file in files) 
      {
        // read infoFile
        string wavName = wavDir + "/" + Path.GetFileName(file).Replace(".csv", "");
        string infoName = wavDir + "/" + Path.GetFileName(file).Replace(".wav.csv", ".xml");
        string sampleRate = "?";
        string fileLen = "?";
        string recTime = "?";
        BatRecord info = ElekonInfoFile.read(infoName);
        if(info != null)
        {
          sampleRate = info.Samplerate.Replace(" Hz", ""); ;
          fileLen = info.Duration.Replace(" Sec","");
          recTime = info.DateTime;
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
        for(int row = 2; row <= rowcnt; row++) 
        {
          report.addRow();
          int repRow = report.RowCnt;
          report.setCell(repRow, "sampleRate", sampleRate);
          report.setCell(repRow, "FileLen", fileLen);
          report.setCell(repRow, "recTime", recTime);
          report.setCell(repRow, "name", wavName);
          int id = csvAnn.getCellAsInt(row, "id");
          report.setCell(repRow, "nr", id + 1);
          double startTime = csvAnn.getCellAsDouble(row, "start_time");
          report.setCell(repRow, "start", startTime, 3);
          double duration = -1;
          double callInterval = -0.001;
          if (csvFeat != null)
          {
            double fMaxAmp = csvFeat.getCellAsDouble(row, "max_power");
            report.setCell(repRow, "freq_max_amp", fMaxAmp);
            duration = csvFeat.getCellAsDouble(row, "duration");
            callInterval = csvFeat.getCellAsDouble(row, "call_interval");
            if (callInterval < 0)
              callInterval = -0.001;
          }
          else
          {
            double endTime = csvAnn.getCellAsDouble(row, "end_time");
            duration = (endTime - startTime);
          }
          report.setCell(repRow, "callInterval", callInterval * 1000, 1);
          report.setCell(repRow, "duration", duration * 1000, 1);
          double fMin = csvAnn.getCellAsDouble(row, "low_freq");
          report.setCell(repRow, "freq_min", fMin, 1);
          double fMax = csvAnn.getCellAsDouble(row, "high_freq");
          report.setCell(repRow, "freq_max", fMax, 1);
          string latin = csvAnn.getCell(row, "class");
          double prob = csvAnn.getCellAsDouble(row, "class_prob");
          report.setCell(repRow, "prob", prob);
          string abbr = "";
          if (prob < minProb)
            abbr = "??PRO[";
          SpeciesInfos specInfo = SpeciesInfos.findLatin(latin, speciesInfos);
          if((info != null) && (specInfo != null))
            abbr += specInfo.Abbreviation;
          if (prob < minProb)
            abbr += "]";
          report.setCell(repRow, colSpecies, abbr);
        }
      }
      report.saveAs(reportName);
    }

    void cleanup(string root)
    {
      removeDir(root, AppParams.ANNOTATION_SUBDIR);
    }
  }
}
