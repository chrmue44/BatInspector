/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace BatInspector
{
  public class BatCommands : BaseCommands
  {
    private ViewModel _model;
    public BatCommands(delegateUpdateProgress delUpd, ViewModel model) : base(delUpd)
    {
      _model = model;
      _features = new ReadOnlyCollection<OptItem>(new[]
      {
        new OptItem("AdjustReport","remove all entries from report not corresponding to project file", 0, fctAdjustReport),
        new OptItem("AdjustProject","remove all entries from project file not corresponding to report", 0, fctAdjustProject),
        new OptItem("SplitProject", "split project",0, fctSplitProject),
        new OptItem("SplitWavFile", "split wav file <fileName> <splitLength> <removeOriginal>",3, fctSplitWavFile),
        new OptItem("SplitJsonAnnotation", "split Json annotation file <fileName> <splitLength> <removeOriginal>",3, fctSplitJsonAnn),
        new OptItem("CreatePrjFile", "create project file in directory <dirName>",1, fctCreatePrjFile),
        new OptItem("CreateReport", "create report from annotations <prjDirName>",1, ftcCreateReport )
      });

      _options = new Options(_features, false);
    }


    int fctAdjustReport(List<string> pars, out string ErrText)
    {
      ErrText = "";
      _model.removeDeletedWavsFromReport(_model.Prj.ReportName);
      _model.Prj?.Analysis?.save(_model.Prj.ReportName, _model.Prj.Notes, _model.Prj.SummaryName);
      return 0;
    }
    int fctAdjustProject(List<string> pars, out string ErrText)
    {
      ErrText = "";
      _model.Prj?.removeFilesNotInReport();
      return 0;
    }

    int fctSplitProject(List<string> pars, out string ErrText) 
    {
      ErrText = "";
      if ((_model.Prj != null) && (_model.Prj.Ok))
      {
        DebugLog.log("starting to split project..", enLogType.INFO);
        int maxFilesPerProject = 600;
        double prjCount = (double)_model.Prj.Records.Length / maxFilesPerProject + 1;
        if (prjCount > (int)prjCount)
          prjCount += 1;
        string prjPath = _model.Prj.PrjDir;
        Project.splitProject(_model.Prj, (int)prjCount, _model.Regions, _model.DefaultModelParams);
        DebugLog.log("start deleting " + prjPath, enLogType.INFO);
        Directory.Delete(prjPath, true);
      }
      return 0;
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
        Bd2AnnFile.splitAnnotation(fileName, splitLength,removeOriginal);
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
      string fileName = pars[0];
      Project prj = new Project(_model.Regions, _model.SpeciesInfos, null, _model.DefaultModelParams);
      DirectoryInfo dir = new DirectoryInfo(fileName);
      prj.fillFromDirectory(dir, AppParams.DIR_WAVS);
      return retVal;
    }

    int ftcCreateReport(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string prjDir = pars[0];
      Project prj = new Project(_model.Regions, _model.SpeciesInfos, null, _model.DefaultModelParams);
      prj.readPrjFile(prjDir, _model.DefaultModelParams);
      retVal = _model.createReport(prj);
      return retVal;
    }
  }
}