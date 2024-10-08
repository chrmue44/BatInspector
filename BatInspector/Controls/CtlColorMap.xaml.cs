﻿/********************************************************************************
 *               Author: Christian Müller
 *     Date of creation: 2021-08-10                                       
 *   Copyright (C) 2023: Christian Müller chrmue44(at)gmail(dot).com
 *
 *              Licence:  CC BY-NC 4.0 
 ********************************************************************************/
using System.Collections.Generic;
using System.Windows.Controls;

using BatInspector.Properties;
namespace BatInspector.Controls
{
  /// <summary>
  /// Interaktionslogik für CtlColorMap.xaml
  /// </summary>
  public partial class CtlColorMap : UserControl
  {
    public CtlColorMap()
    {
      InitializeComponent();
    }

    public void setup(string color, List<ColorItem> col)
    {
      _grp.Header = color;
      int w = 80;
      _ctlCol1.setup(MyResources.ColorMapColorPt + " 1", enDataType.INT, 0, w, true);
      _ctlCol1._tb.Focusable = true;
      _ctlCol1.setValue(col[0].Color);
      _ctlCol2.setup(MyResources.ColorMapColorPt + " 2", enDataType.INT, 0, w, true);
      _ctlCol2._tb.Focusable = true;
      _ctlCol2.setValue(col[1].Color);
      _ctlCol3.setup(MyResources.ColorMapColorPt + " 3", enDataType.INT, 0, w, true);
      _ctlCol3.setValue(col[2].Color);
      _ctlCol3._tb.Focusable = true;
      _ctlCol4.setup(MyResources.ColorMapColorPt + " 4", enDataType.INT, 0, w, true);
      _ctlCol4.setValue(col[3].Color);
      _ctlCol4._tb.Focusable = true;
      _ctlCol5.setup(MyResources.ColorMapColorPt + " 5", enDataType.INT, 0, w, true);
      _ctlCol5.setValue(col[4].Color);
      _ctlCol5._tb.Focusable = true;

      _ctlVal1.setup(MyResources.ColorMapValuePt + " 1", enDataType.INT, 0, w, true);
      _ctlVal1.setValue(col[0].Value);
      _ctlVal1._tb.Focusable = true;
      _ctlVal2.setup(MyResources.ColorMapValuePt + " 2", enDataType.INT, 0, w, true);
      _ctlVal2.setValue(col[1].Value);
      _ctlVal2._tb.Focusable = true;
      _ctlVal3.setup(MyResources.ColorMapValuePt + " 3", enDataType.INT, 0, w, true);
      _ctlVal3.setValue(col[2].Value);
      _ctlVal3._tb.Focusable = true;
      _ctlVal4.setup(MyResources.ColorMapValuePt + " 4", enDataType.INT, 0, w, true);
      _ctlVal4.setValue(col[3].Value);
      _ctlVal4._tb.Focusable = true;
      _ctlVal5.setup(MyResources.ColorMapValuePt + " 5", enDataType.INT, 0, w, true);
      _ctlVal5.setValue(col[4].Value);
      _ctlVal5._tb.Focusable = true;
    }
  }
}
