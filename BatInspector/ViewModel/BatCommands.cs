/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
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
using System.Collections.ObjectModel;

namespace BatInspector
{
  public class BatCommands : BaseCommands
  {
    ViewModel _model;
    public BatCommands(delegateUpdateProgress delUpd, ViewModel model) : base(delUpd)
    {
      _model = model;
      _features = new ReadOnlyCollection<OptItem>(new[]
      {
        new OptItem("Test", "test", 1, fctTest),
        new OptItem("AdjustReport","remove all entries from report not corresponding to project file", 0, fctAdjustReport),
        new OptItem("initSpecInfos","initialize species infos in settings",0,fctInitSpecInfos),
        new OptItem("log","log <message> <type>", 2, fctLog)
      }); ; 

      _options = new Options(_features, false);
    }

    int fctTest(List<string> pars, out string ErrText)
    {
      ErrText = "";
      return 0;
    }

    int fctAdjustReport(List<string> pars, out string ErrText)
    {
      ErrText = "";
      string report = _model.PrjPath + "/report.csv";
      _model.removeDeletedWavsFromReport(report);
      return 0;
    }

    int fctInitSpecInfos(List<string> pars, out string ErrText)
    {
      ErrText = "";
      _model.Settings.initSpeciesInfos();
      return 0;
    }

    int fctLog(List<string> pars, out string ErrText)
    {
      ErrText = "";
      enLogType lType = enLogType.INFO;
      Expression exp = new Expression(_parser.VarTable.VarList);
      string res = exp.parseToString(pars[0]);
      Enum.TryParse(pars[1], out lType);
      DebugLog.log(res, lType);
      _model.Settings.initSpeciesInfos();
      return 0;
    }

  }
}
