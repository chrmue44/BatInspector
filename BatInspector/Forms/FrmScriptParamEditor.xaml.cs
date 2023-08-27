using BatInspector.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BatInspector.Forms
{
  /// <summary>
  /// Interaction logic for FrmScriptParamEditor.xaml
  /// </summary>
  public partial class FrmScriptParamEditor : Window
  {
    public List<string> Parameter { get; private set; }
    public FrmScriptParamEditor(List<string> paramTexts, string scriptName)
    {
      this.Title = BatInspector.Properties.MyResources.FrmScriptParamEditorTitle + ": " + scriptName;
      InitializeComponent();
      _ctlParCnt.setup("Number of Parameters", Controls.enDataType.INT, 0, 150, true, setNrParameter);
      Parameter = new List<string>();
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
            Parameter.Add("");
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
        ctlDataItem di = new ctlDataItem();
        di.setup(BatInspector.Properties.MyResources.FrmScriptParamEditorNameParam + (i + 1).ToString(), enDataType.STRING, 0, 150, true);
        di.setValue(Parameter[i]);
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
        ctlDataItem it = _sp.Children[i] as ctlDataItem;
        Parameter[i] = it.getValue();
      }
      this.Close();
    }
  }
}
