/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2023-08-18                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/

using BatInspector.Forms;
using libParser;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BatInspector.Properties;
using System.IO;
using libScripter;
using System.Drawing;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using System.Windows.Media.Media3D;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für ctlWavFile.xaml
  /// </summary>
  public partial class ctlWavFile : UserControl, IPool
  {
    AnalysisFile _analysis;
    string _wavFilePath;
    PrjRecord _record;
    MainWindow _parent;
    bool _initialized = false;
    enModel _modelType;
    Sonogram _sonogram = null;
    dlgRelease _dlgRelease = null;

    public string WavFilePath {  get { return _wavFilePath; } }
    public bool WavInit { get { return _initialized; } }
    public AnalysisFile Analysis { get { return _analysis; } }
    public string WavName { get { return _record.File; } }

    public bool InfoVisible
    {
      get { return _grpInfoAuto.Visibility == Visibility.Visible; }
      set
      {
        _grpInfoAuto.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _grpInfoMan.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        _btnCopy.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        _btnTools.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        if (!value)
        {
          _grid.ColumnDefinitions[1].Width = new GridLength(0);
//          _grid.ColumnDefinitions[2].Width = new GridLength(0);
        }
        else
        {
          _grid.ColumnDefinitions[1].Width = new GridLength(360);
  //        _grid.ColumnDefinitions[2].Width = new GridLength(160);
        }
      }
    }

    public ctlWavFile()
    {
    }

    public void setCallBack(dlgRelease dlg)
    {
      _dlgRelease = dlg;
    }

    public void release()
    {
      if (_sonogram != null)
      {
        _sonogram.release();
        _sonogram = null;
      }
      if (_dlgRelease != null)
        _dlgRelease(this);
    }

    public void createNewPng()
    {
      string wavName = Path.Combine(_wavFilePath, WavName);
      _sonogram.createFtImageFromWavFile(wavName, AppParams.FFT_WIDTH, App.Model.ColorTable);
      if ((_sonogram.Image == null) && (App.Model.Prj != null))
        DebugLog.log($"unable to create PNG filefor : {wavName}", enLogType.ERROR);
      else
      {
        _img.Source = _sonogram.Image;
        _img.MaxHeight = MainWindow.MAX_IMG_HEIGHT;
        //Force render
        _img.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
        _img.Arrange(new Rect(_img.DesiredSize));;
      }
    }

    public void setup(AnalysisFile analysis, PrjRecord record, MainWindow parent, bool showButtons)
    {
      _parent = parent;
      InitializeComponent();
      _sonogram = App.Model.View.createSonogram(record.Name);
      _ctlRemarks.setup(MyResources.CtlWavRemarks, enDataType.STRING, 0, 80, true, _tbRemarks_TextChanged);
      _analysis = analysis;
      _record = record;
      _btnCopy.Visibility = showButtons ? Visibility.Visible : Visibility.Collapsed;
      _btnCopy.IsEnabled = showButtons;
      _btnTools.Visibility = showButtons ? Visibility.Visible : Visibility.Collapsed;
      _btnTools.IsEnabled = showButtons;
      _cbSel.Focusable = true;
      _cbSel.IsChecked = record.Selected;
      Visibility = Visibility.Visible;
    }


    public void updateCallInformations(AnalysisFile analysis, PrjRecord rec)
    {
      if (rec == null)
        return;

      _analysis = analysis;
      List<string> spec = new List<string>();
      if (_spDataMan.Children.Count > 0)
      {
        ctlSelectItem ctl = _spDataMan.Children[0] as ctlSelectItem;
        spec = ctl.getItems();
      }
      initCallInformations(spec);
      _cbSel.IsChecked = rec.Selected;
    }

    public void setFileInformations(PrjRecord record, string wavFilePath, AnalysisFile analysis, List<string> spec, enModel modelType, double height)
    {
      _wavFilePath = wavFilePath;
      _record = record;
      _modelType = modelType;
      _cbSel.IsChecked = record.Selected;
      _btnWavFile.Content = _record.File.Replace("_", "__");  //hack, because single '_' shows as underlined char
      //_grp.Header = Name.Replace("_", "__");  //hack, because single '_' shows as underlined char
      _analysis = analysis;
      initCallInformations(spec);
      _initialized = true;
      setHeight(height);
    }

    public void setFocus()
    {
      _cbSel.Focus();
    }

    public void toggleCheckBox()
    {
      _cbSel.IsChecked = !_cbSel.IsChecked;
      _record.Selected = !_record.Selected;
    }

    public void setHeight(double height)
    {
      _grid.RowDefinitions[1].Height = new GridLength(height);
      _img.MaxHeight = height;
      _img.Height = height;
    }


    private void initCallInformations(List<string> spec)
    {
      if (_analysis != null)
      {
        int wLbl = 55;
        int callNr = 1;
        _spDataAuto.Children.Clear();
        _spDataMan.Children.Clear();
        _ctlRemarks.setValue(_analysis.getString(Cols.REMARKS));
        foreach (AnalysisCall call in _analysis.Calls)
        {
          string callStr = call.getString(Cols.NR);
          ctlDataItem it = new ctlDataItem();
          it.Focusable = false;
          it.setup(getLabelStr() + " " + callStr + ": ", enDataType.STRING, 0, wLbl);
          it.setValue(call.getString(Cols.SPECIES) + "(" + ((int)(call.getDouble(Cols.PROBABILITY) * 100 + 0.5)).ToString() + "%)");
          _spDataAuto.Children.Add(it);

          ctlSelectItem im = new ctlSelectItem();
          im.setup(getLabelStr() + " " + callStr + ": ", callNr - 1, wLbl, 90, selItemChanged, clickCallLabel,
                MyResources.ctlWavToolTipCall);
          im.setItems(spec.ToArray());
          im.setValue(call.getString(Cols.SPECIES_MAN));
          if (call.Changed)
            im.setBgColor((SolidColorBrush)App.Current.Resources["colorBackgroundAttn"]);
          _spDataMan.Children.Add(im);
          callNr++;
        }
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      string fName = Path.Combine(_wavFilePath ,_record.File);
      if (File.Exists(fName))
      {
        if (_analysis != null)
        {
          _parent.setZoom(_record.File, _analysis, _wavFilePath, this, App.Model.CurrentlyOpen.Analysis.ModelType);
        }
        else
        {
          AnalysisFile ana = new AnalysisFile(_record.File, 383500, 3.001);
           _parent.setZoom(_record.File, ana, _wavFilePath, this, App.Model.CurrentlyOpen.Analysis.ModelType);
        }
      }
      else
        DebugLog.log("Zoom not possible, file '" + _record.File + "' does not exist", enLogType.ERROR);
    }

    private void selItemChanged(int index, string val)
    {
      if (_analysis != null)
      {
        if ((index >= 0) && (index < _analysis.Calls.Count))
          _analysis.Calls[index].setString(Cols.SPECIES_MAN, val);
        else
          DebugLog.log("ctlWavFile.selItemChanged(): index error", enLogType.ERROR);
      }
    }

    private string getLabelStr()
    {
      switch (_modelType)
      {
        case enModel.BAT_DETECT2:
          return MyResources.CtlWavCall;
        default:
          return MyResources.CtlWavSection;
      }
    }

    private void ctlLostFocus(object sender, RoutedEventArgs e)
    {
      _cbSel.BorderThickness = new Thickness(1, 1, 1, 1);
    }

    private void ctlGotFocus(object sender, RoutedEventArgs e)
    {
      _cbSel.BorderThickness = new Thickness(3, 3, 3, 3);
    }

    public void _btnCopy_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        for (int i = 0; i < _analysis.Calls.Count; i++)
        {
          ctlSelectItem ctlm = _spDataMan.Children[i] as ctlSelectItem;
          if ((_analysis.Calls[i].getDouble(Cols.PROBABILITY) >= 0.5) &&  //TODO no fix value
               SpeciesInfos.isInList(App.Model.SpeciesInfos, Analysis.Calls[i].getString(Cols.SPECIES)))
            ctlm.setValue(Analysis.Calls[i].getString(Cols.SPECIES).ToUpper());
          else
            ctlm.setValue("?");
          _analysis.Calls[i].setString(Cols.SPECIES_MAN, ctlm.getValue());
        }
      }
      catch (Exception ex)
      {
        DebugLog.log("Error copying species: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _tbRemarks_TextChanged(enDataType type, object val)
    {
 //     if (_isSetupCall)
 //       _isSetupCall = false;
   //   else
      {
        if ((type == enDataType.STRING) && (_analysis != null))
          _analysis.setString(Cols.REMARKS, _ctlRemarks.getValue());
      }
    }

    private void clickCallLabel(int index)
    {
      Button_Click(null, null);
      _parent.changeCallInZoom(index);
    }

    public void update()
    {
      updateCallInformations(_analysis, _record);
      this.InvalidateVisual();
      this.UpdateLayout();
    }

    private void _btnTools_Click(object sender, RoutedEventArgs e)
    {
      if (_analysis != null)
      {
        FrmTools frm = new FrmTools(_record.File);
        bool upd = frm.ShowDialog() == true;
        if (upd)
          update();
      }
    }

    private void _cbSel_Click(object sender, RoutedEventArgs e)
    {
      _record.Selected = _cbSel.IsChecked == true;
    }

    private void _btnWavFile_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        string exe = AppParams.Inst.WavTool;
        if (!string.IsNullOrEmpty(exe))
        {
          ProcessRunner p = new ProcessRunner();
          string fName = Path.Combine(WavFilePath, _record.File);
          p.launchCommandLineApp(exe, null, "", true, fName);
        }
      }
      catch(Exception ex) 
      {
        DebugLog.log("error launching WAV tool: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void UserControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
      _parent._scrollPrj_MouseWheel(sender, e);
    }
  }
}
