/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-27                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for frmParams.xaml
  /// </summary>
  public partial class frmScriptParams : Window
  {
    List<string> _paramVals;
    List<ParamItem> _paramText;

    public List<string> ParameterValues { get { return _paramVals; } }
    public frmScriptParams(string title, List<ParamItem> paramTexts)
    {
      InitializeComponent();
      _paramText = paramTexts;
      this.Title = title;
      _paramVals = new List<string>();
      this.Height = 120;
      for(int i = 0; i <  _paramText.Count; i++)
      {
        CtlSelectFile ctl = new CtlSelectFile();
        switch (_paramText[i].Type)
        {
          case enParamType.FILE:
            ctl.setup(_paramText[i].Name, 150, false, "", checkParams);
            ctl.Margin = new Thickness(2, 2, 0, 0);
            _sp.Children.Add(ctl);
            break;
          case enParamType.DIRECTORY:
            ctl.setup(_paramText[i].Name, 150, true, "", checkParams);
            ctl.Margin = new Thickness(2, 2, 0, 0);
            _sp.Children.Add(ctl);
            break;
          case enParamType.MICSCELLANOUS:
            ctlDataItem ctld = new ctlDataItem();
            ctld.setup(_paramText[i].Name, enDataType.STRING, 0, 150, true, checkMiscParams);
            ctld.Margin = new Thickness(2, 2, 0, 0);
            ctld.setValue("");
            _sp.Children.Add(ctld);
            break;
          case enParamType.BOOL:
            CheckBox chk = new CheckBox();
            chk.Content = _paramText[i].Name;
            _sp.Children.Add(chk);
            break;
        }
        this.Height += 35;
      }
      _btnOk.IsEnabled = false;
    }

    private void checkMiscParams(enDataType type, object val)
    {
      checkParams();
    }

    private void checkParams()
    {
      bool en = true;
      for(int i = 0; i < _sp.Children.Count; i++)
      {
        switch (_paramText[i].Type)
        {
          case enParamType.DIRECTORY:
          case enParamType.FILE:
            CtlSelectFile ctl = _sp.Children[i] as CtlSelectFile;
            if (ctl.getValue().Length == 0)
            {
              en = false;
              break;
            }
            break;
          case enParamType.MICSCELLANOUS:
            ctlDataItem ctld = _sp.Children[i] as ctlDataItem;
            if (ctld.getValue().Length == 0)
            {
              en = false;
              break;
            }
            break;
          case enParamType.BOOL:
            break;        
        }
      }
      _btnOk.IsEnabled = en;
    }
    private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      this.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      for(int i = 0; i < _paramText.Count; i++) 
      {
        switch (_paramText[i].Type)
        {
          case enParamType.FILE:
          case enParamType.DIRECTORY:
            CtlSelectFile ctl = _sp.Children[i] as CtlSelectFile;
            _paramVals.Add(ctl.getValue());
            break;
          case enParamType.MICSCELLANOUS:
            ctlDataItem ctld = _sp.Children[i] as ctlDataItem;
            _paramVals.Add(ctld.getValue());
            break;
          case enParamType.BOOL: 
            CheckBox chk = _sp.Children[i] as CheckBox;
            string boolVal = "0";
            if((chk != null) && (chk.IsChecked == true))
              boolVal = "1";
            _paramVals.Add(boolVal);
            break;
        }
      }
      DialogResult = true;
      this.Close();
    }
  }
}
