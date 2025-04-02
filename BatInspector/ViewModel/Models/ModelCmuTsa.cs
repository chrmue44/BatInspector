/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-05-31                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Forms;
using libParser;
using libScripter;
using System;

using System.IO;


namespace BatInspector
{
  public class ModelCmuTsa : BaseModel
  {
    public const string MODEL_NAME = "CMrnn";
    public const int OPT_INSPECT = 0x01;
    public const int OPT_CUT = 0x02;
    public const int OPT_PREPARE = 0x04;
    public const int OPT_PREDICT1 = 0x08;
    public const int OPT_CONF95 = 0x10;
    public const int OPT_PREDICT2 = 0x40;
    public const int OPT_PREDICT3 = 0x80;
    public const int OPT_CLEANUP = 0x100;

    public ModelCmuTsa(int index) :
      base(index, enModel.rnn6aModel, MODEL_NAME)
    {

    }

    public override int classify(Project prj, bool removeEmptyFiles, bool cli = false)
    {
      int retVal = 0;

      frmStartPredict.showMsg();
      int options = frmStartPredict.Options;
      string reportName = prj.getReportName(this.Index);
      reportName = reportName.Replace("\\", "/");
      ModelParams modPar = prj.AvailableModelParams[this.Index];
      string modPath = Path.IsPathRooted(AppParams.Inst.ModelRootPath) ?
                 AppParams.Inst.ModelRootPath :
                 Path.Combine(AppParams.AppDataPath, AppParams.Inst.ModelRootPath);

      string speciesFile = Path.Combine(modPath, modPar.SubDir,"species.csv");
      addSpeciesColsToReport(reportName, speciesFile);
      string datFile = Path.Combine(prj.PrjDir, "Xdata000.npy");
      string wrkDir = "C:/Users/chrmu/prj/BatInspector/py";
      string args = modPar.Script;

      if ((options & OPT_INSPECT) != 0)
      {
        //internal:
        string dir = Path.Combine(prj.PrjDir, prj.WavSubDir);

        if (File.Exists(reportName))
          File.Delete(reportName);
        BioAcoustics.analyzeFiles(reportName, dir);
        prj.Analysis.read(reportName, App.Model.DefaultModelParams);
      }

      if ((options & (OPT_CUT | OPT_PREPARE | OPT_PREDICT1)) != 0)
      {
        DebugLog.log("preparing files for species prediction", enLogType.INFO);
        prepareFolder(prj.PrjDir);
        if ((options & OPT_CUT) != 0)
          args += " --cut --sampleRate " + AppParams.Inst.SamplingRate.ToString();
        if ((options & OPT_PREPARE) != 0)
          args += " --prepPredict";
        if ((options & OPT_PREDICT1) != 0)
          args += " --predict";
        string modelDir = modPath + "/" + modPar.SubDir;
        args += " --csvcalls " + reportName +
             " --root " + modelDir + " --specFile " + modelDir + "/speies.csv" +
             " --dataDir " + prj.PrjDir +
             " --data " + datFile +
             " --model " + this.Name +
             " --predCol " + Cols.SPECIES;
        retVal = _proc.launchCommandLineApp(AppParams.PythonBin, null, wrkDir, true, args, true, true);
      }

      if ((options & OPT_CONF95) != 0)
      {
        DebugLog.log("executing confidence test prediction", enLogType.INFO);
        prj.Analysis.read(reportName, App.Model.DefaultModelParams);
        prj.Analysis.checkConfidence(App.Model.SpeciesInfos);
        prj.Analysis.save(reportName, prj.Notes, prj.SummaryName);
        prj.Analysis.read(reportName, App.Model.DefaultModelParams);
      }

      if ((options & OPT_CLEANUP) != 0)
      {
        DebugLog.log("cleaning up temporary files", enLogType.INFO);
        cleanupTempFiles(prj.PrjDir);
      }
      return retVal;
    }

    public override void train()
    {
      throw new NotImplementedException();
    }

    public static Csv createReport()
    {
      Csv csv = new Csv();
      csv.clear();
      csv.addRow();
      string[] header =
      {
        Cols.NAME,
        Cols.NR,
        Cols.SPECIES,
        Cols.SAMPLERATE,
        Cols.FILE_LEN,
        Cols.F_MAX_AMP,
        Cols.F_MIN,
        Cols.F_MAX,
        Cols.F_KNEE,
        Cols.DURATION,
        Cols.START_TIME,
        Cols.BANDWIDTH,
        Cols.F_START,
        Cols.F_25,
        Cols.F_CENTER,
        Cols.F_75,
        Cols.F_END,
        Cols.FC,
        Cols.F_BW_KNEE_FC,
        Cols.BIN_MAX_AMP,
        Cols.PC_F_MAX_AMP,
        Cols.PC_F_MAX,
        Cols.PC_F_MIN,
        Cols.PC_KNEE,
        Cols.TEMP_BW_KNEE_FC,
        Cols.SLOPE,
        Cols.KALMAN_SLOPE,
        Cols.CURVE_NEG,
        Cols.CURVE_POS_START,
        Cols.CURVE_POS_END,
        Cols.MID_OFFSET,
        Cols.SMOTTHNESS,
        Cols.SNR,
        Cols.SPECIES_MAN,
        Cols.PROBABILITY,
        Cols.REMARKS
      };
      csv.initColNames(header, true);
      return csv;
    }

    private void prepareFolder(string dirName, bool delete = true)
    {
      createDir(dirName, "bat", delete);
      createDir(dirName, "wav", delete);
      createDir(dirName, "dat", delete);
      createDir(dirName, "log", delete);
      createDir(dirName, "img", delete);
    }


    private void addSpeciesColsToReport(string report, string speciesFile)
    {
      Csv spec = new Csv();
      Csv rep = new Csv();
      spec.read(speciesFile, ";", true);
      rep.read(report, ";", true);
      if (rep.findInRow(1, "----") < 1)
      {
        int c = rep.ColCnt + 1;
        rep.insertCol(c, "", "----");
      }
      if (rep.findInRow(1, Cols.SPECIES) < 1)
      {
        int c = rep.ColCnt + 1;
        rep.insertCol(c, "", Cols.SPECIES);
      }
      for (int r = 2; r <= spec.RowCnt; r++)
      {
        string species = spec.getCell(r, 1);
        if (rep.findInRow(1, species) < 1)
        {
          int c = rep.ColCnt + 1;
          rep.insertCol(c, "", species);
        }
      }
      rep.save();
    }

    private void cleanupTempFiles(string dirName)
    {
      removeDir(dirName, "bat");
      removeDir(dirName, "wav");
      removeDir(dirName, "dat");
      removeDir(dirName, "log");
      removeDir(dirName, "img");
      var dir = new DirectoryInfo(dirName);
      foreach (var file in dir.EnumerateFiles("*.npy"))
      {
        file.Delete();
      }
    }   
  }
}
