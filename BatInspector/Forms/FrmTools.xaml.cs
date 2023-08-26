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
using BatInspector.Properties;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmTools.xaml
  /// </summary>
  public partial class FrmTools : Window
  {
    string _fileName;
    ViewModel _model;

    public FrmTools(string fileName, ViewModel model)
    {
      InitializeComponent();
      _fileName = fileName;
      _model = model;
      Title = "Tools for " + fileName;
      foreach(ScriptItem s in AppParams.Inst.Scripts)
      {
        if(s.IsTool)
        {
          Button b = new Button();
          b.Content = s.Description;
          b.Tag = s.Index;
          b.Click += btnCklick;
          b.Margin = new Thickness(50, 2, 50, 2);
          b.Height = 30;
          _sp.Children.Add(b);
        }
      }
      Button btnC = new Button();
      btnC.Content = MyResources.BtnCancel;
      btnC.Tag = -1;
      btnC.Click += btnCklick;
      btnC.Margin = new Thickness(50, 2, 50, 2);
      btnC.Height = 30;
      _sp.Children.Add(btnC);
    }

    private void btnCklick(object sender, RoutedEventArgs e)
    {
      Button b = e.Source as Button;
      int index = (int)b.Tag;
      if ((index >= 0) && (index < AppParams.Inst.Scripts.Count))
      {
        if (AppParams.Inst.Scripts[index].IsTool)
        {
          _model.Scripter.VarList.set("VAR_FILE_NAME", _fileName);
          string scriptName = Path.Combine(AppParams.AppDataPath, 
                                           AppParams.DIR_SCRIPTS,
                                           AppParams.Inst.Scripts[index].Name);
          _model.executeScript(scriptName, false);
          this.DialogResult = true;
        }
      }
      this.Close();
    }
  }
}
