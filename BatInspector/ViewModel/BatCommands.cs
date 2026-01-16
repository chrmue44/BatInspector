/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Forms;
using libParser;
using libScripter;
using OxyPlot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;

namespace BatInspector
{
  public class BatCommands : BaseCommands
  {
    public BatCommands(delegateUpdateProgress delUpd) : base(delUpd)
    {
      _features = new ReadOnlyCollection<OptItem>(new[]
      {
        new OptItem("AddProjectToDb", "add currently open project to DB", 0, ftcAddPrjToDb),
        new OptItem("AdjustReport","remove all entries from report not corresponding to project file", 0, fctAdjustReport),
        new OptItem("AdjustProject","remove all entries from project file not corresponding to report", 0, fctAdjustProject),
        new OptItem("ApplyMicCorrection", "apply microphone correction <softening>",1, fctMicCorrection),
        new OptItem("CheckModelPerformance", "check model performance against man. evaluation", 0, fctCheckModelPerformance),
        new OptItem("CombineProjects", "combine multiple projects <DirName> <PrjOut> <Prj1> < Prj2> ...",4 , fctCombineProjects ),
        new OptItem("CopyFile", "copy file to directory <fileName> <targetDir>",2, fctCopyFile),
        new OptItem("CountCallsInTrainingData", "count calls in fctFindMissingFilesInTrainingData data and write to csv file <annPath> <csvFile>",2,fctCountCallsInTrainingData),
        new OptItem("CreateMicFreqResponse", "create mic response <wavFile> <outFile>", 2, fctCreateMicResponse),
        new OptItem("CreatePrjFile", "create project file in directory <dirName>",1, fctCreatePrjFile),
        new OptItem("CreateAnnsFromBra", "create training annotations from Elekon BRA files <wavDir> <annDir> <timeOffs> <fMinOffs>", 4, fctCreateAnnsFromBra),
        new OptItem("CreateReport", "create report from annotations <prjDirName>",1, ftcCreateReport ),
        new OptItem("CreateTrainingReport", "create report of training annotations <srcDir> <reportName>", 2, fctTrainReport),
        new OptItem("DelProjectFromDb", "delete currently open project from DB", 0, ftcDelPrjFromDb),
        new OptItem("EditWav", "<option> edit wav file", 1, fctEditWav),
        new OptItem("ExportFiles", "export files from project <FilterExpression> <outDir> <allCalls",3, ftcExportFiles ),
        new OptItem("FindMissingFilesInTrainingData", "find missing files in training data <wavPath>, <annPath>",2,fctFindMissingFilesInTrainingData),
        new OptItem("SplitProject", "split project",0, fctSplitProject),
        new OptItem("SplitWavFile", "split wav file <fileName> <splitLength> <removeOriginal>",3, fctSplitWavFile),
        new OptItem("SplitJsonAnnotation", "split Json annotation file <fileName> <splitLength> <removeOriginal>",3, fctSplitJsonAnn),
        });

      _options = new Options(_features, false);
    }


    int fctAdjustReport(List<string> pars, out string ErrText)
    {
      ErrText = "";
      App.Model.removeDeletedWavsFromReport(App.Model.Prj.ReportName);
      App.Model.Prj?.Analysis?.save(App.Model.Prj.ReportName, App.Model.Prj.Notes, App.Model.Prj.SummaryName);
      return 0;
    }
    int fctAdjustProject(List<string> pars, out string ErrText)
    {
      ErrText = "";
      App.Model.Prj?.removeFilesNotInReport();
      return 0;
    }

    int fctSplitProject(List<string> pars, out string ErrText)
    {
      ErrText = "";
      if ((App.Model.Prj != null) && (App.Model.Prj.Ok))
      {
        int maxFilesPerProject = 600;
        if (pars.Count > 0)
        {
          bool ok = int.TryParse(pars[0], out int val);
          if (ok)
            maxFilesPerProject = val;
        }
        DebugLog.log("starting to split project..", enLogType.INFO);
        double prjCount = (double)App.Model.Prj.Records.Length / maxFilesPerProject + 1;
        if (prjCount > (int)prjCount)
          prjCount += 1;
        string prjPath = App.Model.Prj.PrjDir;
        ModelParams modelParams = App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)];
        Project.splitProject(App.Model.Prj, (int)prjCount, App.Model.Regions, modelParams, App.Model.DefaultModelParams.Length);
        DebugLog.log("start deleting " + prjPath, enLogType.INFO);
        Directory.Delete(prjPath, true);
      }
      return 0;
    }


    int fctCombineProjects(List<string> pars, out string ErrText)
    {
      ErrText = "";
      string dir = pars[0];
      string prjName = pars[1];
      int retVal = 0;
      List<Project> prjs = new List<Project>();
      for (int i = 2; i < pars.Count; i++)
      {
        Project prj = new Project(false,
        App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)],
        App.Model.DefaultModelParams.Length);
        string prjDir = Path.Combine(dir, pars[i]);
        if (Project.containsProject(prjDir) != "")
        {
          prj.readPrjFile(prjDir);
          if (File.Exists(prj.ReportName))
            prj.Analysis.read(prj.ReportName, App.Model.DefaultModelParams, prj.MetaData);
          prjs.Add(prj);
        }
        else
        {
          retVal = 1;
          ErrText = $"project '{pars[i]}' not found";
        }
      }
      if (retVal == 0)
        Project.combineProjects(prjs, dir, prjName);
      return retVal;
    }


    int fctSplitWavFile(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string fileName = pars[0];
      double splitLength;
      bool ok = double.TryParse(pars[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out splitLength);
      bool removeOriginal = (pars[2] == "1");
      if (ok)
        WavFile.splitWav(fileName, splitLength, removeOriginal);
      else
      {
        retVal = 1;
        ErrText = "unable to read 2nd parameter as double";
      }
      return retVal;
    }

    int fctSplitJsonAnn(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string fileName = pars[0];
      double splitLength;
      bool ok = double.TryParse(pars[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out splitLength);
      bool removeOriginal = (pars[2] == "1");
      if (ok)
        Bd2AnnFile.splitAnnotation(fileName, splitLength, removeOriginal);
      else
      {
        retVal = 1;
        ErrText = "unable to read 2nd parameter as double";
      }
      return retVal;
    }

    int fctCreatePrjFile(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      if (pars.Count > 0)
      {
        string fileName = pars[0];
        bool isBirdPrj = false;
        if (pars.Count > 1)
          isBirdPrj = pars[1] == "Birds";
        ModelParams modelParams = App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)];
        Project prj = new Project(false, modelParams, App.Model.DefaultModelParams.Length);
        DirectoryInfo dir = new DirectoryInfo(fileName);
        prj.fillFromDirectory(dir, AppParams.DIR_WAVS);
      }
      else
      {
        retVal = 1;
        ErrText = "parameter 1 missing";
      }
      return retVal;
    }

    int ftcCreateReport(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string prjDir = pars[0];
      ModelParams modelParams = App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)];
      Project prj = Project.createFrom(prjDir);
      if (prj.Ok)
        retVal = App.Model.createReport(prj);
      else
        retVal = 1;
      return retVal;
    }

    int fctTrainReport(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string rootDir = pars[0];
      string report = pars[1];
      retVal = SumAnnotations.createReport(rootDir, report);
      if (retVal != 0)
        ErrText = "error creating training data report";
      return retVal;
    }


    int ftcExportFiles(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      string filterExp = pars[0];
      string outDir = pars[1];
      bool allCalls = pars[2] != "0";
      FilterItem filterItem = new FilterItem(0, "temp", filterExp, allCalls);
      if (App.Model.Prj?.Ok == true)
      {
        App.Model.Prj.applyFilter(App.Model.Filter, filterItem);
        App.Model.Prj.exportFiles(outDir);
        filterItem.Expression = "";
        App.Model.Prj.applyFilter(App.Model.Filter, filterItem);
        ErrText = "";
      }
      else
      {
        ErrText = "open project first";
        retVal = 1;
      }
      return retVal;
    }

    int fctMicCorrection(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      double softening = 0.0;
      ErrText = "";
      double.TryParse(pars[0], NumberStyles.Any, CultureInfo.InvariantCulture, out softening);
      if (App.Model.Prj?.Ok == true)
      {
        App.Model.Prj.applyMicCorrection(softening);
      }
      else
      {
        ErrText = "open project first";
        retVal = 1;
      }
      return retVal;
    }

    int fctCreateMicResponse(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string wavFile = pars[0];
      string micFile = pars[1];
      if (File.Exists(wavFile))
      {
        SoundEdit wav = new SoundEdit();
        wav.readWav(wavFile);
        FreqResponseRecord[] r = wav.createMicSpectrumFromNoiseFile();
        string text = "";
        foreach (FreqResponseRecord rec in r)
        {
          text += $"{rec.Frequency.ToString("0.0", CultureInfo.InvariantCulture)}, {rec.Amplitude.ToString("0.0", CultureInfo.InvariantCulture)}\n".ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        File.WriteAllText(micFile, text);
      }
      else
      {
        ErrText = $"error: {wavFile} not existing";
        retVal = 1;
      }
      return retVal;
    }

    int fctCheckModelPerformance(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      double threshhold = 0.5;
      if (pars.Count > 0)
        double.TryParse(pars[0], NumberStyles.Any, CultureInfo.InvariantCulture, out threshhold);

      if (App.Model.Prj.Ok)
      {
        string modelName = App.Model.Prj.AvailableModelParams[App.Model.Prj.SelectedModelIndex].Name +
                           App.Model.Prj.AvailableModelParams[App.Model.Prj.SelectedModelIndex].DataSet;
        string perf = Path.Combine(App.Model.Prj.PrjDir, "performance" + modelName + ".csv");
        string annDir = Path.Combine(App.Model.Prj.PrjDir, "Annotations");
        BaseModel.checkModelPerformance(App.Model.Prj, annDir, perf, threshhold);
      }
      else
      {
        ErrText = "CheckModelPerformance: open project first";
      }
      retVal = ErrText == "" ? 0 : 1;
      return retVal;
    }

    int fctFindMissingFilesInTrainingData(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string wavPath = pars[0];
      string annPath = pars[1];
      BaseModel.findMissingFilesInTrainingData(wavPath, annPath);
      return retVal;
    }

    int fctCountCallsInTrainingData(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string annPath = pars[0];
      string csvFile = pars[1];
      BaseModel.countCallsInTrainingData(annPath, csvFile);
      return retVal;
    }

    int ftcAddPrjToDb(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      if ((App.Model.Prj != null) && (App.Model.Prj.Ok))
      {
        if (App.Model.MySQL.IsConnected)
        {
          bool overRideWarning = false;
          if ((pars.Count > 0) && (pars[0] == "1"))
            overRideWarning = true;
          retVal = App.Model.MySQL.DbBats.addProjectToDb(App.Model.Prj, overRideWarning);
          if (retVal != 0)
            ErrText = "error adding project to MySQL database";
        }
        else
          ErrText = "Databese is not connected!";
      }
      else
        ErrText = "project not open or erroneous";
      retVal = string.IsNullOrEmpty(ErrText) ? 0 : 1;
      return retVal;
    }

    int ftcDelPrjFromDb(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      if ((App.Model.Prj != null) && (App.Model.Prj.Ok))
      {
    /*    if (!App.Model.MySQL.IsConnected)
        {
          FrmConnectMysql f = new FrmConnectMysql();
          if (f.ShowDialog() == true)
            App.Model.MySQL.connect(f.Server, f.DataBase, f.User, f.PassWord);
        }  */
        if (App.Model.MySQL.IsConnected)
        {
          retVal = App.Model.MySQL.DbBats.deleteProjectFromDb(App.Model.Prj.PrjId);
          if (retVal != 0)
            ErrText = "error deleting project from MySQL database";
        }
        else
          ErrText = "Databese is not connected!";
      }
      else
        ErrText = "project not open or erroneous";
      retVal = string.IsNullOrEmpty(ErrText) ? 0 : 1;
      return retVal;
    }


    int fctCopyFile(List<string> pars, out string ErrText)
    {

      int retVal = 0;
      ErrText = "";
      string[] fileName =  new string[]{ pars[0] };
      string targetDir = pars[1];
      bool overWrite = (pars.Count > 2) && ((pars[2] == "1") || (pars[2]=="TRUE")) ? true : false;
      Utils.copyFiles(fileName, targetDir, false, overWrite);
      return retVal;
    }
    
    int fctCreateAnnsFromBra(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";

      string wavDir = pars[0];
      string annDir = pars[1];
      string speciesName = "Myotis daubentonii";
      string eventType = "Echolocation";
      if (pars.Count > 4)
        speciesName = pars[4];
      if (pars.Count > 5)
        eventType = pars[5];

      bool ok = double.TryParse(pars[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double timeOffs);
      if(ok)
      {
        ok = double.TryParse(pars[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double fMinOffs);
        if(ok)
        {
          string[] files = Directory.GetFiles(wavDir, "*.wav");
          foreach(string file in files)
          {
            string braName = file.Replace(".wav", ".bra");
            Bd2AnnFile ann = ElekonBra.convertToBd2Ann(braName, timeOffs, fMinOffs, speciesName, eventType);
            string annName = Path.Combine(annDir, Path.GetFileName(file)) + ".json";
            ann.saveAs(annName);
          }
        }
      }
      if (!ok)
        ErrText = $"CreateAnnsFromBra, erroneous parameters: {pars[2]}, {pars[3]}, expected doubles";

      return retVal;
    }


    int fctEditWav(List<string> pars, out string ErrText)
    {

      int retVal = 0;
      ErrText = "";

      switch(pars[0])
      {
        case "OPEN":
          {
            if (pars.Count > 1)
            {
              string wavName = pars[1];
              if (App.Model.ZoomView.Waterfall == null)
                App.Model.ZoomView.initWaterfallDiagram(wavName, enMetaData.AUTO);
              int ret = App.Model.ZoomView.Waterfall.Audio.readWav(wavName);
              if(ret != 0)
              {
                ErrText = $"unable to read wav file {wavName}";
              }
            }
            else
              ErrText = "EditWav: not enough parameters for option OPEN";
          }
          break;

        case "BANDPASS":
          {
            if (pars.Count > 2)
            {
              bool ok = double.TryParse(pars[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double fMin);
              ok &= double.TryParse(pars[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double fMax);
              if (ok)
              {
                App.Model.ZoomView.Waterfall.Audio.FftForward();
                App.Model.ZoomView.Waterfall.Audio.bandpass(fMin, fMax);
                App.Model.ZoomView.Waterfall.Audio.FftBackward();
              }
              else
                ErrText = $"EditWav: erroneous parameters: {fMax}, {fMax}";
            }
            else
              ErrText = "EditWav: not enough parameters for option 'BANDPASS";
          }
          break;

        case "NORM":
          {
            double amplitude = 0.95;
            if (pars.Count > 1)
            {
              bool ok = double.TryParse(pars[1], NumberStyles.Any, CultureInfo.InvariantCulture, out amplitude);
              if (!ok)
                ErrText = $"EditWav: erroneous parameters ({pars[1]}) for option 'NORM'";
            }
            App.Model.ZoomView.Waterfall.Audio.normalize(amplitude);
          }
          break;

        case "DAMP":
          {
            if (pars.Count > 2)
            {
              bool ok = double.TryParse(pars[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double dist1);
              ok &= double.TryParse(pars[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double dist2);
              if (ok)
              {
                if ((dist1 > 0) && (dist2 > dist1))
                  App.Model.ZoomView.Waterfall.Audio.applyDampening(20, 50, dist1, dist2);
                else
                  ErrText = "EditWav: distance 1 must be > 0, and distance 2 > distance 1";
              }
              else
                ErrText = $"EditWav: erroneous parameters ({pars[1]}, {pars[2]}) for option 'DAMP'";
            }
            else
              ErrText = "EditWav: not enough parameters for option 'DAMP'";
          }
          break;

        case "ADD":
          if (pars.Count > 2)
          {
            string wavName = pars[1];
            bool ok = double.TryParse(pars[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double fact);
            if(ok)
            {
              int ret = App.Model.ZoomView.Waterfall.Audio.addSignal(wavName, fact);
              if (ret != 0)
                ErrText = "EditWav: addition of external WAV failed: added wav, does'n exist, is to short or factor erroneous";
            }
            else
              ErrText = $"EditWav: erroneous parameter ({pars[2]}) for option 'ADD'";

          }
          else
            ErrText = "EditWav: not enough parameters for option 'ADD'";
          break;

        case "SAVE":
          if(pars.Count > 1)
            App.Model.ZoomView.Waterfall.Audio.saveAs(pars[1]);
          else
            App.Model.ZoomView.Waterfall.Audio.save();
          break;
      }

      if (ErrText != "")
        retVal = 1;
      return retVal;
    }
  }
}