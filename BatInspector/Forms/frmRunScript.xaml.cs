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
using Microsoft.Win32;
using System.Windows;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmRunScript.xaml
  /// </summary>
  public partial class frmRunScript : Window
  {
    ViewModel _model;
    public frmRunScript(ViewModel model)
    {
      _model = model;
      InitializeComponent();
      _tbFileName.Text = _model.ScriptName;
    }

    private void _btnSelect_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "Script files (*.scr)|*.scr|All files (*.*)|*.*";
      if (openFileDialog.ShowDialog() == true)
        _tbFileName.Text = openFileDialog.FileName;
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      _model.executeScript(_tbFileName.Text);
      this.Close();
    }

    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnEdit_Click(object sender, RoutedEventArgs e)
    {
      _model.editScript(_tbFileName.Text);
      this.Close();
    }
  }
}
