/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2022-11-20
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
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
      foreach(ScriptItem s in AppParams.Inst.ScriptInventory.Scripts)
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
      if ((index >= 0) && (index < AppParams.Inst.ScriptInventory.Scripts.Count))
      {
        if (AppParams.Inst.ScriptInventory.Scripts[index].IsTool)
        {
          _model.Scripter.VarList.set("VAR_FILE_NAME", _fileName);
          string scriptName = Path.Combine(AppParams.Inst.ScriptInventoryPath, 
                                           AppParams.Inst.ScriptInventory.Scripts[index].Name);
          _model.Scripter.runScript(scriptName, false, false);
          this.DialogResult = true;
        }
      }
      this.Close();
    }
  }
}
