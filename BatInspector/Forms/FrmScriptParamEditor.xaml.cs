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


namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmScriptParamEditor.xaml
  /// </summary>
  public partial class FrmScriptParamEditor : Window
  {
    public List<ParamItem> Parameter { get; private set; }
    public FrmScriptParamEditor(List<ParamItem> paramTexts, string scriptName)
    {
      InitializeComponent();
      this.Title = BatInspector.Properties.MyResources.FrmScriptParamEditorTitle + ": " + scriptName;
      _ctlParCnt.setup(BatInspector.Properties.MyResources.FrmScriptParamEditorNrPars, Controls.enDataType.INT, 0, 150, true, setNrParameter);
      Parameter = new List<ParamItem>();
      Parameter.AddRange(paramTexts);
      fillParamControls(Parameter.Count);
      _ctlParCnt.setValue(Parameter.Count);
    }

    private void setNrParameter(enDataType type, object val) 
    {
      if(type == enDataType.INT)
      {
        int nr = _ctlParCnt.getIntValue();
        if(nr > Parameter.Count)
        {
          while (nr > Parameter.Count)
            Parameter.Add(new ParamItem());
        }
        else if(nr < Parameter.Count)
        {
          while ((nr < Parameter.Count)  && (Parameter.Count > 0))
            Parameter.RemoveAt(Parameter.Count - 1); 
        }
        _sp.Children.Clear();
        fillParamControls(nr);
      }
    }

    private void fillParamControls(int nr)
    {
      _sp.Children.Clear();
      for (int i = 0; i < nr; i++)
      {
        ctlParamEditItem di = new ctlParamEditItem();
        di.setup(BatInspector.Properties.MyResources.FrmScriptParamEditorNameParam + " " + (i + 1).ToString(), 
        enParamType.MICSCELLANOUS, 150);
        di.setValue(Parameter[i].Name);
        di.setType(Parameter[i].Type);
        Parameter[i].VarName = "PAR" + (i+1).ToString();
        _sp.Children.Add(di);
      }
    }
  private void _btnCancel_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _btnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      for(int i = 0; i <  _sp.Children.Count; i++) 
      {
        ctlParamEditItem it = _sp.Children[i] as ctlParamEditItem;
        Parameter[i].Name = it.getValue();
        Parameter[i].Type = it.getType();
      }
      this.Close();
    }
  }
}
