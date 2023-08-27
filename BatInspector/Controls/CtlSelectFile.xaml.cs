using libParser;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;

namespace BatInspector.Controls
{

  public delegate void dlgVoid();
  /// <summary>
  /// Interaktionslogik für CtlSelectFile.xaml
  /// </summary>
  public partial class CtlSelectFile : UserControl
  {
    bool _isFolder = false;
    string _filter = "all files(*.*)|*.*";
    dlgVoid _dlgAction = null;

    public CtlSelectFile()
    {
      InitializeComponent();
    }

    public new bool IsEnabled { get { return _txt.IsEnabled; } set { _txt.IsEnabled = value; _lbl.Opacity = value ? 1.0 : 0.5; } }

    public bool IsFolder { get { return _isFolder; } }

   /* public bool IsEnabled
    {
      get { return _txt.IsEnabled; }
      set { _txt.IsEnabled = value; _btnOpen.IsEnabled = value; }
    }*/

    public void setup(string label, int widthLbl = 80, bool isFolder = false, string filter = "", dlgVoid dlgAction = null)
    {
      _lbl.Text = label;
      _grd.ColumnDefinitions[0].Width = new GridLength(widthLbl);
      _isFolder = isFolder;
      _txt.Text = "";
      _filter = filter;
      _dlgAction = dlgAction;
    }

    public string getValue()
    {
      return _txt.Text; 
    }

    public void setValue(string value) 
    {
      _txt.Text = value;
    }

    private void _btnOpen_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (_isFolder)
        {
          System.Windows.Forms.FolderBrowserDialog ofo = new System.Windows.Forms.FolderBrowserDialog();
          System.Windows.Forms.DialogResult res = ofo.ShowDialog();
          if (res == System.Windows.Forms.DialogResult.OK)
          {
            _txt.Text = ofo.SelectedPath;
          }
        }
        else
        {
          OpenFileDialog ofi = new OpenFileDialog();
          ofi.Filter = _filter;
          bool? ok = ofi.ShowDialog();
          if (ok == true)
          {
            _txt.Text = ofi.FileName;
          }
        }
        if (_dlgAction != null)
          _dlgAction();
      }
      catch (Exception ex)
      {
        DebugLog.log("error file open dialog: " + ex.ToString(), enLogType.ERROR);
      }
    }


    private void _txt_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (_dlgAction != null)
        _dlgAction();
    }
  }
}
