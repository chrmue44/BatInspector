using BatInspector.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für frmSpeciesData.xaml
  /// </summary>
  /// 

  public partial class frmSpeciesData : Window
  {
    ViewModel _model;
    dlgcloseChildWindow _closWin;
    public frmSpeciesData(ViewModel model, dlgcloseChildWindow closeWin)
    {
      _model = model;
      _closWin = closeWin;
      InitializeComponent();
      _ctlSelSpecies1.setup("select species:", 0, 150, 200, species1Changed);
      _ctlSelSpecies1._cb.Items.Clear();
      _ctlSelSpecies2.setup("select species:", 0, 150, 200, species2Changed);
      _ctlSelSpecies2._cb.Items.Clear();
      foreach (SpeciesInfos si in _model.Settings.Species)
      {
        _ctlSelSpecies1._cb.Items.Add(si.Latin);
        _ctlSelSpecies2._cb.Items.Add(si.Latin);
      }
      if (_ctlSelSpecies1._cb.Items.Count > 0)
      {
        _ctlSelSpecies1._cb.SelectedIndex = 0;
        species1Changed(0, _ctlSelSpecies1._cb.Text);
      }
      if (_ctlSelSpecies2._cb.Items.Count > 0)
      {
        _ctlSelSpecies2._cb.SelectedIndex = 0;
        species2Changed(0, _ctlSelSpecies1._cb.Text);
      }
    }


    private void _btnCacel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      _btnSave_Click(sender, e);
      this.Close();
    }

    private void species1Changed(int idx, string val)
    {
      foreach (SpeciesInfos si in _model.Settings.Species)
      {
        if(si.Latin == val)
        {
          _ctlSpecData1._ctlLocalName.setValue(si.Local);
          _ctlSpecData1._ctDuration.setMinValue(si.DurationMin);
          _ctlSpecData1._ctDuration.setMaxValue(si.DurationMax);
          _ctlSpecData1._ctFreqC.setMinValue(si.FreqCharMin);
          _ctlSpecData1._ctFreqC.setMaxValue(si.FreqCharMax);
          _ctlSpecData1._tbProof.Text = si.ProofSpecies;
          _ctlSpecData1._tbHabitat.Text = si.Habitat;
          _ctlSpecData1._tbDistintCalls.Text = si.CharCalls;
          break;
        }
      }
    }

    private void species2Changed(int idx, string val)
    {
      foreach (SpeciesInfos si in _model.Settings.Species)
      {
        if (si.Latin == val)
        {
          _ctlSpecData2._ctlLocalName.setValue(si.Local);
          _ctlSpecData2._ctDuration.setMinValue(si.DurationMin);
          _ctlSpecData2._ctDuration.setMaxValue(si.DurationMax);
          _ctlSpecData2._ctFreqC.setMinValue(si.FreqCharMin);
          _ctlSpecData2._ctFreqC.setMaxValue(si.FreqCharMax);
          _ctlSpecData2._tbProof.Text = si.ProofSpecies;
          _ctlSpecData2._tbHabitat.Text = si.Habitat;
          _ctlSpecData2._tbDistintCalls.Text = si.CharCalls;
          break;
        }
      }
    }

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {
      foreach (SpeciesInfos si in _model.Settings.Species)
      {
        if (si.Latin == _ctlSelSpecies1._cb.Text)
        {
          si.DurationMax = _ctlSpecData1._ctDuration.MaxDouble;
          si.DurationMin = _ctlSpecData1._ctDuration.MinDouble;
          si.FreqCharMax = _ctlSpecData1._ctFreqC.MaxDouble;
          si.FreqCharMin = _ctlSpecData1._ctFreqC.MinDouble;
          si.ProofSpecies = _ctlSpecData1._tbProof.Text;
          si.Habitat = _ctlSpecData1._tbHabitat.Text;
          si.CharCalls = _ctlSpecData1._tbDistintCalls.Text;
        }
        if (si.Latin == _ctlSelSpecies2._cb.Text)
        {
          si.DurationMax = _ctlSpecData2._ctDuration.MaxDouble;
          si.DurationMin = _ctlSpecData2._ctDuration.MinDouble;
          si.FreqCharMax = _ctlSpecData2._ctFreqC.MaxDouble;
          si.FreqCharMin = _ctlSpecData2._ctFreqC.MinDouble;
          si.ProofSpecies = _ctlSpecData2._tbProof.Text;
          si.Habitat = _ctlSpecData2._tbHabitat.Text;
          si.CharCalls = _ctlSpecData2._tbDistintCalls.Text;
        }
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      _closWin(enWinType.BAT);
    }
  }
}
