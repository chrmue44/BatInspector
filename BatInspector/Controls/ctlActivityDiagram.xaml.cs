/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2024-08-21                                      
 *   Copyright (C) 2024: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Pen = System.Drawing.Pen;
using Graphics = System.Drawing.Graphics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using libParser;
using BatInspector.Forms;
using BatInspector.Properties;

namespace BatInspector.Controls
{
  /// <summary>
  /// Interaction logic for ctlActivityDiagram.xaml
  /// </summary>
  public partial class ctlActivityDiagram : UserControl
  {
    ActivityDiagram _diagram;
    ViewModel _model = null;
    Bitmap _bmp = null;
    ActivityData _data = null;
    string _bmpName;

    public ctlActivityDiagram()
    {
      InitializeComponent();
    }

    public void setup(ViewModel model)
    {
      _model = model;
      _diagram = new ActivityDiagram(_model.ColorTable);
      int lblW = 150;
      _ctrlTitle.setup(Properties.MyResources.DiagramTitle, enDataType.STRING, 0, lblW, true, textChanged);
      _ctlStyle.setup(Properties.MyResources.ctlActivityDisplayStyle, 0, 80, 100, styleChanged);
      _ctlMaxValue.setup(Properties.MyResources.MaxValue, enDataType.INT, 0, 80, true, textChanged);
      _ctlTimeShift.setup(MyResources.ctlActivityTimeShift, 160, 30, textChanged);
      
      _cbWeek.IsChecked = true;
      _cbDay.IsChecked = true;
      _cbTwilight.IsChecked = true;
      string[] items = new string[3];
      items[0] = BatInspector.Properties.MyResources.ctlActivityColored;
      items[1] = BatInspector.Properties.MyResources.Square;
      items[2] = BatInspector.Properties.MyResources.Circle;
      _ctlStyle.setItems(items);
      _ctlTimeShift.NUDTextBox.Text="0";
    }

    private void textChanged(enDataType type, object val)
    {
      createPlot();
    }

    private void styleChanged(int index, string val)
    {
      createPlot();
    }

    public ActivityDiagram Diagram { get{ return _diagram; } }

    public void createPlot(ActivityData hm, string bmpName)
    {
      _data = hm;
      _bmpName = bmpName;
      createPlot();
    }




    private void createPlot()
    {
      if (_data == null)
        return;
      bool month = _cbMonth.IsChecked == true;
      bool week = _cbWeek.IsChecked == true;
      bool day = _cbDay.IsChecked == true;
      bool twilight = _cbTwilight.IsChecked == true;

      int offsetHours = _ctlTimeShift.Value;

      int maxDispValue = _ctlMaxValue.getIntValue();
      if (maxDispValue == 0)
      {
        maxDispValue = (int)_data.calcMeanValue();
        _ctlMaxValue.setValue(maxDispValue);
      }


      _bmp = _diagram.createPlot(_data, _ctrlTitle.getValue(), _ctlStyle.getSelectedIndex(), maxDispValue, month, week, day, twilight, offsetHours);
      try
      {
        BitmapImage bitmapimage = new BitmapImage();
        using (MemoryStream memory = new MemoryStream())
        {
          _bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
          memory.Position = 0;
          bitmapimage.BeginInit();
          bitmapimage.StreamSource = memory;
          bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
          bitmapimage.EndInit();

        }
        _cnv.Source = bitmapimage;
        _diagram.saveBitMap(_bmpName);
      }
      catch (Exception ex)
      {
        DebugLog.log("unable to create activity diagram: " + ex.ToString(), enLogType.ERROR);
      }
    }

    private void _cbMonth_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      createPlot();
    }
  }
}
