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

    public ctlActivityDiagram()
    {
      InitializeComponent();
    }

    public void setup(ViewModel model)
    {
      _model = model;
      _diagram = new ActivityDiagram(_model.ColorTable);
      int lblW = 150;
      _ctrlTitle.setup(BatInspector.Properties.MyResources.DiagramTitle, enDataType.STRING, 0, lblW, true);
      _ctlStyle.setup(BatInspector.Properties.MyResources.ctlActivityDisplayStyle, 0, 80, 100);
      string[] items = new string[3];
      items[0] = BatInspector.Properties.MyResources.ctlActivityColored;
      items[1] = BatInspector.Properties.MyResources.Square;
      items[2] = BatInspector.Properties.MyResources.Circle;
      _ctlStyle.setItems(items);
    }



    public void createPlot(ActivityData hm, bool month, bool week, bool day, bool twilight)
    {
      _bmp = _diagram.createPlot(hm, _ctrlTitle.getValue(), _ctlStyle.getSelectedIndex(), month, week, day, twilight);
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
      }
      catch (Exception ex)
      {
        DebugLog.log("unable to create activity diagram: " + ex.ToString(), enLogType.ERROR);
      }
    }

    public void saveBitMap(string fileName)
    {
      if (_bmp != null)
      {
        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Bmp;
        _bmp.Save(fileName, format);
        DebugLog.log($"activity diagram '{fileName}' successfully exported", enLogType.INFO);
      }
    }
  }
}
