
/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System.Windows;
using BatInspector.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für frmFindBat.xaml
  /// </summary>
  public partial class frmFindBat : Window
  {
    ViewModel _model;
    string strNo = "no";
    string strYes = "yes;";
    string _strUndef = "undef";

    public frmFindBat(ViewModel model)
    {
      string[] items = {strNo, strYes, _strUndef };
      string[] itemsSig =
      {
        enSigStructure.FMa_CF_FMe.ToString(),
        enSigStructure.QCF.ToString(),
        enSigStructure.QCF_FM.ToString(),
        enSigStructure.FM_QCF.ToString(),
        enSigStructure.FM.ToString()
      };

      InitializeComponent();
      
      _model = model;
      _ctlSigStruct.setup("Signal Structure", 0, 120);
      _ctlSigStruct.setItems(itemsSig);
      _ctlSigStruct.setValue(enSigStructure.FM_QCF.ToString());
      _ctlAbsPeak.setup("Absence of Peak", 1, 120);
      _ctlAbsPeak.setItems(items);
      _ctlAbsPeak.setValue(_strUndef);
      _ctlAlternating.setup("Alternarting", 2, 120);
      _ctlAlternating.setItems(items);
      _ctlAlternating.setValue(_strUndef);
      _ctlFinalWhack.setup("Final Whack", 3, 120);
      _ctlFinalWhack.setItems(items);
      _ctlFinalWhack.setValue(_strUndef);
      _ctlHockeyStick.setup("Hockey Stick", 4, 120);
      _ctlHockeyStick.setItems(items);
      _ctlHockeyStick.setValue(_strUndef);
      _ctlNasalSon.setup("Nasal sonority", 5, 120);
      _ctlNasalSon.setItems(items);
      _ctlNasalSon.setValue(_strUndef);
      _ctlProgStart.setup("Progressive Start", 6, 120);
      _ctlProgStart.setItems(items);
      _ctlProgStart.setValue(_strUndef);
      _ctlWhistledSon.setup("Whistled Sonority", 7, 120);
      _ctlWhistledSon.setItems(items);
      _ctlWhistledSon.setValue(_strUndef);
      _ctlExpStart.setup("Explosive Start", 8, 120);
      _ctlExpStart.setItems(items);
      _ctlExpStart.setValue(_strUndef);

      int width = 130;
      int widthTb = 60;
      _ctlLat.setup("Latitude", enDataType.DOUBLE, 6, width,  true);
      _ctlLon.setup("Longitude", enDataType.DOUBLE, 6, width , true);
      _ctlFstart.setup("Start Frequency [kHz]", enDataType.DOUBLE, 1, width,  true);
      _ctlFend.setup("End Frequency [kHz]", enDataType.DOUBLE, 1, width,  true);
      _ctlFME.setup("Freq. max. Energy [kHz]", enDataType.DOUBLE, 1, width,  true);
      _ctlDuration.setup("Duration [ms]", enDataType.DOUBLE, 1, width,  true);
    }

    enBoolDC getBoolDC(int idx)
    {
      switch(idx)
      {
        case 0: 
          return enBoolDC.NO;
        case 1:
          return enBoolDC.YES;
        default:
          return enBoolDC.DONT_CARE;
      }
    }

    private void _btnCheck_Click(object sender, RoutedEventArgs e)
    {
      CallParams pars = new CallParams();
      pars.StartF = _ctlFstart.getDoubleValue();
      pars.EndF = _ctlFend.getDoubleValue();
      pars.FME = _ctlFME.getDoubleValue();
      pars.Duration = _ctlDuration.getDoubleValue();
      pars.Latitude = _ctlLat.getDoubleValue();
      pars.Longitude = _ctlLon.getDoubleValue();
      pars.AbsenceOfPeek = getBoolDC(_ctlAbsPeak.getSelectedIndex());
      pars.Alternating = getBoolDC(_ctlAlternating.getSelectedIndex());
      pars.ExplosiveStart = getBoolDC(_ctlExpStart.getSelectedIndex());
      pars.FinalWack = getBoolDC(_ctlFinalWhack.getSelectedIndex());
      pars.HockeyStick = getBoolDC(_ctlHockeyStick.getSelectedIndex());
      pars.NasalSonority = getBoolDC(_ctlNasalSon.getSelectedIndex());
      pars.WhistledSonority = getBoolDC(_ctlWhistledSon.getSelectedIndex());
      enSigStructure sigStr = enSigStructure.FM_QCF;
      Enum.TryParse(_ctlSigStruct.getValue(), out sigStr);
      pars.SigStructure = sigStr;
      MissingInfo info = new MissingInfo();
      setInfo(info);
      List<string> steps;
      List<enSpec> specs = _model.Classifier.classify(pars, ref info, out steps);
      setInfo(info);
      _tbSpecies.Text = "";
      foreach (enSpec sp in specs)
      {
        _tbSpecies.Text += sp.ToString() + "\n";
      }
      _tbSpecies.Text += "\n\n steps Barataud:\n";
      foreach (string s in steps)
        _tbSpecies.Text += s + "\n";
    }

    void setInfo(MissingInfo info)
    {
      _ctlAbsPeak.setAlert(info.AbsenceOfPeek && (_ctlAbsPeak.getSelectedIndex() == 2));
      _ctlAlternating.setAlert(info.Alternating && (_ctlAlternating.getSelectedIndex() == 2));
      _ctlExpStart.setAlert(info.ExplosiveStart && (_ctlExpStart.getSelectedIndex() == 2));
      _ctlFinalWhack.setAlert(info.FinalWack && (_ctlFinalWhack.getSelectedIndex() == 2));
      _ctlHockeyStick.setAlert(info.HockeyStick && (_ctlHockeyStick.getSelectedIndex() == 2));
      _ctlNasalSon.setAlert(info.NasalSonority && (_ctlNasalSon.getSelectedIndex() == 2));
      _ctlWhistledSon.setAlert(info.WhistledSonority && ( _ctlWhistledSon.getSelectedIndex() == 2));
      _ctlProgStart.setAlert(info.ProgressiveStart && (_ctlProgStart.getSelectedIndex() == 2));
    }

    private void _btnReset_Click(object sender, RoutedEventArgs e)
    {
      _ctlAbsPeak.setValue(_strUndef);
      _ctlAlternating.setValue(_strUndef);
      _ctlExpStart.setValue(_strUndef);
      _ctlFinalWhack.setValue(_strUndef);
      _ctlHockeyStick.setValue(_strUndef);
      _ctlNasalSon.setValue(_strUndef);
      _ctlWhistledSon.setValue(_strUndef);
      _ctlProgStart.setValue(_strUndef);
    }

    private void _btnGet_Click(object sender, RoutedEventArgs e)
    {
      AnalysisFile a = _model.Prj.Analysis.getAnalysis(_model.ZoomView.FileInfo.FileName);
      if (a != null)
      {
        int idx = _model.ZoomView.SelectedCallIdx;
        if ((idx >= 0) && (idx < a.Calls.Count))
        {
          AnalysisCall c = a.Calls[idx];
          ElekonInfoFile.parsePosition(_model.ZoomView.FileInfo, out double lat, out double lon);
          _ctlLat.setValue(lat);
          _ctlLon.setValue(lon);
          _ctlFstart.setValue(c.getDouble(Cols.F_MAX) / 1000.0);
          _ctlFend.setValue(c.getDouble(Cols.F_MIN) / 1000.0);
          _ctlFME.setValue(c.getDouble(Cols.F_MAX_AMP) / 1000.0);
          _ctlDuration.setValue(c.getDouble(Cols.DURATION));
          double[] f = c.getFreqPoints();
          enSigStructure sig = ClassifierBarataud.getSigStructure(f);
          _ctlSigStruct.setValue(sig.ToString());
        }
      }
    }
  }
}
