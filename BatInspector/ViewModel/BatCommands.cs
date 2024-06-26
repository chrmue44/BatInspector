﻿/********************************************************************************
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
        new OptItem("SplitProject", "split project",0, fctSplitProject)
      }); ; ; 

      _options = new Options(_features, false);
    }


    int fctAdjustReport(List<string> pars, out string ErrText)
    {
      ErrText = "";
      _model.removeDeletedWavsFromReport(_model.Prj.ReportName);
      _model.Prj?.Analysis?.save(_model.Prj.ReportName, _model.Prj.Notes);
      return 0;
    }
    int fctAdjustProject(List<string> pars, out string ErrText)
    {
      ErrText = "";
      _model.Prj?.removeFilesNotInReport();
      return 0;
    }

    int fctSplitProject(List <string> pars, out string ErrText) 
    {
      ErrText = "";
      if((_model.Prj != null) && (_model.Prj.Ok))
      {
        DebugLog.log("starting to split project..", enLogType.INFO);
        int maxFilesPerProject = 600;
        double prjCount = (double)_model.Prj.Records.Length / maxFilesPerProject + 1;
        if (prjCount > (int)prjCount)
          prjCount += 1;
        string prjPath = _model.Prj.PrjDir;
        Project.splitProject(_model.Prj, (int)prjCount, _model.Regions);
        DebugLog.log("start deleting " + prjPath, enLogType.INFO);
        Directory.Delete(prjPath, true );
      }
      return 0;
    }
  }
}
