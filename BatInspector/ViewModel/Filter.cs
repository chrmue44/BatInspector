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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatInspector
{
  public delegate void dlgDelete(int index);
  public class FilterItem
  {
    public int Index { get; set; }
    public string Name { get; set; }
    public string Expression { get; set; }
    public bool IsForAllCalls { get; set; }
  }
  
  public class Filter
  {
    const string VAR_NR_CALLS = "NrOfCalls";
    const string VAR_SPECIES_AUTO = "SpeciesAuto";
    const string VAR_SPECIES_MAN = "SpeciesMan";
    const string VAR_FREQ_MAX = "FreqMax";
    const string VAR_FREQ_MIN = "FreqMin";
    const string VAR_DURATION = "DurationCall";
    const string VAR_PROBABILITY = "Probability";
    const string VAR_SNR = "Snr";
    const string VAR_REMARKS = "Remarks";
    const string VAR_TIME = "RecTime";

    List<FilterItem> _list;
    Expression _expression;
    public List<FilterItem> Items { get { return _list; } }

    public Filter()
    {
      _list = new List<FilterItem>();
      _expression = new Expression(null);
      _expression.Variables.set(VAR_NR_CALLS, 0);
      _expression.Variables.set(VAR_SPECIES_AUTO,"");
      _expression.Variables.set(VAR_SPECIES_MAN,"");
      _expression.Variables.set(VAR_FREQ_MIN,"");
      _expression.Variables.set(VAR_FREQ_MAX,"");
      _expression.Variables.set(VAR_DURATION,"");
      _expression.Variables.set(VAR_PROBABILITY,"");
      _expression.Variables.set(VAR_REMARKS, "");
      _expression.Variables.set(VAR_TIME, 0);
      _expression.Variables.set(VAR_SNR,0.0);
    }

    public bool apply(FilterItem filter, AnalysisFile file)
    {
      bool retVal = false;
      if (filter.IsForAllCalls)
        retVal = true;

      if (file != null)
      {
        foreach (AnalysisCall call in file.Calls)
        {
          _expression.setVariable(VAR_NR_CALLS, file.Calls.Count);
          _expression.setVariable(VAR_SPECIES_AUTO, call.SpeciesAuto);
          _expression.setVariable(VAR_SPECIES_MAN, call.SpeciesMan);
          _expression.setVariable(VAR_FREQ_MIN, call.FreqMin);
          _expression.setVariable(VAR_FREQ_MIN, call.FreqMax);
          _expression.setVariable(VAR_DURATION, call.Duration);
          _expression.setVariable(VAR_PROBABILITY, call.Probability);
          _expression.setVariable(VAR_SNR, call.Snr);
          _expression.setVariable(VAR_TIME, AnyType.getTimeString(file.RecTime));
          AnyType res = _expression.parse(filter.Expression);
          if (filter.IsForAllCalls)
          {
            if ((res.getType() != AnyType.tType.RT_BOOL) || !res.getBool())
              retVal = false;
          }
          else
          {
            if ((res.getType() == AnyType.tType.RT_BOOL) && res.getBool())
              retVal = true;
          }
        }
      }
      return retVal;
    }

    public FilterItem getFilter(string name)
    {
      FilterItem retVal = null;
      foreach(FilterItem it in _list)
      {
        if(it.Name == name)
        {
          retVal = it;
          break;
        }
      }
      return retVal;
    }

    public string getVariables()
    {
      string retVal = "";
      foreach(VarListItem v in _expression.Variables.getVarList(false))
      {
        retVal += v.name + "\n";
      }
      return retVal;
    }
  }
}
