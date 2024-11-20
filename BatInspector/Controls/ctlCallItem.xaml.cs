/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BatInspector.Controls
{
 
  public delegate void dlgCallItemChanged(int index, string spec, enCallType callType);

  /// <summary>
  /// Interaktionslogik für ctlDataItem.xaml
  /// </summary>
  public partial class ctlCallItem : UserControl
  {
    string _valString;
    dlgCallItemChanged _dlgValChange = null;
    int _index;
    Brush _brushDefault;
    dlgClickLabel _dlgClickLabel;

    public int SelectIndex { get { return _cbSpec.SelectedIndex; } set { _cbSpec.SelectedIndex = value; } }    

   // public bool Focusable { set { _tb.Focusable = value; } get { return _tb.Focusable; } }

    public void setup(string label, int index, int widthLbl = 80, int widthTbSpec = 80, int widthTbType = 40,
                      dlgCallItemChanged dlgValChange = null, dlgClickLabel dlgClick = null, string tooltip = "", bool edit = true)
    {
      _lbl.Text = label;
      _lbl.Focusable = false;
      _lbl.ToolTip = tooltip;
      _dlgValChange = dlgValChange;
      _dlgClickLabel = dlgClick;
      _index = index;
      _lbl.Width = widthLbl;
      _cbSpec.Width = widthTbSpec;
      _cbType.Width = widthTbType;
      _cbSpec.IsEnabled = edit;
      _cbType.IsEnabled = edit;
      _brushDefault = _lbl.Background;
    }

    public new bool IsEnabled
    {
      get { return _cbSpec.IsEnabled; }
      set{ _cbSpec.IsEnabled = value; }
    }

    public void setAlert(bool on)
    {
      if (on)
      {
        _lbl.Background = Brushes.DarkOrange;
      }
      else
        _lbl.Background = _brushDefault;
    }

    public void setValue(string val)
    {
      _valString = val;
      _cbSpec.Text = val;
      foreach(object o in _cbSpec.Items)
      {
        if(o.ToString() == val)
        {
          _cbSpec.SelectedItem = o;
          break;
        }
      }
    }

    public void setCallType(enCallType callType)
    {
      _cbType.SelectedIndex = (int)callType;
    }


    public void setItemsSpec(string[] items)
    {
      if (items != null)
      {
        _cbSpec.Items.Clear();
        foreach (string it in items)
          _cbSpec.Items.Add(it);
      }
      _cbSpec.SelectedIndex = 0;
    }

    public void setItemsType(string[] items)
    {
      if (items != null)
      {
        _cbType.Items.Clear();
        foreach (string it in items)
          _cbType.Items.Add(it);
      }
      _cbType.SelectedIndex = 0;
    }

    public List<string> getSpecItems()
    {
      List<string> retVal = new List<string>();
      foreach (string it in _cbSpec.Items)
        retVal.Add(it);
      return retVal;
    }

    public string getValue()
    {
      return _valString;
    }

    public int getSelectedIndex()
    {
      return _cbSpec.SelectedIndex;
    }

    public ctlCallItem()
    {
      InitializeComponent();
    }

    public void setBgColor(SolidColorBrush color)
    {
      _cbSpec.Background = color;
      _lbl.Background = color;
    }


    private void _lbl_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (_dlgClickLabel != null)
        _dlgClickLabel(_index);
    }

    private void _cbSpec_DropDownClosed(object sender, System.EventArgs e)
    {
      if (IsVisible && (_cbSpec.SelectedIndex >= 0) && (_cbType.SelectedIndex >= 0))
      {
        _valString = _cbSpec.Items[_cbSpec.SelectedIndex].ToString();
        enCallType callType = (enCallType)_cbType.SelectedIndex;
        if (_dlgValChange != null)
          _dlgValChange(_index, _valString, callType);
      }
    }
  }
}
