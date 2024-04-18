/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using libParser;
using System.Globalization;
using System.Windows.Controls;


namespace BatInspector.Controls
{

  /// <summary>
  /// Interaktionslogik für ctlParamEditItem.xaml
  /// </summary>
  public partial class ctlParamEditItem : UserControl
  {
    enParamType _type = enParamType.MICSCELLANOUS;
    string _valString;
    

   // public bool Focusable { set { _tb.Focusable = value; } get { return _tb.Focusable; } }

    public void setup(string label, enParamType type, int widthLbl = 80 )
    {
      _lbl.Text = label;
      _lbl.Focusable = false;
      _type = type;
      _grd.ColumnDefinitions[0].Width = new System.Windows.GridLength(widthLbl);
      _lbl.Width = widthLbl;
      _cbType.Items.Add(BatInspector.Properties.MyResources.FileSelector);
      _cbType.Items.Add(BatInspector.Properties.MyResources.DirectorySelector);
      _cbType.Items.Add(BatInspector.Properties.MyResources.StringNumber);
      _cbType.Items.Add(BatInspector.Properties.MyResources.Boolean);
      _cbType.SelectedIndex = 2;
    }

    
    public void setValue(string val)
    {
        _tb.Text = val;
        _valString = val;
    }

    public void setType(enParamType type) 
    {
      _type = type;
      switch(type)
      {
        case enParamType.FILE:
          _cbType.SelectedIndex = 0;
          break;
        case enParamType.DIRECTORY:
        _cbType.SelectedIndex = 1;
          break;
        case enParamType.MICSCELLANOUS:
          _cbType.SelectedIndex = 2;
          break;
        case enParamType.BOOL:
          _cbType.SelectedIndex = 3;
          break;

      }
    }

    public string getValue()
    {
          return _valString;
    }

    public enParamType getType()
    {
      switch (_cbType.SelectedIndex)
      {
        case 0:
          _type = enParamType.FILE;
          break;
        case 1:
          _type = enParamType.DIRECTORY;
          break;
        default:
        case 2:
          _type = enParamType.MICSCELLANOUS;
          break;
        case 3:
          _type = enParamType.BOOL;
          break;
      }
      return _type;
    }

    public ctlParamEditItem()
    {
      InitializeComponent();
    }

    private void _tb_TextChanged(object sender, TextChangedEventArgs e)
    {
          _valString = _tb.Text;
    }
  }
}
