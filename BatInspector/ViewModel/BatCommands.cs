﻿/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Forms;
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Markup;

namespace BatInspector
{
  public class BatCommands : BaseCommands
  {
    public BatCommands(delegateUpdateProgress delUpd) : base(delUpd)
    {
      _features = new ReadOnlyCollection<OptItem>(new[]
      {
        new OptItem("AdjustReport","remove all entries from report not corresponding to project file", 0, fctAdjustReport),
        new OptItem("AdjustProject","remove all entries from project file not corresponding to report", 0, fctAdjustProject),
        new OptItem("CombineProjects", "combine multiple projects <DirName> <PrjOut> <Prj1> < Prj2> ...",4 , fctCombineProjects ),
        new OptItem("CreatePrjFile", "create project file in directory <dirName>",1, fctCreatePrjFile),
        new OptItem("CreateReport", "create report from annotations <prjDirName>",1, ftcCreateReport ),
        new OptItem("ExportFiles", "export files from project <FilterExpression> <outDir> <allCalls",3, ftcExportFiles ),
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
        DebugLog.log("starting to split project..", enLogType.INFO);
        int maxFilesPerProject = 600;
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
        Project prj = new Project(App.Model.Regions, App.Model.SpeciesInfos, false,
        App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)],
        App.Model.DefaultModelParams.Length);
        string prjDir = Path.Combine(dir, pars[i]);
        if (Project.containsProject(prjDir) != "")
        {
          prj.readPrjFile(prjDir, App.Model.DefaultModelParams);
          if (File.Exists(prj.ReportName))
            prj.Analysis.read(prj.ReportName, App.Model.DefaultModelParams);
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
      string fileName = pars[0];
      ModelParams modelParams = App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)];
      Project prj = new Project(App.Model.Regions, App.Model.SpeciesInfos, false, modelParams, App.Model.DefaultModelParams.Length);
      DirectoryInfo dir = new DirectoryInfo(fileName);
      prj.fillFromDirectory(dir, AppParams.DIR_WAVS);
      return retVal;
    }

    int ftcCreateReport(List<string> pars, out string ErrText)
    {
      int retVal = 0;
      ErrText = "";
      string prjDir = pars[0];
      ModelParams modelParams = App.Model.DefaultModelParams[App.Model.getModelIndex(AppParams.Inst.DefaultModel)];
      Project prj = new Project(App.Model.Regions, App.Model.SpeciesInfos, false, modelParams, App.Model.DefaultModelParams.Length);
      prj.readPrjFile(prjDir, App.Model.DefaultModelParams);
      retVal = App.Model.createReport(prj);
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
  }
}