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

    public FrmTools(string fileName)
    {
      InitializeComponent();
      _fileName = fileName;
      Title = "Tools for " + fileName;
      foreach(ScriptItem s in AppParams.Inst.ScriptInventory.Scripts)
      {
        if(s.IsTool)
        {
          System.Windows.Controls.Button b = new System.Windows.Controls.Button();
          b.Content = s.Description;
          b.Tag = s.Index;
          b.Click += btnCklick;
          b.Margin = new System.Windows.Thickness(50, 2, 50, 2);
          b.Height = 30;
          _sp.Children.Add(b);
        }
      }
      System.Windows.Controls.Button btnC = new System.Windows.Controls.Button();
      btnC.Content = MyResources.BtnCancel;
      btnC.Tag = -1;
      btnC.Click += btnCklick;
      btnC.Margin = new System.Windows.Thickness(50, 2, 50, 2);
      btnC.Height = 30;
      _sp.Children.Add(btnC);
    }

    private void btnCklick(object sender, RoutedEventArgs e)
    {
    System.Windows.Controls.Button b = e.Source as System.Windows.Controls.Button;
      int index = (int)b.Tag;
      if ((index >= 0) && (index < AppParams.Inst.ScriptInventory.Scripts.Count))
      {
        if (AppParams.Inst.ScriptInventory.Scripts[index].IsTool)
        {
          App.Model.Scripter.VarList.set("VAR_FILE_NAME", _fileName);
          string scriptName = Path.Combine(AppParams.Inst.ScriptInventoryPath, 
                                           AppParams.Inst.ScriptInventory.Scripts[index].Name);
          App.Model.Scripter.runScript(scriptName, false, false);
          this.DialogResult = true;
        }
      }
      this.Close();
    }
  }
}
