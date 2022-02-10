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
    public frmSpeciesData(ViewModel model)
    {
      _model = model;
      InitializeComponent();
      _ctlSelSpecies.setup("select specieas:", 0, 150, 200, speciesChanged);
      _ctlSelSpecies._cb.Items.Clear();
      foreach (SpeciesInfos si in _model.Settings.Species)
      {
        _ctlSelSpecies._cb.Items.Add(si.Latin);
      }
      if (_ctlSelSpecies._cb.Items.Count > 0)
      {
        _ctlSelSpecies._cb.SelectedIndex = 0;
        speciesChanged(0, _ctlSelSpecies._cb.Text);
      }
    }


    private void _btnCacel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {

    }

    private void speciesChanged(int idx, string val)
    {
      foreach (SpeciesInfos si in _model.Settings.Species)
      {
        if(si.Latin == val)
        {
          _ctlSpecData._ctlLocalName.setValue(si.Local);
          _ctlSpecData._ctDuration.setMinValue(si.DurationMin);
          _ctlSpecData._ctDuration.setMaxValue(si.DurationMax);
          _ctlSpecData._ctFreqC.setMinValue(si.FreqCharMin);
          _ctlSpecData._ctFreqC.setMaxValue(si.FreqCharMax);
          _ctlSpecData._tbProof.Text = si.ProofSpecies;
          _ctlSpecData._tbHabitat.Text = "todo";
          _ctlSpecData._tbDistintCalls.Text = si.CharCalls;
          break;
        }
      }
    }
  }
}
