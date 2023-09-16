/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-09-01                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.Windows;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmRecovery.xaml
  /// </summary>
  public partial class FrmRecovery : Window
  {
    public FrmRecovery(string prjName)
    {
      InitializeComponent();
      this.Title += ": " + prjName;
    }


    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
      this.Close();
    }
  }
}
