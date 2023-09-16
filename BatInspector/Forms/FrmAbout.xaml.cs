/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/ 

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaktionslogik für FrmAbout.xaml
  /// </summary>
  public partial class FrmAbout : Window
  {
    public FrmAbout(string version)
    {
      InitializeComponent();
      _tbVersion.Text = version;
      try
      {
        _tbLicences.Text = File.ReadAllText("Licenses.txt");
        _tbHistory.Text = File.ReadAllText("versions.txt");
      }
      catch { }
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      winUtils.hideCloseButton(new WindowInteropHelper(this).Handle);
    }
  }

}
