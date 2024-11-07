/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using System.Windows;
using System.Windows.Interop;
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
    public FrmColorMap()
    {
      InitializeComponent();
      _ctlB.setup(MyResources.ColorMapBlue, AppParams.Inst.ColorGradientBlue);
      _ctlG.setup(MyResources.ColorMapGreen, AppParams.Inst.ColorGradientGreen);
      _ctlR.setup(MyResources.ColorMapRed, AppParams.Inst.ColorGradientRed);
    }


    private void readValuesFromScreen()
    {
      AppParams.Inst.ColorGradientBlue[0].Value = _ctlB._ctlVal1.getIntValue();
      AppParams.Inst.ColorGradientBlue[1].Value = _ctlB._ctlVal2.getIntValue();
      AppParams.Inst.ColorGradientBlue[2].Value = _ctlB._ctlVal3.getIntValue();
      AppParams.Inst.ColorGradientBlue[3].Value = _ctlB._ctlVal4.getIntValue();
      AppParams.Inst.ColorGradientBlue[4].Value = _ctlB._ctlVal5.getIntValue();

      AppParams.Inst.ColorGradientBlue[0].Color = _ctlB._ctlCol1.getIntValue();
      AppParams.Inst.ColorGradientBlue[1].Color = _ctlB._ctlCol2.getIntValue();
      AppParams.Inst.ColorGradientBlue[2].Color = _ctlB._ctlCol3.getIntValue();
      AppParams.Inst.ColorGradientBlue[3].Color = _ctlB._ctlCol4.getIntValue();
      AppParams.Inst.ColorGradientBlue[4].Color = _ctlB._ctlCol5.getIntValue();

      AppParams.Inst.ColorGradientGreen[0].Value = _ctlG._ctlVal1.getIntValue();
      AppParams.Inst.ColorGradientGreen[1].Value = _ctlG._ctlVal2.getIntValue();
      AppParams.Inst.ColorGradientGreen[2].Value = _ctlG._ctlVal3.getIntValue();
      AppParams.Inst.ColorGradientGreen[3].Value = _ctlG._ctlVal4.getIntValue();
      AppParams.Inst.ColorGradientGreen[4].Value = _ctlG._ctlVal5.getIntValue();

      AppParams.Inst.ColorGradientGreen[0].Color = _ctlG._ctlCol1.getIntValue();
      AppParams.Inst.ColorGradientGreen[1].Color = _ctlG._ctlCol2.getIntValue();
      AppParams.Inst.ColorGradientGreen[2].Color = _ctlG._ctlCol3.getIntValue();
      AppParams.Inst.ColorGradientGreen[3].Color = _ctlG._ctlCol4.getIntValue();
      AppParams.Inst.ColorGradientGreen[4].Color = _ctlG._ctlCol5.getIntValue();

      AppParams.Inst.ColorGradientRed[0].Value = _ctlR._ctlVal1.getIntValue();
      AppParams.Inst.ColorGradientRed[1].Value = _ctlR._ctlVal2.getIntValue();
      AppParams.Inst.ColorGradientRed[2].Value = _ctlR._ctlVal3.getIntValue();
      AppParams.Inst.ColorGradientRed[3].Value = _ctlR._ctlVal4.getIntValue();
      AppParams.Inst.ColorGradientRed[4].Value = _ctlR._ctlVal5.getIntValue();

      AppParams.Inst.ColorGradientRed[0].Color = _ctlR._ctlCol1.getIntValue();
      AppParams.Inst.ColorGradientRed[1].Color = _ctlR._ctlCol2.getIntValue();
      AppParams.Inst.ColorGradientRed[2].Color = _ctlR._ctlCol3.getIntValue();
      AppParams.Inst.ColorGradientRed[3].Color = _ctlR._ctlCol4.getIntValue();
      AppParams.Inst.ColorGradientRed[4].Color = _ctlR._ctlCol5.getIntValue();
    }

    private void _btnApply_Click(object sender, RoutedEventArgs e)
    {
      readValuesFromScreen();
      App.Model.ColorTable.createColorLookupTable();
      double w = _cvImg.ActualWidth;
      double h = _cvImg.ActualHeight;
      _cvImg.Children.Clear();
      for (double i = 0; i < w;i++)
      {
        System.Windows.Media.Color color = App.Model.ColorTable.getSwmColor(i, 0, w);
        GraphHelper.createLine(_cvImg, i, 0, i, h, new SolidColorBrush(color));
      }
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _btnApply_Click(null, null);
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
