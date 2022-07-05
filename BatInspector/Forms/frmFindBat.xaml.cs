
using System.Windows;
using BatInspector.Properties;
using BatInspector.Controls;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für frmFindBat.xaml
  /// </summary>
  public partial class frmFindBat : Window
  {
    ViewModel _model;
    double _charFreq;
    double _duration;
    double _callDist;

    public frmFindBat(ViewModel model)
    {
      InitializeComponent();
      _model = model;
      _ctlCharFreq.setup(MyResources.CharFrequency, enDataType.DOUBLE, 1, 120, 120, true, charFreqChanged);
      _ctlDuration.setup(MyResources.Duration, enDataType.DOUBLE, 1, 120, 120, true, durationChanged);
      _ctlCallDist.setup(MyResources.CallDistance, enDataType.DOUBLE, 1, 120, 120, true, callDistChanged);
      _ctlCharFreq.setValue(0.0);
      _ctlDuration.setValue(0.0);
      _ctlCallDist.setValue(0.0);
    }

    void charFreqChanged(enDataType type, object val)
    {
      if (type == enDataType.DOUBLE)
      {
        _charFreq = (double)val;
        getPossibleSpecies();
      }
    }

    void callDistChanged(enDataType type, object val)
    {
      if (type == enDataType.DOUBLE)
      {
        _callDist = (double)val;
        getPossibleSpecies();
      }
    }

    void durationChanged(enDataType type, object val)
    {
      if (type == enDataType.DOUBLE)
      {
        _duration = (double)val;
        getPossibleSpecies();
      }
    }

    void getPossibleSpecies()
    {
       _dgSpecies.ItemsSource = _model.getPossibleSpecies(_charFreq, _duration, _callDist);
    }
  }
}
