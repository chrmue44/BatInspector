/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using libParser;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BatInspector.Controls
{

  /// <summary>
  /// Interaktionslogik für ctlDataItem.xaml
  /// </summary>
  public partial class ctlSelectItem : System.Windows.Controls.UserControl
  {
    string _valString = ""  ;
    dlgSelItemChanged? _dlgValChange = null;
    int _index;
    System.Windows.Media.Brush _brushDefault = System.Windows.Media.Brushes.Black;
    dlgClickLabel? _dlgClickLabel;

    public int SelectIndex { get { return _cb.SelectedIndex; } set { _cb.SelectedIndex = value; } }    

   // public bool Focusable { set { _tb.Focusable = value; } get { return _tb.Focusable; } }

    public void setup(string label, int index, int widthLbl = 80, int widthTb = 80,
                      dlgSelItemChanged? dlgValChange = null, dlgClickLabel? dlgClick = null, string tooltip = "", bool edit = true)
    {
      _lbl.Text = label;
      _lbl.Focusable = false;
      _lbl.ToolTip = tooltip;
      _dlgValChange = dlgValChange;
      _dlgClickLabel = dlgClick;
      _index = index;
      _lbl.Width = widthLbl;
      _cb.Width = widthTb;
      _cb.IsEnabled = edit;
      _brushDefault = _lbl.Background;
    }

    public new bool IsEnabled
    {
      get { return _cb.IsEnabled; }
      set{ _cb.IsEnabled = value; }
    }

    public int ItemCount { get { return _cb.Items.Count; } }

    public void setAlert(bool on)
    {
      if (on)
      {
        _lbl.Background = System.Windows.Media.Brushes.DarkOrange;
      }
      else
        _lbl.Background = _brushDefault;
    }

    public void setValue(string val)
    {
      _valString = val;
      _cb.Text = val;
      foreach(object o in _cb.Items)
      {
        if(o.ToString() == val)
        {
          _cb.SelectedItem = o;
          break;
        }
      }
    }

    public void setItems(string[] items)
    {
      /*
      if (items != null)
      {
        _cb.Items.Clear();
        foreach (string it in items)
          _cb.Items.Add(it);
      }*/
      if(_cb.ItemsSource == null)
        _cb.Items.Clear();
      _cb.ItemsSource = items;
      _cb.SelectedIndex = 0;
    }


    public string getValue()
    {
      return _cb.Items[_cb.SelectedIndex].ToString() ?? ""; 
    }

    public int getSelectedIndex()
    {
      return _cb.SelectedIndex;
    }

    public ctlSelectItem()
    {
      InitializeComponent();
   
    }

    public void setBgColor(SolidColorBrush color)
    {
      if (_cb.IsLoaded)
      {
        ApplyColor();
      }
      else
      {
        _cb.Loaded += (s, e) => ApplyColor();
      }

      void ApplyColor()
      {
        var border = FindVisualChild<Border>(_cb);
        if (border != null)
        {
          border.Background = color;
        }
      }
    }


    private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is T typedChild) return typedChild;
        var result = FindVisualChild<T>(child);
        if (result != null) return result;
      }
      return null;
    }


    public void setFontBold(bool bold)
    {
      if (bold)
      {
        _cb.FontWeight = FontWeights.Bold;
        _lbl.FontWeight = FontWeights.Bold;
      }
      else
      {
        _cb.FontWeight = FontWeights.Normal;
        _lbl.FontWeight = FontWeights.Normal;
      }
    }

    private void _cb_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
/*
      if ( IsVisible && (_cb.SelectedIndex >= 0))
      {
        _valString = _cb.Items[_cb.SelectedIndex].ToString();
        if (_dlgValChange != null)
          _dlgValChange(_index, _valString);
      }
      */
    }

    private void _lbl_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        if (_dlgClickLabel != null)
          _dlgClickLabel(_index);
      }
      catch (Exception ex)
      {
        DebugLog.log($"_lbl_MouseDown: {ex.ToString()}", enLogType.ERROR);
      }
    }

    private void _cb_DropDownClosed(object sender, System.EventArgs e)
    {
      try
      {
        if (IsVisible && (_cb.SelectedIndex >= 0))
        {
          _valString = _cb.Items[_cb.SelectedIndex].ToString() ?? "";
          if (_dlgValChange != null)
            _dlgValChange(_index, _valString);
        }
      }
      catch(Exception ex)
      {
        DebugLog.log($"_cb_DropDownClosed: {ex.ToString()}", enLogType.ERROR);
      }
    }
  }

  public enum enDataType
  {
    STRING,
    DOUBLE,
    INT,
    UINT
  }

  public delegate void dlgSelItemChanged(int index, string val);
  public delegate void dlgClickLabel(int index);

}
