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
using BatInspector.Properties;
using libParser;
using libScripter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BatInspector
{
  public class FilterVarItem
  {
    public string VarName { get; set; }
    public bool AvailableFile { get; set; } 
    public bool AvailableCall { get;set; }
    public bool AvailableSumReport { get; set; }

    public FilterVarItem()
    {

    }
    public FilterVarItem(string varName, bool availableFile, bool availableCall, bool availableSumReport)
    {
      VarName = varName;
      AvailableFile = availableFile;
      AvailableCall = availableCall;
      AvailableSumReport = availableSumReport;
    }
  }

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
    const string VAR_FREQ_MAX_AMP = "FreqMaxAmp";
    const string VAR_DURATION = "DurationCall";
    const string VAR_PROBABILITY = "Probability";
    const string VAR_SNR = "Snr";
    const string VAR_REMARKS = "Remarks";
    const string VAR_TIME = "RecTime";

    List<FilterItem> _list;
    Expression _expression;
    public List<FilterItem> Items { get { return _list; } }
    
    List<FilterVarItem> _vars;
    public Filter()
    {
      _list = new List<FilterItem>();
      _expression = new Expression(null);
      _vars = new List<FilterVarItem>
      {
        new FilterVarItem (VAR_NR_CALLS, true, false, false ),
        new FilterVarItem (VAR_SPECIES_AUTO, true, true, false ),
        new FilterVarItem (VAR_SPECIES_MAN, true, true, true ),
        new FilterVarItem (VAR_FREQ_MIN, true, true, false ),
        new FilterVarItem (VAR_FREQ_MAX, true, true, false ),
        new FilterVarItem (VAR_FREQ_MAX_AMP, true, true, false ),
        new FilterVarItem (VAR_DURATION, true, true, false ),
        new FilterVarItem (VAR_PROBABILITY, true, true, false ),
        new FilterVarItem (VAR_REMARKS, true, true, false ),
        new FilterVarItem (VAR_TIME, true, false, false ),
        new FilterVarItem (VAR_SNR, true, true, false ),
    };

      _expression.Variables.set(VAR_NR_CALLS, 0);
      _expression.Variables.set(VAR_SPECIES_AUTO,"");
      _expression.Variables.set(VAR_SPECIES_MAN,"");
      _expression.Variables.set(VAR_FREQ_MIN,"");
      _expression.Variables.set(VAR_FREQ_MAX,"");
      _expression.Variables.set(VAR_FREQ_MAX_AMP, "");
      _expression.Variables.set(VAR_DURATION,"");
      _expression.Variables.set(VAR_PROBABILITY,"");
      _expression.Variables.set(VAR_REMARKS, "");
      _expression.Variables.set(VAR_TIME, 0);
      _expression.Variables.set(VAR_SNR,0.0);
    }

    public bool apply(FilterItem filter, AnalysisCall call, string remarks, string time, out bool ok)
    {
      bool retVal = false;
      _expression.setVariable(VAR_REMARKS, remarks);
      _expression.setVariable(VAR_TIME, time);
      _expression.setVariable(VAR_SPECIES_AUTO, call.getString(Cols.SPECIES));
      _expression.setVariable(VAR_SPECIES_MAN, call.getString(Cols.SPECIES_MAN));
      _expression.setVariable(VAR_FREQ_MIN, call.getDouble(Cols.F_MIN));
      _expression.setVariable(VAR_FREQ_MAX, call.getDouble(Cols.F_MAX));
      _expression.setVariable(VAR_FREQ_MAX_AMP, call.getDouble(Cols.F_MAX_AMP));
      _expression.setVariable(VAR_DURATION, call.getDouble(Cols.DURATION));
      _expression.setVariable(VAR_PROBABILITY, call.getDouble(Cols.PROBABILITY));
      _expression.setVariable(VAR_SNR, call.getDouble(Cols.SNR));
      AnyType res = _expression.parse(filter.Expression);
      ok = _expression.Errors == 0;
      if ((res.getType() == AnyType.tType.RT_BOOL) && res.getBool())
        retVal = true;
      return retVal;
    }

    public bool apply(FilterItem filter, AnalysisFile file)
    {
      bool retVal = false;
      if (filter.IsForAllCalls)
        retVal = true;

      if (file != null)
      {
        _expression.setVariable(VAR_REMARKS, file.getString(Cols.REMARKS));
        _expression.setVariable(VAR_TIME, AnyType.getTimeString(file.RecTime));
        foreach (AnalysisCall call in file.Calls)
        {
          _expression.setVariable(VAR_NR_CALLS, file.Calls.Count);
          _expression.setVariable(VAR_SPECIES_AUTO, call.getString(Cols.SPECIES));
          _expression.setVariable(VAR_SPECIES_MAN, call.getString(Cols.SPECIES_MAN));
          _expression.setVariable(VAR_FREQ_MIN, call.getDouble(Cols.F_MIN));
          _expression.setVariable(VAR_FREQ_MAX, call.getDouble(Cols.F_MAX));
          _expression.setVariable(VAR_FREQ_MAX_AMP, call.getDouble(Cols.F_MAX_AMP));
          _expression.setVariable(VAR_DURATION, call.getDouble(Cols.DURATION));
          _expression.setVariable(VAR_PROBABILITY, call.getDouble(Cols.PROBABILITY));
          _expression.setVariable(VAR_SNR, call.getDouble(Cols.SNR));
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

    public bool apply(FilterItem filter, AnalysisCall call)
    {
      bool retVal = false;

      _expression.setVariable(VAR_SPECIES_AUTO, call.getString(Cols.SPECIES));
      _expression.setVariable(VAR_SPECIES_MAN, call.getString(Cols.SPECIES_MAN));
      _expression.setVariable(VAR_FREQ_MIN, call.getDouble(Cols.F_MIN));
      _expression.setVariable(VAR_FREQ_MIN, call.getDouble(Cols.F_MAX));
      _expression.setVariable(VAR_DURATION, call.getDouble(Cols.DURATION));
      _expression.setVariable(VAR_PROBABILITY, call.getDouble(Cols.PROBABILITY));
      _expression.setVariable(VAR_SNR, call.getDouble(Cols.SNR));
      AnyType res = _expression.parse(filter.Expression);
      
      if ((res.getType() == AnyType.tType.RT_BOOL) && res.getBool())
        retVal = true;

      return retVal;
    }

    public bool apply(FilterItem filter, Csv csv, int row)
    {
      bool retVal = false;
      _expression.setVariable(VAR_SPECIES_MAN, csv.getCell(row, Cols.SPECIES_MAN));
      AnyType res = _expression.parse(filter.Expression);

      if ((res.getType() == AnyType.tType.RT_BOOL) && res.getBool())
        retVal = true;

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
      string retVal = "Variable Name       | File | Call | SumReport |\n" +
                      "--------------------+------+------+-----------+\n";
      foreach(VarListItem v in _expression.Variables.getVarList(false))
      {
        retVal += v.name;
        int len = 20 - v.name.Length;
        while (len > 0)
        {
          retVal += " ";
          len--;
        }
        retVal += "|";
        retVal += findVarItem(v.name).AvailableFile ? "  x   |" : "      |";
        retVal += findVarItem(v.name).AvailableCall ? "  x   |" : "      |";
        retVal += findVarItem(v.name).AvailableSumReport ? "     x     |" : "           |";
        retVal += "\n";
      }
      return retVal;
    }
   
    public static void populateFilterComboBox(ComboBox fiBox, ViewModel model)
    {
      fiBox.Items.Clear();
      fiBox.Items.Add(MyResources.MainFilterNone);
      foreach (FilterItem f in model.Filter.Items)
      {
        string name = f.Name;
        fiBox.Items.Add(name);
      }
      if (fiBox.Items.Count > 0)
        fiBox.Text = (string)fiBox.Items[0];
    }

    private FilterVarItem findVarItem(string varName)
    {
      FilterVarItem item = new FilterVarItem();
      foreach(FilterVarItem f in _vars)
      {
        if (f.VarName == varName)
        {
          item = f;
          break;
        }
      }
      return item;
    }
  }
}
