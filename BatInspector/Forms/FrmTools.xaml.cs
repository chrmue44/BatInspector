/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2022-11-20
 *   Copyright (C) 2022: Christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/
using System.Windows;
using System.Windows.Controls;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmTools.xaml
  /// </summary>
  public partial class FrmTools : Window
  {
    dlgUpdate _dlgUpdate;
    string _fileName;
    ViewModel _model;

    public FrmTools(dlgUpdate update, string fileName, ViewModel model)
    {
      InitializeComponent();
      _dlgUpdate = update;
      _fileName = fileName;
      _model = model;
      Title = "Tools for " + fileName;
      foreach(ScriptItem s in _model.Settings.Scripts)
      {
        if(s.IsTool)
        {
          Button b = new Button();
          b.Content = s.Description;
          b.Tag = s.Index;
          b.Click += btnCklick;
          b.Margin = new Thickness(10, 2, 10, 2);
          b.Height = 30;
          _sp.Children.Add(b);
        }
      }
    }

    private void btnCklick(object sender, RoutedEventArgs e)
    {
      Button b = e.Source as Button;
      int index = (int)b.Tag;
      if ((index >= 0) && (index < _model.Settings.Scripts.Count))
      {
        if (_model.Settings.Scripts[index].IsTool)
        {
          _model.Scripter.VarList.set("VAR_FILE_NAME", _fileName);
          _model.executeScript(_model.Settings.Scripts[index].Name, false);
          _dlgUpdate();
        }
      }
      this.Close();
    }
  }
}
