/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using libParser;
using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;

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
    MainWindow _parent;
    public frmSpeciesData(ViewModel model, dlgcloseChildWindow closeWin, MainWindow parent)
    {
      _model = model;
      _closWin = closeWin;
      _parent = parent;
      InitializeComponent();
      this.Title = Path.Combine(AppParams.Inst.BatInfoPath,"batinfo.json");
      _ctlSelSpecies1.setup("select species:", 0, 150, 200, species1Changed);
      _ctlSelSpecies1._cb.Items.Clear();
      _ctlSelSpecies2.setup("select species:", 0, 150, 200, species2Changed);
      _ctlSelSpecies2._cb.Items.Clear();
      _ctlSpecData1.setDelegate(showSpecExample);
      _ctlSpecData2.setDelegate(showSpecExample);

      foreach (SpeciesInfos si in _model.SpeciesInfos)
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

    private void showSpecExample(string locSpecName)
    {
      foreach(SpeciesInfos spec in _model.SpeciesInfos)
      {
        if(spec.Local == locSpecName)
        {
          if (spec.WavExample != null)
          {
            string wavName = Path.GetFileName(spec.WavExample);
            try
            {
              string fullName = Path.Combine(AppParams.AppDataPath, spec.WavExample);
              WavFile w = new WavFile();
              w.readFile(fullName);
              double duration = (double)w.AudioSamples.Length / w.FormatChunk.Frequency;
              AnalysisFile ana = new AnalysisFile(fullName, (int)w.FormatChunk.Frequency, duration);
              _parent.setZoom(wavName, ana, Path.GetDirectoryName(fullName), null, enModel.BAT_DETECT2);
            }
            catch(Exception ex) 
            {
              DebugLog.log("unable to open " + wavName + " " + ex.ToString(), enLogType.ERROR);
            }
          }
        }
      }
    }

    private void _btnCacel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      _btnSave_Click(sender, e);
      this.Visibility = Visibility.Hidden;
    }

    private void species1Changed(int idx, string val)
    {
      foreach (SpeciesInfos si in _model.SpeciesInfos)
      {
        if(si.Latin == val)
        {
          _ctlSpecData1._ctlLocalName.setValue(si.Local);
          _ctlSpecData1._ctDuration.setMinValue(si.DurationMin);
          _ctlSpecData1._ctDuration.setMaxValue(si.DurationMax);
          _ctlSpecData1._ctFreqC.setMinValue(si.FreqCharMin);
          _ctlSpecData1._ctFreqC.setMaxValue(si.FreqCharMax);
          _ctlSpecData1._ctCallDist.setMinValue(si.CallDistMin);
          _ctlSpecData1._ctCallDist.setMaxValue(si.CallDistMax);
          _ctlSpecData1._ctlFmin.setMinValue(si.FreqMinMin);
          _ctlSpecData1._ctlFmin.setMaxValue(si.FreqMinMax);
          _ctlSpecData1._ctlFmax.setMinValue(si.FreqMaxMin);
          _ctlSpecData1._ctlFmax.setMaxValue(si.FreqMaxMax);
          _ctlSpecData1._tbProof.Text = si.ProofSpecies;
          _ctlSpecData1._tbHabitat.Text = si.Habitat;
          _ctlSpecData1._tbConfusion.Text = si.ConfusionSpec;
          _ctlSpecData1._tbDistintCalls.Text = si.CharCalls;
          break;
        }
      }
    }

    private void species2Changed(int idx, string val)
    {
      foreach (SpeciesInfos si in _model.SpeciesInfos)
      {
        if (si.Latin == val)
        {
          _ctlSpecData2._ctlLocalName.setValue(si.Local);
          _ctlSpecData2._ctDuration.setMinValue(si.DurationMin);
          _ctlSpecData2._ctDuration.setMaxValue(si.DurationMax);
          _ctlSpecData2._ctFreqC.setMinValue(si.FreqCharMin);
          _ctlSpecData2._ctFreqC.setMaxValue(si.FreqCharMax);
          _ctlSpecData2._ctlFmin.setMinValue(si.FreqMinMin);
          _ctlSpecData2._ctlFmin.setMaxValue(si.FreqMinMax);
          _ctlSpecData2._ctlFmax.setMinValue(si.FreqMaxMin);
          _ctlSpecData2._ctlFmax.setMaxValue(si.FreqMaxMax);
          _ctlSpecData2._ctCallDist.setMinValue(si.CallDistMin);
          _ctlSpecData2._ctCallDist.setMaxValue(si.CallDistMax);
          _ctlSpecData2._tbProof.Text = si.ProofSpecies;
          _ctlSpecData2._tbHabitat.Text = si.Habitat;
          _ctlSpecData2._tbConfusion.Text = si.ConfusionSpec;
          _ctlSpecData2._tbDistintCalls.Text = si.CharCalls;
          break;
        }
      }
    }

    private void _btnSave_Click(object sender, RoutedEventArgs e)
    {
      foreach (SpeciesInfos si in _model.SpeciesInfos)
      {
        if (si.Latin == _ctlSelSpecies1._cb.Text)
        {
          si.DurationMax = _ctlSpecData1._ctDuration.MaxDouble;
          si.DurationMin = _ctlSpecData1._ctDuration.MinDouble;
          si.FreqCharMax = _ctlSpecData1._ctFreqC.MaxDouble;
          si.FreqCharMin = _ctlSpecData1._ctFreqC.MinDouble;
          si.FreqMinMin = _ctlSpecData1._ctlFmin.MinDouble;
          si.FreqMinMax = _ctlSpecData1._ctlFmin.MaxDouble;
          si.FreqMaxMin = _ctlSpecData1._ctlFmax.MinDouble;
          si.FreqMaxMax = _ctlSpecData1._ctlFmax.MaxDouble;
          si.CallDistMin = _ctlSpecData1._ctCallDist.MinDouble;
          si.CallDistMax = _ctlSpecData1._ctCallDist.MaxDouble;
          si.ProofSpecies = _ctlSpecData1._tbProof.Text;
          si.Habitat = _ctlSpecData1._tbHabitat.Text;
          si.ConfusionSpec = _ctlSpecData1._tbConfusion.Text;
          si.CharCalls = _ctlSpecData1._tbDistintCalls.Text;
        }else  if (si.Latin == _ctlSelSpecies2._cb.Text)
        {
          si.DurationMax = _ctlSpecData2._ctDuration.MaxDouble;
          si.DurationMin = _ctlSpecData2._ctDuration.MinDouble;
          si.FreqCharMax = _ctlSpecData2._ctFreqC.MaxDouble;
          si.FreqCharMin = _ctlSpecData2._ctFreqC.MinDouble;
          si.FreqMinMin = _ctlSpecData2._ctlFmin.MinDouble;
          si.FreqMinMax = _ctlSpecData2._ctlFmin.MaxDouble;
          si.FreqMaxMin = _ctlSpecData2._ctlFmax.MinDouble;
          si.FreqMaxMax = _ctlSpecData2._ctlFmax.MaxDouble;
          si.CallDistMin = _ctlSpecData2._ctCallDist.MinDouble;
          si.CallDistMax = _ctlSpecData2._ctCallDist.MaxDouble;
          si.ProofSpecies = _ctlSpecData2._tbProof.Text;
          si.Habitat = _ctlSpecData2._tbHabitat.Text;
          si.ConfusionSpec = _ctlSpecData2._tbConfusion.Text;
          si.CharCalls = _ctlSpecData2._tbDistintCalls.Text;
        }
      }
      BatInfo bi = new BatInfo();
      bi.Species = _model.SpeciesInfos;
      bi.save(AppParams.Inst.BatInfoPath);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      _closWin(enWinType.BAT);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
