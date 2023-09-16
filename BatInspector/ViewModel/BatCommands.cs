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
      }); ; 

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
  }
}
