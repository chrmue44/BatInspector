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
using System.Windows.Media;
using BatInspector.Controls;
using BatInspector.Properties;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für FrmColorMap.xaml
  /// </summary>
  public partial class FrmColorMap : Window
  {
    ViewModel _model;
    public FrmColorMap(ViewModel model)
    {
      _model = model;
      InitializeComponent();
      _ctlB.setup(MyResources.ColorMapBlue, _model.Settings.ColorGradientBlue);
      _ctlG.setup(MyResources.ColorMapGreen, _model.Settings.ColorGradientGreen);
      _ctlR.setup(MyResources.ColorMapRed, _model.Settings.ColorGradientRed);
    }


    private void readValuesFromScreen()
    {
      _model.Settings.ColorGradientBlue[0].Value = _ctlB._ctlVal1.getIntValue();
      _model.Settings.ColorGradientBlue[1].Value = _ctlB._ctlVal2.getIntValue();
      _model.Settings.ColorGradientBlue[2].Value = _ctlB._ctlVal3.getIntValue();
      _model.Settings.ColorGradientBlue[3].Value = _ctlB._ctlVal4.getIntValue();
      _model.Settings.ColorGradientBlue[4].Value = _ctlB._ctlVal5.getIntValue();

      _model.Settings.ColorGradientBlue[0].Color = _ctlB._ctlCol1.getIntValue();
      _model.Settings.ColorGradientBlue[1].Color = _ctlB._ctlCol2.getIntValue();
      _model.Settings.ColorGradientBlue[2].Color = _ctlB._ctlCol3.getIntValue();
      _model.Settings.ColorGradientBlue[3].Color = _ctlB._ctlCol4.getIntValue();
      _model.Settings.ColorGradientBlue[4].Color = _ctlB._ctlCol5.getIntValue();

      _model.Settings.ColorGradientGreen[0].Value = _ctlG._ctlVal1.getIntValue();
      _model.Settings.ColorGradientGreen[1].Value = _ctlG._ctlVal2.getIntValue();
      _model.Settings.ColorGradientGreen[2].Value = _ctlG._ctlVal3.getIntValue();
      _model.Settings.ColorGradientGreen[3].Value = _ctlG._ctlVal4.getIntValue();
      _model.Settings.ColorGradientGreen[4].Value = _ctlG._ctlVal5.getIntValue();

      _model.Settings.ColorGradientGreen[0].Color = _ctlG._ctlCol1.getIntValue();
      _model.Settings.ColorGradientGreen[1].Color = _ctlG._ctlCol2.getIntValue();
      _model.Settings.ColorGradientGreen[2].Color = _ctlG._ctlCol3.getIntValue();
      _model.Settings.ColorGradientGreen[3].Color = _ctlG._ctlCol4.getIntValue();
      _model.Settings.ColorGradientGreen[4].Color = _ctlG._ctlCol5.getIntValue();

      _model.Settings.ColorGradientRed[0].Value = _ctlR._ctlVal1.getIntValue();
      _model.Settings.ColorGradientRed[1].Value = _ctlR._ctlVal2.getIntValue();
      _model.Settings.ColorGradientRed[2].Value = _ctlR._ctlVal3.getIntValue();
      _model.Settings.ColorGradientRed[3].Value = _ctlR._ctlVal4.getIntValue();
      _model.Settings.ColorGradientRed[4].Value = _ctlR._ctlVal5.getIntValue();

      _model.Settings.ColorGradientRed[0].Color = _ctlR._ctlCol1.getIntValue();
      _model.Settings.ColorGradientRed[1].Color = _ctlR._ctlCol2.getIntValue();
      _model.Settings.ColorGradientRed[2].Color = _ctlR._ctlCol3.getIntValue();
      _model.Settings.ColorGradientRed[3].Color = _ctlR._ctlCol4.getIntValue();
      _model.Settings.ColorGradientRed[4].Color = _ctlR._ctlCol5.getIntValue();
    }

    private void _btnApply_Click(object sender, RoutedEventArgs e)
    {
      readValuesFromScreen();
      _model.ColorTable.createColorLookupTable();
      double w = _cvImg.ActualWidth;
      double h = _cvImg.ActualHeight;
      _cvImg.Children.Clear();
      for (double i = 0; i < w;i++)
      {
        System.Windows.Media.Color color = _model.ColorTable.getSwmColor(i, 0, w);
        CtrlZoom.createLine(_cvImg, i, 0, i, h, new SolidColorBrush(color));
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _btnApply_Click(null, null);
    }
  }
}
