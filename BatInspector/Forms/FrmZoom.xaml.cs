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
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für FrmZoom.xaml
  /// </summary>
  public partial class FrmZoom : Window
  {
    ViewModel _model;
    dlgcloseChildWindow _closeWin;

    public System.Windows.Media.ImageSource ImgSource
    {
      set { _ctl._imgFt.Source = value; }
    }
    public FrmZoom(ViewModel model, dlgcloseChildWindow dlgClose)
    {
      InitializeComponent();
      _model = model;
      _closeWin = dlgClose;
      ContentRendered += FrmZoom_ContentRendered;
    }

    public void setup(string name, AnalysisFile analysis, string wavFilePath, System.Windows.Media.ImageSource img)
    {
      _ctl.setup(analysis, wavFilePath, _model, img);
      this.Title = name;
    }




    private void FrmZoom_ContentRendered(object sender, EventArgs e)
    {
      _ctl.update();
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
      _model.KeyPressed = System.Windows.Input.Key.None;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      _model.KeyPressed = e.Key;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      _model.Settings.ZoomWindowHeight = this.Height;
      _model.Settings.ZoomWindowWidth = this.Width;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      _closeWin(enWinType.ZOOM);
    }
  }
}
