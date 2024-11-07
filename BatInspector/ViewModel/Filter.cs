/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Properties;
using libParser;
using libScripter;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BatInspector
{
  public class FilterVarItem
  {
    public string VarName { get; set; }
    public bool AvailableFile { get; set; } 
    public bool AvailableCall { get;set; }
    public bool AvailableSumReport { get; set; }
    public AnyType.tType Type { get; set; }
    public string Help { get; set; }

    public FilterVarItem()
    {

    }
    public FilterVarItem(string varName, AnyType.tType type, bool availableFile, bool availableCall,
                         bool availableSumReport, string help)
    {
      VarName = varName;
      AvailableFile = availableFile;
      AvailableCall = availableCall;
      AvailableSumReport = availableSumReport;
      Type = type;
      Help = help;
    }
  }

  public delegate void dlgDelete(int index);
  public class FilterItem
  {
    public int Index { get; set; }
    public string Name { get; set; }
    public string Expression { get; set; }
    public bool IsForAllCalls { get; set; }
  
    public FilterItem(int index, string name, string expression, bool allCalls)
    {
      Index = index;
      Name = name;
      Expression = expression;
      IsForAllCalls = allCalls;
    }
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
    const string VAR_REMARKS = "Remarks";
    const string VAR_TIME = "RecTime";

    ExpressionGenerator _gen;
    List<FilterItem> _list;
    Expression _expression;
   
    public List<FilterItem> Items { get { return _list; } }
    public ExpressionGenerator ExpGenerator { get{ return _gen; } }

    public FilterItem TempFilter = null;

    List<FilterVarItem> _vars;

    public Filter(List<string> species)
    {
      _list = new List<FilterItem>();
      _expression = new Expression(null);
      _vars = new List<FilterVarItem>
      {
        new FilterVarItem (VAR_NR_CALLS, AnyType.tType.RT_INT64, true, false, false, BatInspector.Properties.MyResources.FilterHelpVarNrCalls),
        new FilterVarItem (VAR_SPECIES_AUTO, AnyType.tType.RT_STR, true, true, false , BatInspector.Properties.MyResources.FilterHelpVarSpeciesAuto),
        new FilterVarItem (VAR_SPECIES_MAN, AnyType.tType.RT_STR, true, true, true, BatInspector.Properties.MyResources.FilterHelpVarSpeciesMan),
        new FilterVarItem (VAR_FREQ_MIN, AnyType.tType.RT_FLOAT, true, true, false, BatInspector.Properties.MyResources.FilterVarHelpFmin ),
        new FilterVarItem (VAR_FREQ_MAX, AnyType.tType.RT_FLOAT, true, true, false, BatInspector.Properties.MyResources.FilterVarHelpFmax ),
        new FilterVarItem (VAR_FREQ_MAX_AMP, AnyType.tType.RT_FLOAT, true, true, false, BatInspector.Properties.MyResources.FilterVarHelpFmaxAmp ),
        new FilterVarItem (VAR_DURATION, AnyType.tType.RT_FLOAT, true, true, false, BatInspector.Properties.MyResources.FilterVarHelpDuration ),
        new FilterVarItem (VAR_PROBABILITY, AnyType.tType.RT_FLOAT, true, true, false, BatInspector.Properties.MyResources.FilterVarHelpProb ),
        new FilterVarItem (VAR_REMARKS, AnyType.tType.RT_STR, true, true, false , BatInspector.Properties.MyResources.FilterVarHelpRecording),
        new FilterVarItem (VAR_TIME, AnyType.tType.RT_TIME, true, false, false, BatInspector.Properties.MyResources.FilterVarHelpTime)
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

      _gen = new ExpressionGenerator(getVariables, species);
    }

    public bool apply(FilterItem filter, AnalysisCall call, string remarks, string time, out bool ok)
    {
      if (string.IsNullOrEmpty(filter.Expression))
      {
        ok = true;
        return true;
      }

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

      if (string.IsNullOrEmpty(filter.Expression))
        return true;

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
          AnyType res = _expression.parse(filter.Expression);
          if (filter.IsForAllCalls)
          {
            if ((res.getType() != AnyType.tType.RT_BOOL) || !res.getBool())
            {
              retVal = false;
              break;
            }
          }
          else
          {
            if ((res.getType() == AnyType.tType.RT_BOOL) && res.getBool())
            {
              retVal = true;
              break;
            }
          }
        }
      }
      return retVal;
    }

    public bool apply(FilterItem filter, AnalysisCall call)
    {
      if (filter == null)
        return true;

      bool retVal = false;

      _expression.setVariable(VAR_SPECIES_AUTO, call.getString(Cols.SPECIES));
      _expression.setVariable(VAR_SPECIES_MAN, call.getString(Cols.SPECIES_MAN));
      _expression.setVariable(VAR_FREQ_MIN, call.getDouble(Cols.F_MIN));
      _expression.setVariable(VAR_FREQ_MAX, call.getDouble(Cols.F_MAX));
      _expression.setVariable(VAR_DURATION, call.getDouble(Cols.DURATION));
      _expression.setVariable(VAR_PROBABILITY, call.getDouble(Cols.PROBABILITY));
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

    public string getVariablesHelpList()
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

    public List<FilterVarItem> getVariables()
    {
      return _vars;
    }

    public static void populateFilterComboBox(ComboBox fiBox)
    {
      fiBox.Items.Clear();
      fiBox.Items.Add(MyResources.MainFilterNone);
      fiBox.Items.Add(MyResources.MainFilterNew);
      foreach (FilterItem f in App.Model.Filter.Items)
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
