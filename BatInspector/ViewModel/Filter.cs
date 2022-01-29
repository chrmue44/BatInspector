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
    const string VAR_SPECIES_AUTO = "SpeciesAuto";
    const string VAR_SPECIES_MAN = "SpeciesMan";
    const string VAR_FREQ_MAX = "FreqMax";
    const string VAR_FREQ_MIN = "FreqMin";
    const string VAR_DURATION = "DurationCall";
    const string VAR_PROBABILITY = "Probability";
    const string VAR_SNR = "Snr";

    List<FilterItem> _list;
    Expression _expression;
    public List<FilterItem> Items { get { return _list; } }

    public Filter()
    {
      _list = new List<FilterItem>();
      _expression = new Expression();
      _expression.Variables.insert(VAR_SPECIES_AUTO);
      _expression.Variables.insert(VAR_SPECIES_MAN);
      _expression.Variables.insert(VAR_FREQ_MIN);
      _expression.Variables.insert(VAR_FREQ_MAX);
      _expression.Variables.insert(VAR_DURATION);
      _expression.Variables.insert(VAR_PROBABILITY);
      _expression.Variables.insert(VAR_SNR);
    }

    public bool apply(FilterItem filter, AnalysisFile file)
    {
      bool retVal = false;
      if (filter.IsForAllCalls)
        retVal = true;

      foreach (AnalysisCall call in file.Calls)
      {
        _expression.setVariable(VAR_SPECIES_AUTO, call.SpeciesAuto);
        _expression.setVariable(VAR_SPECIES_MAN, call.SpeciesMan);
        _expression.setVariable(VAR_FREQ_MIN, call.FreqMin);
        _expression.setVariable(VAR_FREQ_MIN, call.FreqMax);
        _expression.setVariable(VAR_DURATION, call.Duration);
        _expression.setVariable(VAR_PROBABILITY, call.Probability);
        _expression.setVariable(VAR_SNR, call.Snr);
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
  }
}
