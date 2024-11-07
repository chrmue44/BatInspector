/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using BatInspector.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für FrmZoom.xaml
  /// </summary>
  public partial class FrmZoom : Window
  {
    dlgcloseChildWindow _closeWin;

    public System.Windows.Media.ImageSource ImgSource
    {
      set { _ctl._imgFt.Source = value; }
    }
    public FrmZoom(dlgcloseChildWindow dlgClose)
    {
      InitializeComponent();
      _closeWin = dlgClose;
      ContentRendered += FrmZoom_ContentRendered;
    }

    public void setup(string name, AnalysisFile analysis, string wavFilePath,  ctlWavFile ctlWav, dlgVoid openExpWindow, enModel modelType)
    {
      List<string> species = App.Model.CurrentlyOpen == null ? null : App.Model.CurrentlyOpen.Species;
        _ctl.setup(analysis, wavFilePath, species, ctlWav, openExpWindow, modelType);
      this.Title = name;
    }


    private void FrmZoom_ContentRendered(object sender, EventArgs e)
    {
      _ctl.update();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      AppParams.Inst.ZoomWindowHeight = this.Height;
      AppParams.Inst.ZoomWindowWidth = this.Width;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      _closeWin(enWinType.ZOOM);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }
}
